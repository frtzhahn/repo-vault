using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using RepoVault.Core.Data;
using RepoVault.Core.GitServer;
using RepoVault.Core.Models;
using RepoVault.Core.Security;
using RepoVault.Core.Services;

namespace RepoVault.Desktop.AppState;

public sealed class AppSession
{
    public AppSession(string? connectionString = null)
    {
        Db = new DbHelper(connectionString);
        Auth = new AuthService(Db);
        Rbac = new RbacGuard(Db);
        Users = new UserService(Db);
        Courses = new CourseService(Db);
        Discussions = new DiscussionService(Db);
        Evaluations = new EvaluationService(Db);
        Contributors = new ContributorService(Db);
        Browser = new RepoBrowser();
        Seeder = new DemoWorkspaceSeeder(Db);

        ReposDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "RepoVault", "repos");
        Repos = new RepoService(Db, ReposDirectory);
    }

    public DbHelper Db { get; }
    public AuthService Auth { get; }
    public RbacGuard Rbac { get; }
    public UserService Users { get; }
    public RepoService Repos { get; }
    public CourseService Courses { get; }
    public DiscussionService Discussions { get; }
    public EvaluationService Evaluations { get; }
    public ContributorService Contributors { get; }
    public RepoBrowser Browser { get; }
    public DemoWorkspaceSeeder Seeder { get; }

    public UserSummaryDto? CurrentUser { get; private set; }
    public bool IsAuthenticated => CurrentUser is not null;
    public GitHttpServer? RunningServer { get; set; }
    public GitHttpServer? Server
    {
        get => RunningServer;
        set => RunningServer = value;
    }

    public int ServerPort { get; set; } = 5050;
    public string ReposDirectory { get; }

    public event EventHandler<PushCompletedEventArgs>? ServerPushCompleted;
    public event EventHandler? ServerStateChanged;

    public void SetCurrentUser(User user)
    {
        CurrentUser = new UserSummaryDto(
            user.UserId, user.IdNumber, user.Username, user.FullName,
            user.Email, user.Role, user.IsActive, user.CreatedAt);
    }

    public User? GetCurrentUserEntity()
    {
        if (CurrentUser is null)
        {
            return null;
        }

        return new User
        {
            UserId = CurrentUser.UserId,
            IdNumber = CurrentUser.IdNumber,
            Username = CurrentUser.Username,
            FullName = CurrentUser.FullName,
            Email = CurrentUser.Email,
            Role = CurrentUser.Role,
            IsActive = CurrentUser.IsActive,
            CreatedAt = CurrentUser.CreatedAt,
        };
    }

    public string BuildCloneUrl(string repoSlug)
    {
        string slug = RepoService.NormalizeSlug(repoSlug);
        if (Server is { IsRunning: true })
        {
            string baseUrl = Server.ServerUrl.TrimEnd('/');
            if (baseUrl.Contains("://+", StringComparison.Ordinal) ||
                baseUrl.Contains("://*", StringComparison.Ordinal))
            {
                string lanIp = LanHelper.GetLanIpAddress();
                return $"http://{lanIp}:{ServerPort}/repos/{slug}.git";
            }

            return $"{baseUrl}/repos/{slug}.git";
        }

        return $"http://127.0.0.1:{ServerPort}/repos/{slug}.git";
    }

    public string GetActiveServerHostDisplay()
    {
        if (Server is { IsRunning: true })
        {
            string baseUrl = Server.ServerUrl;
            if (baseUrl.Contains("://+", StringComparison.Ordinal) ||
                baseUrl.Contains("://*", StringComparison.Ordinal))
            {
                return $"{LanHelper.GetLanIpAddress()}:{ServerPort}";
            }

            return $"127.0.0.1:{ServerPort}";
        }

        return $"127.0.0.1:{ServerPort}";
    }

    public async Task<(bool Ok, bool LoopbackFallback, string? ErrorMessage)> EnsureServerRunningAsync()
    {
        if (RunningServer is { IsRunning: true })
        {
            bool isLoopback = RunningServer.ServerUrl.Contains("127.0.0.1", StringComparison.Ordinal);
            return (true, isLoopback, null);
        }

        bool loopbackFallback = false;
        var server = new GitHttpServer(Db, Auth, Rbac, Repos, ServerPort, $"http://+:{ServerPort}/");
        try
        {
            await server.StartAsync();
        }
        catch (HttpListenerException)
        {
            server.Dispose();
            loopbackFallback = true;

            server = new GitHttpServer(Db, Auth, Rbac, Repos, ServerPort, $"http://127.0.0.1:{ServerPort}/");
            try
            {
                await server.StartAsync();
            }
            catch (HttpListenerException ex)
            {
                server.Dispose();
                return (false, true, ex.Message);
            }
        }

        server.PushCompleted += OnServerPushCompleted;
        RunningServer = server;
        ServerStateChanged?.Invoke(this, EventArgs.Empty);
        return (true, loopbackFallback, null);
    }

    public async Task StopServerAsync()
    {
        GitHttpServer? server = RunningServer;
        if (server is null)
        {
            return;
        }

        server.PushCompleted -= OnServerPushCompleted;
        RunningServer = null;
        try
        {
            await server.StopAsync();
        }
        finally
        {
            server.Dispose();
            ServerStateChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    private void OnServerPushCompleted(object? sender, PushCompletedEventArgs e)
    {
        ServerPushCompleted?.Invoke(this, e);
    }

    public void Reset()
    {
        CurrentUser = null;
        RunningServer = null;
    }
}
