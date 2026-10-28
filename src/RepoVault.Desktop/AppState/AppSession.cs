using System.IO;
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
    public int ServerPort { get; set; } = 5050;
    public string ReposDirectory { get; }

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

    public void Reset()
    {
        CurrentUser = null;
        RunningServer = null;
    }
}
