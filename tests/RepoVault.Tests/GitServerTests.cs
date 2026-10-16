using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Text;
using RepoVault.Core.Data;
using RepoVault.Core.GitServer;
using RepoVault.Core.Models;
using RepoVault.Core.Security;
using RepoVault.Core.Services;

namespace RepoVault.Tests;

public sealed record ClientGitResult(int ExitCode, string Stdout, string Stderr);

public sealed class GitServerTests : IAsyncLifetime
{
    private const string ZeroSha = "0000000000000000000000000000000000000000";

    private readonly DbHelper _db = new();
    private readonly List<string> _tags = new();
    private readonly List<string> _tempDirs = new();

    private string _tag = string.Empty;
    private string _password = string.Empty;
    private User _faculty = new();
    private User _student = new();
    private Course _course = new();
    private GitHttpServer _server = null!;
    private RepoService _catalog = null!;
    private int _port;

    public async Task InitializeAsync()
    {
        _tag = Guid.NewGuid().ToString("N")[..8];
        _tags.Add(_tag);
        _password = "Passw0rd!" + _tag;

        var auth = new AuthService(_db);
        _faculty = await RegisterAsync(auth, "F" + _tag, UserRole.Faculty);
        _student = await RegisterAsync(auth, "S" + _tag, UserRole.Student);

        var courses = new CourseService(_db);
        var (courseOk, courseMsg, course) = await courses.CreateCourseAsync(
            _faculty, "C" + _tag, "Course " + _tag, "CSD", "2026-1st");
        Assert.True(courseOk, courseMsg);
        _course = course!;

        _port = FreePort();
        string root = Path.Combine(Path.GetTempPath(), "repovault-git-" + _tag);
        _tempDirs.Add(root);
        _catalog = new RepoService(_db, root);

        _server = new GitHttpServer(
            _db, auth, new RbacGuard(_db), _catalog, _port);
        await _server.StartAsync();
    }

    public async Task DisposeAsync()
    {
        if (_server is not null)
        {
            await _server.DisposeAsync();
        }

        foreach (string tag in _tags)
        {
            try
            {
                await CleanupAsync(tag);
            }
            catch
            {
            }
        }

        foreach (string dir in _tempDirs)
        {
            try
            {
                if (Directory.Exists(dir))
                {
                    Directory.Delete(dir, true);
                }
            }
            catch
            {
            }
        }
    }

    private async Task CleanupAsync(string tag)
    {
        await using var conn = await _db.CreateOpenConnectionAsync();
        string like = "%" + tag + "%";

        await using (var repos = conn.CreateCommand())
        {
            repos.CommandText = "DELETE FROM Repositories WHERE RepoSlug LIKE @p;";
            repos.Parameters.AddWithValue("@p", like);
            await repos.ExecuteNonQueryAsync();
        }

        await using (var courses = conn.CreateCommand())
        {
            courses.CommandText = "DELETE FROM Courses WHERE CourseCode LIKE @p;";
            courses.Parameters.AddWithValue("@p", like);
            await courses.ExecuteNonQueryAsync();
        }

        await using (var users = conn.CreateCommand())
        {
            users.CommandText = "DELETE FROM Users WHERE Username LIKE @p OR IdNumber LIKE @p;";
            users.Parameters.AddWithValue("@p", like);
            await users.ExecuteNonQueryAsync();
        }
    }

    private static int FreePort()
    {
        var probe = new TcpListener(IPAddress.Loopback, 0);
        probe.Start();
        int port = ((IPEndPoint)probe.LocalEndpoint).Port;
        probe.Stop();
        return port;
    }

    private async Task<User> RegisterAsync(AuthService auth, string prefix, UserRole role)
    {
        var (ok, msg, user) = await auth.RegisterUserAsync(
            "ID" + prefix, "u" + prefix, _password,
            "Name " + prefix, "u" + prefix + "@example.com", role);
        Assert.True(ok, msg);
        return user!;
    }

    private string RepoUrl(string slug, bool withCreds = true) =>
        withCreds
            ? $"http://{_student.Username}:{_password}@127.0.0.1:{_port}/repos/{slug}.git"
            : $"http://127.0.0.1:{_port}/repos/{slug}.git";

    private string RawUrl(string slug) => $"http://127.0.0.1:{_port}/repos/{slug}.git";

    private static async Task<ClientGitResult> GitAsync(string dir, params string[] args)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "git",
            WorkingDirectory = dir,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };
        psi.Environment["GIT_TERMINAL_PROMPT"] = "0";
        psi.Environment["LC_ALL"] = "C";
        foreach (string arg in args)
        {
            psi.ArgumentList.Add(arg);
        }

        using var proc = new Process { StartInfo = psi };
        proc.Start();

        Task<string> stdout = proc.StandardOutput.ReadToEndAsync();
        Task<string> stderr = proc.StandardError.ReadToEndAsync();

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        try
        {
            await proc.WaitForExitAsync(cts.Token);
        }
        catch (OperationCanceledException)
        {
            try
            {
                proc.Kill(true);
            }
            catch
            {
            }

            return new ClientGitResult(-1, string.Empty, "git timed out");
        }

        return new ClientGitResult(proc.ExitCode, await stdout, await stderr);
    }

    private static async Task<string> MakeClientRepoAsync(string dir, string fileName, string content)
    {
        Directory.CreateDirectory(dir);
        Assert.Equal(0, (await GitAsync(dir, "init", "--initial-branch=main", ".")).ExitCode);
        Assert.Equal(0, (await GitAsync(dir, "config", "user.name", "Test")).ExitCode);
        Assert.Equal(0, (await GitAsync(dir, "config", "user.email", "test@test.local")).ExitCode);
        await File.WriteAllTextAsync(Path.Combine(dir, fileName), content);
        Assert.Equal(0, (await GitAsync(dir, "add", ".")).ExitCode);
        ClientGitResult commit = await GitAsync(dir, "commit", "-m", "first commit");
        Assert.Equal(0, commit.ExitCode);

        ClientGitResult sha = await GitAsync(dir, "rev-parse", "HEAD");
        Assert.Equal(0, sha.ExitCode);
        return sha.Stdout.Trim();
    }

    private async Task<RepositoryRecord> MakeServerRepoAsync(string slug)
    {
        var (ok, msg, repo) = await _catalog.CreateRepositoryAsync(
            _faculty, _course.CourseId, slug, "Repo " + slug, null,
            DateTime.UtcNow.AddDays(7));
        Assert.True(ok, msg);

        var contributors = new ContributorService(_db);
        var (assignOk, assignMsg) = await contributors.AssignContributorAsync(
            _faculty, repo!.RepoId, _student.UserId);
        Assert.True(assignOk, assignMsg);

        return repo;
    }

    private static HttpClient AuthedClient(string username, string password)
    {
        var client = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
        string token = Convert.ToBase64String(Encoding.UTF8.GetBytes(username + ":" + password));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", token);
        return client;
    }

    private async Task<List<PushLedgerEntry>> LedgerForAsync(int repoId)
    {
        await using var conn = await _db.CreateOpenConnectionAsync();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT PushId, RepoId, PusherUserId, BranchRef, OldCommitHash, " +
                          "NewCommitHash, ClientIpAddress, PushedAt FROM PushLedger " +
                          "WHERE RepoId = @r ORDER BY PushId;";
        cmd.Parameters.AddWithValue("@r", repoId);

        var rows = new List<PushLedgerEntry>();
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            rows.Add(new PushLedgerEntry
            {
                PushId = Convert.ToInt64(reader["PushId"]),
                RepoId = Convert.ToInt32(reader["RepoId"]),
                PusherUserId = Convert.ToInt32(reader["PusherUserId"]),
                BranchRef = reader["BranchRef"]?.ToString() ?? string.Empty,
                OldCommitHash = reader["OldCommitHash"]?.ToString() ?? string.Empty,
                NewCommitHash = reader["NewCommitHash"]?.ToString() ?? string.Empty,
                ClientIpAddress = reader.IsDBNull(reader.GetOrdinal("ClientIpAddress"))
                    ? null : reader["ClientIpAddress"]?.ToString(),
                PushedAt = (DateTime)reader["PushedAt"],
            });
        }

        return rows;
    }

    [Fact]
    public async Task Advertisement_ReceivePack_Format()
    {
        string slug = "advrecv" + _tag;
        await MakeServerRepoAsync(slug);

        using var anon = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
        using var challenge = await anon.GetAsync(RawUrl(slug) + "/info/refs?service=git-receive-pack");
        Assert.Equal(HttpStatusCode.Unauthorized, challenge.StatusCode);
        Assert.NotNull(challenge.Headers.WwwAuthenticate);

        using var client = AuthedClient(_student.Username, _password);
        using var res = await client.GetAsync(RawUrl(slug) + "/info/refs?service=git-receive-pack");
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        Assert.Equal(
            "application/x-git-receive-pack-advertisement",
            res.Content.Headers.ContentType?.MediaType);

        string body = await res.Content.ReadAsStringAsync();
        Assert.StartsWith("001f# service=git-receive-pack\n0000", body);
    }

    [Fact]
    public async Task Advertisement_UploadPack_Format()
    {
        string slug = "advup" + _tag;
        await MakeServerRepoAsync(slug);

        using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
        using var res = await client.GetAsync(RawUrl(slug) + "/info/refs?service=git-upload-pack");
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        Assert.Equal(
            "application/x-git-upload-pack-advertisement",
            res.Content.Headers.ContentType?.MediaType);

        string body = await res.Content.ReadAsStringAsync();
        Assert.StartsWith("001e# service=git-upload-pack\n0000", body);
    }

    [Fact]
    public async Task Advertisement_BadService_Returns400()
    {
        string slug = "advbad" + _tag;
        await MakeServerRepoAsync(slug);

        using var client = AuthedClient(_student.Username, _password);
        using var res = await client.GetAsync(RawUrl(slug) + "/info/refs?service=git-foo");
        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);

        using var missing = await client.GetAsync(RawUrl(slug) + "/info/refs");
        Assert.Equal(HttpStatusCode.BadRequest, missing.StatusCode);
    }

    [Fact]
    public async Task Push_Unauthenticated_Returns401()
    {
        string slug = "noauth" + _tag;
        await MakeServerRepoAsync(slug);

        using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
        using var content = new ByteArrayContent([]);
        using var res = await client.PostAsync(RawUrl(slug) + "/git-receive-pack", content);
        Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
        Assert.NotNull(res.Headers.WwwAuthenticate);
    }

    [Fact]
    public async Task Push_NonExistentRepo_Returns404()
    {
        using var client = AuthedClient(_student.Username, _password);
        using var content = new ByteArrayContent([]);
        using var res = await client.PostAsync(
            RawUrl("missing" + _tag) + "/git-receive-pack", content);
        Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
    }

    [Fact]
    public async Task Push_LockedRepo_RejectedWithReadableError()
    {
        string slug = "locked" + _tag;
        RepositoryRecord repo = await MakeServerRepoAsync(slug);

        var (lockOk, lockMsg) = await _catalog.SetLockStateAsync(_faculty, repo.RepoId, true);
        Assert.True(lockOk, lockMsg);

        string client = Path.Combine(Path.GetTempPath(), "repovault-cli-locked-" + _tag);
        _tempDirs.Add(client);
        await MakeClientRepoAsync(client, "hello.txt", "hello");

        ClientGitResult push = await GitAsync(client, "push", RepoUrl(slug), "main");
        Assert.NotEqual(0, push.ExitCode);
        Assert.Contains("repository is locked", push.Stderr + push.Stdout);
        Assert.Empty(await LedgerForAsync(repo.RepoId));
    }

    [Fact]
    public async Task Push_TrashedRepo_RejectedWithReadableError()
    {
        string slug = "trashed" + _tag;
        RepositoryRecord repo = await MakeServerRepoAsync(slug);

        var (trashOk, trashMsg) = await _catalog.MoveToRepoBinAsync(_faculty, repo.RepoId);
        Assert.True(trashOk, trashMsg);

        string client = Path.Combine(Path.GetTempPath(), "repovault-cli-trash-" + _tag);
        _tempDirs.Add(client);
        await MakeClientRepoAsync(client, "hello.txt", "hello");

        ClientGitResult push = await GitAsync(client, "push", RepoUrl(slug), "main");
        Assert.NotEqual(0, push.ExitCode);
        Assert.Contains("repository is locked", push.Stderr + push.Stdout);
        Assert.Empty(await LedgerForAsync(repo.RepoId));
    }

    [Fact]
    public async Task EndToEnd_PushCloneSecondPush_And_ForcePushRejected()
    {
        string slug = "e2e" + _tag;
        RepositoryRecord repo = await MakeServerRepoAsync(slug);

        var pushes = new List<PushCompletedEventArgs>();
        _server.PushCompleted += (_, e) => pushes.Add(e);

        string client = Path.Combine(Path.GetTempPath(), "repovault-cli-e2e-" + _tag);
        _tempDirs.Add(client);
        string firstSha = await MakeClientRepoAsync(client, "hello.txt", "hello e2e");

        ClientGitResult push = await GitAsync(client, "push", RepoUrl(slug), "main");
        Assert.Equal(0, push.ExitCode);

        List<PushLedgerEntry> ledger = await LedgerForAsync(repo.RepoId);
        Assert.Single(ledger);
        Assert.Equal(_student.UserId, ledger[0].PusherUserId);
        Assert.Equal("refs/heads/main", ledger[0].BranchRef);
        Assert.Equal(ZeroSha, ledger[0].OldCommitHash);
        Assert.Equal(firstSha, ledger[0].NewCommitHash);
        Assert.Equal("127.0.0.1", ledger[0].ClientIpAddress);

        Assert.Single(pushes);
        Assert.Equal(repo.RepoId, pushes[0].RepoId);
        Assert.Equal(firstSha, pushes[0].NewCommitHash);

        string clone = Path.Combine(Path.GetTempPath(), "repovault-clone-e2e-" + _tag);
        _tempDirs.Add(clone);
        ClientGitResult done = await GitAsync(Path.GetTempPath(), "clone", RepoUrl(slug), clone);
        Assert.Equal(0, done.ExitCode);
        Assert.Equal("hello e2e", await File.ReadAllTextAsync(Path.Combine(clone, "hello.txt")));

        await File.WriteAllTextAsync(Path.Combine(client, "second.txt"), "second file");
        Assert.Equal(0, (await GitAsync(client, "add", ".")).ExitCode);
        Assert.Equal(0, (await GitAsync(client, "commit", "-m", "second commit")).ExitCode);
        ClientGitResult secondSha = await GitAsync(client, "rev-parse", "HEAD");

        ClientGitResult push2 = await GitAsync(client, "push", RepoUrl(slug), "main");
        Assert.Equal(0, push2.ExitCode);

        ledger = await LedgerForAsync(repo.RepoId);
        Assert.Equal(2, ledger.Count);
        Assert.Equal(firstSha, ledger[1].OldCommitHash);
        Assert.Equal(secondSha.Stdout.Trim(), ledger[1].NewCommitHash);

        Assert.Equal(0, (await GitAsync(client, "commit", "--amend", "-m", "rewritten")).ExitCode);
        ClientGitResult forced = await GitAsync(client, "push", "--force", RepoUrl(slug), "main");
        Assert.NotEqual(0, forced.ExitCode);

        Assert.Equal(2, (await LedgerForAsync(repo.RepoId)).Count);
    }
}
