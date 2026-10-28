using System.Diagnostics;
using System.IO.Compression;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using RepoVault.Core.Data;
using RepoVault.Core.Metrics;
using RepoVault.Core.Models;
using RepoVault.Core.Security;
using RepoVault.Core.Services;

namespace RepoVault.Core.GitServer;

public sealed class PushCompletedEventArgs : EventArgs
{
    public int RepoId { get; init; }
    public int PusherUserId { get; init; }
    public string BranchRef { get; init; } = string.Empty;
    public string OldCommitHash { get; init; } = string.Empty;
    public string NewCommitHash { get; init; } = string.Empty;
    public string? ClientIpAddress { get; init; }
    public DateTime PushedAt { get; init; }
}

public sealed class GitHttpServer : IAsyncDisposable, IDisposable
{
    private static readonly Regex Route = new(
        @"^/repos/([a-zA-Z0-9_\-]+)\.git/(info/refs|git-receive-pack|git-upload-pack)$",
        RegexOptions.Compiled);

    private const string LockedPktLine = "001dERR repository is locked\n";
    private const string ReceiveAdvertise = "001f# service=git-receive-pack\n0000";
    private const string UploadAdvertise = "001e# service=git-upload-pack\n0000";

    private readonly DbHelper _db;
    private readonly AuthService _auth;
    private readonly RbacGuard _rbac;
    private readonly RepoService _catalog;
    private readonly CommitLogIndexer _indexer;
    private readonly CodeMetricsScanner _scanner;
    private readonly HttpListener _listener = new();
    private readonly SemaphoreSlim _pushGate = new(1, 1);

    private readonly string _prefix;
    private int _waiting;
    private int _running;
    private Task? _loop;
    private bool _disposed;

    public GitHttpServer(
        DbHelper db, AuthService auth, RbacGuard rbac,
        RepoService catalog, int port = 5050, string? prefix = null)
    {
        _db = db;
        _auth = auth;
        _rbac = rbac;
        _catalog = catalog;
        _indexer = new CommitLogIndexer(db);
        _scanner = new CodeMetricsScanner(db);
        _prefix = prefix ?? $"http://127.0.0.1:{port}/";
        _listener.Prefixes.Add(_prefix);
    }

    public bool IsRunning => _running == 1;

    public int ActiveQueueLength => Volatile.Read(ref _waiting);

    public string ServerUrl => _prefix.TrimEnd('/');

    public event EventHandler<PushCompletedEventArgs>? PushCompleted;

    public event EventHandler<int>? QueueChanged;

    public Task StartAsync(CancellationToken ct = default)
    {
        if (Interlocked.CompareExchange(ref _running, 1, 0) == 1)
        {
            return Task.CompletedTask;
        }

        _listener.Start();
        _loop = AcceptLoopAsync();
        return Task.CompletedTask;
    }

    public async Task StopAsync()
    {
        if (Interlocked.CompareExchange(ref _running, 0, 1) == 0)
        {
            return;
        }

        try
        {
            _listener.Stop();
        }
        catch (ObjectDisposedException)
        {
        }

        if (_loop is not null)
        {
            try
            {
                await _loop.ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is HttpListenerException || ex is ObjectDisposedException)
            {
            }
        }
    }

    private async Task AcceptLoopAsync()
    {
        while (IsRunning)
        {
            HttpListenerContext ctx;
            try
            {
                ctx = await _listener.GetContextAsync().ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is HttpListenerException || ex is ObjectDisposedException)
            {
                break;
            }

            // handle each request on its own task so slow clones never block the loop
            _ = HandleAsync(ctx);
        }
    }

    private async Task HandleAsync(HttpListenerContext ctx)
    {
        try
        {
            await RouteAsync(ctx).ConfigureAwait(false);
        }
        catch
        {
            SendStatus(ctx, 500);
        }
        finally
        {
            try
            {
                ctx.Response.Close();
            }
            catch
            {
            }
        }
    }

    private async Task RouteAsync(HttpListenerContext ctx)
    {
        Match match = Route.Match(ctx.Request.Url?.AbsolutePath ?? string.Empty);
        if (!match.Success)
        {
            SendStatus(ctx, 404);
            return;
        }

        string slug = match.Groups[1].Value;
        string endpoint = match.Groups[2].Value;

        RepositoryRecord? repo = await _catalog.GetBySlugGlobalAsync(slug).ConfigureAwait(false);
        if (repo is null)
        {
            SendStatus(ctx, 404);
            return;
        }

        string svc;
        if (endpoint == "info/refs")
        {
            if (ctx.Request.HttpMethod != "GET")
            {
                SendStatus(ctx, 405);
                return;
            }

            svc = ctx.Request.QueryString["service"] switch
            {
                "git-receive-pack" => "receive-pack",
                "git-upload-pack" => "upload-pack",
                _ => string.Empty,
            };

            if (svc.Length == 0)
            {
                SendStatus(ctx, 400);
                return;
            }
        }
        else
        {
            if (ctx.Request.HttpMethod != "POST")
            {
                SendStatus(ctx, 405);
                return;
            }

            svc = endpoint == "git-receive-pack" ? "receive-pack" : "upload-pack";
        }

        User? pusher = null;
        if (svc == "receive-pack")
        {
            var (ok, _, user) = await _auth
                .AuthenticateBasicHeaderAsync(ctx.Request.Headers["Authorization"]).ConfigureAwait(false);
            if (!ok || user is null)
            {
                ctx.Response.Headers["WWW-Authenticate"] = "Basic realm=\"Repo Vault\"";
                SendStatus(ctx, 401);
                return;
            }

            pusher = user;

            // frozen check before rbac so authed users get the readable pkt-line error
            if (IsFrozen(repo))
            {
                if (endpoint == "info/refs")
                {
                    byte[] err = Encoding.ASCII.GetBytes(LockedPktLine);
                    ctx.Response.StatusCode = 200;
                    ctx.Response.ContentType = "application/x-git-receive-pack-advertisement";
                    await ctx.Response.OutputStream.WriteAsync(err).ConfigureAwait(false);
                    return;
                }

                SendStatus(ctx, 403);
                return;
            }

            if (!await _rbac.CanPushToRepositoryAsync(user, repo.RepoId).ConfigureAwait(false))
            {
                SendStatus(ctx, 403);
                return;
            }
        }
        else if (svc == "upload-pack")
        {
            var (ok, _, user) = await _auth
                .AuthenticateBasicHeaderAsync(ctx.Request.Headers["Authorization"]).ConfigureAwait(false);
            if (!ok || user is null)
            {
                ctx.Response.Headers["WWW-Authenticate"] = "Basic realm=\"Repo Vault\"";
                SendStatus(ctx, 401);
                return;
            }

            if (!await _rbac.CanCloneRepositoryAsync(user, repo.RepoId).ConfigureAwait(false))
            {
                SendStatus(ctx, 403);
                return;
            }
        }

        if (!Directory.Exists(repo.BareRepoDiskPath))
        {
            await GitProcessRunner.InitializeBareRepositoryAsync(repo.BareRepoDiskPath).ConfigureAwait(false);
        }

        if (endpoint == "info/refs")
        {
            await ServeAdvertisementAsync(ctx, svc, repo.BareRepoDiskPath).ConfigureAwait(false);
            return;
        }

        await ServeRpcAsync(ctx, svc, repo, pusher).ConfigureAwait(false);
    }

    private static bool IsFrozen(RepositoryRecord repo) =>
        repo.LifecycleStatus != LifecycleStatus.Active ||
        repo.IsLocked ||
        (repo.DeadlineUtc.HasValue && repo.DeadlineUtc.Value < DateTime.UtcNow);

    private static void SendStatus(HttpListenerContext ctx, int code)
    {
        ctx.Response.StatusCode = code;
    }

    private static async Task ServeAdvertisementAsync(
        HttpListenerContext ctx, string svc, string diskPath)
    {
        string header = svc == "receive-pack" ? ReceiveAdvertise : UploadAdvertise;
        byte[] prefix = Encoding.ASCII.GetBytes(header);

        ctx.Response.StatusCode = 200;
        ctx.Response.ContentType = $"application/x-git-{svc}-advertisement";
        ctx.Response.Headers["Cache-Control"] = "no-cache";
        ctx.Response.Headers["Expires"] = "Fri, 01 Jan 1980 00:00:00 GMT";
        ctx.Response.Headers["Pragma"] = "no-cache";
        await ctx.Response.OutputStream.WriteAsync(prefix).ConfigureAwait(false);

        using var proc = new Process { StartInfo = GitProcessRunner.BuildStartInfo(
            [svc, "--stateless-rpc", "--advertise-refs", diskPath]) };
        proc.Start();
        await proc.StandardOutput.BaseStream.CopyToAsync(ctx.Response.OutputStream).ConfigureAwait(false);
        await proc.WaitForExitAsync().ConfigureAwait(false);
    }

    private async Task ServeRpcAsync(
        HttpListenerContext ctx, string svc, RepositoryRecord repo, User? pusher)
    {
        ctx.Response.StatusCode = 200;
        ctx.Response.ContentType = $"application/x-git-{svc}-result";
        ctx.Response.Headers["Cache-Control"] = "no-cache";
        ctx.Response.SendChunked = true;

        Dictionary<string, string>? before = null;
        if (svc == "receive-pack")
        {
            Interlocked.Increment(ref _waiting);
            QueueChanged?.Invoke(this, ActiveQueueLength);
            await _pushGate.WaitAsync().ConfigureAwait(false);
            Interlocked.Decrement(ref _waiting);
            QueueChanged?.Invoke(this, ActiveQueueLength);

            try
            {
                // re-read the row, faculty may have locked it while we queued
                RepositoryRecord? fresh =
                    await _catalog.GetByIdAsync(repo.RepoId).ConfigureAwait(false);
                if (fresh is null || IsFrozen(fresh))
                {
                    SendStatus(ctx, 403);
                    return;
                }

                repo = fresh;
                before = await GitProcessRunner.SnapshotRefsAsync(repo.BareRepoDiskPath).ConfigureAwait(false);
            }
            catch
            {
                _pushGate.Release();
                throw;
            }
        }

        try
        {
            using var proc = new Process { StartInfo = GitProcessRunner.BuildStartInfo(
                [svc, "--stateless-rpc", repo.BareRepoDiskPath]) };
            proc.Start();

            Stream input = ctx.Request.InputStream;
            GZipStream? gzip = null;
            if (string.Equals(ctx.Request.Headers["Content-Encoding"], "gzip", StringComparison.OrdinalIgnoreCase))
            {
                gzip = new GZipStream(input, CompressionMode.Decompress);
                input = gzip;
            }

            Task pumpIn = PumpStdinAsync(input, proc);
            Task pumpOut = proc.StandardOutput.BaseStream.CopyToAsync(ctx.Response.OutputStream);
            Task<string> pumpErr = proc.StandardError.ReadToEndAsync();

            await Task.WhenAll(pumpIn, pumpOut, pumpErr).ConfigureAwait(false);
            await proc.WaitForExitAsync().ConfigureAwait(false);
            gzip?.Dispose();

            if (svc == "receive-pack" && pusher is not null && before is not null)
            {
                await IngestPushLedgerAsync(ctx, repo, pusher, before).ConfigureAwait(false);
            }
        }
        finally
        {
            if (svc == "receive-pack")
            {
                _pushGate.Release();
            }
        }
    }

    // copy the request body into git stdin, then close it so git flushes output
    private static async Task PumpStdinAsync(Stream input, Process proc)
    {
        try
        {
            await input.CopyToAsync(proc.StandardInput.BaseStream).ConfigureAwait(false);
        }
        catch (IOException)
        {
            // client went away mid-push, git will notice the closed pipe itself
        }
        finally
        {
            try
            {
                proc.StandardInput.Close();
            }
            catch
            {
            }
        }
    }

    private async Task IngestPushLedgerAsync(
        HttpListenerContext ctx, RepositoryRecord repo, User pusher,
        Dictionary<string, string> before)
    {
        Dictionary<string, string> after =
            await GitProcessRunner.SnapshotRefsAsync(repo.BareRepoDiskPath).ConfigureAwait(false);

        string? ip = ctx.Request.RemoteEndPoint?.Address.ToString();
        await using var conn = await _db.CreateOpenConnectionAsync().ConfigureAwait(false);

        foreach (var (branchRef, newSha) in after)
        {
            before.TryGetValue(branchRef, out string? oldSha);
            oldSha ??= GitProcessRunner.ZeroSha;
            if (oldSha == newSha)
            {
                continue;
            }

            await using var cmd = conn.CreateCommand();
            cmd.CommandText = "INSERT INTO PushLedger (RepoId, PusherUserId, BranchRef, OldCommitHash, " +
                              "NewCommitHash, ClientIpAddress, PushedAt) " +
                              "VALUES (@r, @u, @b, @o, @n, @ip, @now);";
            cmd.Parameters.AddWithValue("@r", repo.RepoId);
            cmd.Parameters.AddWithValue("@u", pusher.UserId);
            cmd.Parameters.AddWithValue("@b", branchRef);
            cmd.Parameters.AddWithValue("@o", oldSha);
            cmd.Parameters.AddWithValue("@n", newSha);
            cmd.Parameters.AddWithValue("@ip", (object?)ip ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@now", DateTime.UtcNow);
            await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);

            var evt = new PushCompletedEventArgs
            {
                RepoId = repo.RepoId,
                PusherUserId = pusher.UserId,
                BranchRef = branchRef,
                OldCommitHash = oldSha,
                NewCommitHash = newSha,
                ClientIpAddress = ip,
                PushedAt = DateTime.UtcNow,
            };

            // index commit logs and code metrics before notifying ui subscribers
            await RefreshAfterPushAsync(evt).ConfigureAwait(false);
            PushCompleted?.Invoke(this, evt);
        }
    }

    private async Task RefreshAfterPushAsync(PushCompletedEventArgs e)
    {
        try
        {
            RepositoryRecord? repo = await _catalog.GetByIdAsync(e.RepoId).ConfigureAwait(false);
            if (repo is null)
            {
                return;
            }

            await _indexer.IndexCommitsAsync(e.RepoId, repo.BareRepoDiskPath).ConfigureAwait(false);
            await _scanner.ScanRepositoryMetricsAsync(e.RepoId, repo.BareRepoDiskPath).ConfigureAwait(false);
        }
        catch
        {
            // background refresh must never break push responses
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        await StopAsync().ConfigureAwait(false);
        _listener.Close();
        _pushGate.Dispose();
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        try
        {
            if (IsRunning)
            {
                Interlocked.Exchange(ref _running, 0);
                _listener.Stop();
            }
        }
        catch (ObjectDisposedException)
        {
        }

        _listener.Close();
        _pushGate.Dispose();
    }
}
