using System.Text;
using RepoVault.Core.Data;
using RepoVault.Core.Models;
using RepoVault.Core.Security;

namespace RepoVault.Tests;

public sealed class AuthAndRbacTests
{
    private static string NewTag() => Guid.NewGuid().ToString("N")[..8];

    private static async Task CleanupAsync(DbConnectionFactory db, string tag)
    {
        await using var conn = await db.CreateOpenConnectionAsync();
        string like = "%" + tag + "%";

        await using (var delRepos = conn.CreateCommand())
        {
            delRepos.CommandText = "DELETE FROM Repositories WHERE RepoSlug LIKE @p;";
            delRepos.Parameters.AddWithValue("@p", like);
            await delRepos.ExecuteNonQueryAsync();
        }

        await using (var delCourses = conn.CreateCommand())
        {
            delCourses.CommandText = "DELETE FROM Courses WHERE CourseCode LIKE @p;";
            delCourses.Parameters.AddWithValue("@p", like);
            await delCourses.ExecuteNonQueryAsync();
        }

        await using (var delUsers = conn.CreateCommand())
        {
            delUsers.CommandText = "DELETE FROM Users WHERE Username LIKE @p OR IdNumber LIKE @p;";
            delUsers.Parameters.AddWithValue("@p", like);
            await delUsers.ExecuteNonQueryAsync();
        }
    }

    private static async Task<User> RegisterAsync(
        AuthService auth, string tag, UserRole role, string? password = null)
    {
        var (ok, msg, user) = await auth.RegisterUserAsync(
            "ID" + tag, "u" + tag, password ?? "Passw0rd!" + tag,
            "Name " + tag, "u" + tag + "@example.com", role);
        Assert.True(ok, msg);
        return user!;
    }

    private static async Task<int> CreateCourseAsync(DbConnectionFactory db, int facultyId, string tag)
    {
        await using var conn = await db.CreateOpenConnectionAsync();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "INSERT INTO Courses (CourseCode, CourseTitle, Department, Semester, CreatedByFacultyId) " +
                          "VALUES (@c, @t, @d, @s, @f); SELECT LAST_INSERT_ID();";
        cmd.Parameters.AddWithValue("@c", "C" + tag);
        cmd.Parameters.AddWithValue("@t", "Course " + tag);
        cmd.Parameters.AddWithValue("@d", "CSD");
        cmd.Parameters.AddWithValue("@s", "2026-1st");
        cmd.Parameters.AddWithValue("@f", facultyId);
        object? id = await cmd.ExecuteScalarAsync();
        return Convert.ToInt32(id);
    }

    private static async Task<int> CreateRepoAsync(
        DbConnectionFactory db, int courseId, string tag, string name,
        bool locked, string status, DateTime? deadline)
    {
        await using var conn = await db.CreateOpenConnectionAsync();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "INSERT INTO Repositories (CourseId, RepoSlug, Title, BareRepoDiskPath, " +
                          "DeadlineUtc, IsLocked, LifecycleStatus) " +
                          "VALUES (@c, @s, @t, @p, @d, @l, @st); SELECT LAST_INSERT_ID();";
        cmd.Parameters.AddWithValue("@c", courseId);
        cmd.Parameters.AddWithValue("@s", name + "-" + tag);
        cmd.Parameters.AddWithValue("@t", "Repo " + tag);
        cmd.Parameters.AddWithValue("@p", "/tmp/m2-" + tag + "-" + name + ".git");
        cmd.Parameters.AddWithValue("@d", (object?)deadline ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@l", locked);
        cmd.Parameters.AddWithValue("@st", status);
        object? id = await cmd.ExecuteScalarAsync();
        return Convert.ToInt32(id);
    }

    private static async Task AddContributorAsync(DbConnectionFactory db, int repoId, int studentId)
    {
        await using var conn = await db.CreateOpenConnectionAsync();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "INSERT INTO RepositoryContributors (RepoId, StudentUserId) VALUES (@r, @u);";
        cmd.Parameters.AddWithValue("@r", repoId);
        cmd.Parameters.AddWithValue("@u", studentId);
        await cmd.ExecuteNonQueryAsync();
    }

    private static string BasicHeader(string username, string password) =>
        "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(username + ":" + password));

    [Fact]
    public void HashAndVerify_RoundTrip_Succeeds()
    {
        var (hash, salt, iterations) = PasswordHasher.HashPassword("correct horse battery");
        Assert.True(PasswordHasher.VerifyPassword("correct horse battery", hash, salt, iterations));
    }

    [Fact]
    public void Verify_WrongPassword_Fails()
    {
        var (hash, salt, iterations) = PasswordHasher.HashPassword("right password");
        Assert.False(PasswordHasher.VerifyPassword("wrong password", hash, salt, iterations));
    }

    [Fact]
    public void Verify_TamperedSalt_Fails()
    {
        var (hash, salt, iterations) = PasswordHasher.HashPassword("some password");
        char[] chars = salt.ToCharArray();
        chars[0] = chars[0] == 'A' ? 'B' : 'A';
        Assert.False(PasswordHasher.VerifyPassword("some password", hash, new string(chars), iterations));
        Assert.False(PasswordHasher.VerifyPassword("some password", hash, "not-base64!!!", iterations));
    }

    [Fact]
    public async Task BasicAuth_MissingHeader_Fails()
    {
        var auth = new AuthService(new DbConnectionFactory());
        string?[] headers = [null, "", "Bearer abc123", "Basic"];
        foreach (string? header in headers)
        {
            var (ok, _, _) = await auth.AuthenticateBasicHeaderAsync(header);
            Assert.False(ok);
        }
    }

    [Fact]
    public async Task BasicAuth_MalformedBase64_Fails()
    {
        var auth = new AuthService(new DbConnectionFactory());
        var (ok, _, _) = await auth.AuthenticateBasicHeaderAsync("Basic !!!notbase64!!!");
        Assert.False(ok);
    }

    [Fact]
    public async Task BasicAuth_PasswordWithColons_Succeeds()
    {
        var db = new DbConnectionFactory();
        var auth = new AuthService(db);
        string tag = NewTag();
        try
        {
            User user = await RegisterAsync(auth, tag, UserRole.Student, "p:a:ss:word");
            var (ok, _, found) = await auth.AuthenticateBasicHeaderAsync(BasicHeader(user.Username, "p:a:ss:word"));
            Assert.True(ok);
            Assert.Equal(user.UserId, found!.UserId);
        }
        finally
        {
            await CleanupAsync(db, tag);
        }
    }

    [Fact]
    public async Task BasicAuth_CacheSurvivesUserDeletion()
    {
        var db = new DbConnectionFactory();
        var auth = new AuthService(db);
        string tag = NewTag();
        User user = await RegisterAsync(auth, tag, UserRole.Student);
        try
        {
            string password = "Passw0rd!" + tag;
            var first = await auth.AuthenticateBasicHeaderAsync(BasicHeader(user.Username, password));
            Assert.True(first.Ok);

            // delete the row, the cached credential should still authenticate
            await using var conn = await db.CreateOpenConnectionAsync();
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM Users WHERE UserId = @id;";
            cmd.Parameters.AddWithValue("@id", user.UserId);
            await cmd.ExecuteNonQueryAsync();

            var second = await auth.AuthenticateBasicHeaderAsync(BasicHeader(user.Username, password));
            Assert.True(second.Ok);
            Assert.Equal(user.UserId, second.User!.UserId);
        }
        finally
        {
            await CleanupAsync(db, tag);
        }
    }

    [Fact]
    public async Task Register_Success_ReturnsUser()
    {
        var db = new DbConnectionFactory();
        var auth = new AuthService(db);
        string tag = NewTag();
        try
        {
            var (ok, _, user) = await auth.RegisterUserAsync(
                "ID" + tag, "u" + tag, "Passw0rd!" + tag,
                "Name " + tag, "u" + tag + "@example.com", UserRole.Faculty);
            Assert.True(ok);
            Assert.True(user!.UserId > 0);
            Assert.Equal(UserRole.Faculty, user.Role);
            Assert.True(user.IsActive);
        }
        finally
        {
            await CleanupAsync(db, tag);
        }
    }

    [Fact]
    public async Task Register_DuplicateUsername_Rejected()
    {
        var db = new DbConnectionFactory();
        var auth = new AuthService(db);
        string tag = NewTag();
        try
        {
            await RegisterAsync(auth, tag, UserRole.Student);
            var (ok, msg, _) = await auth.RegisterUserAsync(
                "IDOTHER" + tag, "u" + tag, "Passw0rd!" + tag,
                "Other " + tag, "other" + tag + "@example.com", UserRole.Student);
            Assert.False(ok);
            Assert.Contains("username", msg);
        }
        finally
        {
            await CleanupAsync(db, tag);
        }
    }

    [Fact]
    public async Task Register_DuplicateIdNumber_Rejected()
    {
        var db = new DbConnectionFactory();
        var auth = new AuthService(db);
        string tag = NewTag();
        try
        {
            await RegisterAsync(auth, tag, UserRole.Student);
            var (ok, msg, _) = await auth.RegisterUserAsync(
                "ID" + tag, "uother" + tag, "Passw0rd!" + tag,
                "Other " + tag, "other" + tag + "@example.com", UserRole.Student);
            Assert.False(ok);
            Assert.Contains("id number", msg);
        }
        finally
        {
            await CleanupAsync(db, tag);
        }
    }

    [Fact]
    public async Task Register_DuplicateEmail_Rejected()
    {
        var db = new DbConnectionFactory();
        var auth = new AuthService(db);
        string tag = NewTag();
        try
        {
            await RegisterAsync(auth, tag, UserRole.Student);
            var (ok, msg, _) = await auth.RegisterUserAsync(
                "IDOTHER" + tag, "uother" + tag, "Passw0rd!" + tag,
                "Other " + tag, "u" + tag + "@example.com", UserRole.Student);
            Assert.False(ok);
            Assert.Contains("email", msg);
        }
        finally
        {
            await CleanupAsync(db, tag);
        }
    }

    [Fact]
    public async Task Register_ShortPassword_Rejected()
    {
        var auth = new AuthService(new DbConnectionFactory());
        var (ok, msg, _) = await auth.RegisterUserAsync(
            "IDX", "ux", "short", "Name X", "ux@example.com", UserRole.Student);
        Assert.False(ok);
        Assert.Contains("8", msg);
    }

    [Fact]
    public async Task Authenticate_Success()
    {
        var db = new DbConnectionFactory();
        var auth = new AuthService(db);
        string tag = NewTag();
        try
        {
            User user = await RegisterAsync(auth, tag, UserRole.Student);
            var (ok, _, found) = await auth.AuthenticateAsync(user.Username, "Passw0rd!" + tag);
            Assert.True(ok);
            Assert.Equal(user.UserId, found!.UserId);
        }
        finally
        {
            await CleanupAsync(db, tag);
        }
    }

    [Fact]
    public async Task Authenticate_WrongPassword_Fails()
    {
        var db = new DbConnectionFactory();
        var auth = new AuthService(db);
        string tag = NewTag();
        try
        {
            User user = await RegisterAsync(auth, tag, UserRole.Student);
            var (ok, _, _) = await auth.AuthenticateAsync(user.Username, "wrong password");
            Assert.False(ok);
        }
        finally
        {
            await CleanupAsync(db, tag);
        }
    }

    [Fact]
    public async Task Authenticate_UnknownUser_Fails()
    {
        var auth = new AuthService(new DbConnectionFactory());
        var (ok, _, _) = await auth.AuthenticateAsync("nobody-" + NewTag(), "whatever123");
        Assert.False(ok);
    }

    [Fact]
    public async Task Authenticate_InactiveUser_Rejected()
    {
        var db = new DbConnectionFactory();
        var auth = new AuthService(db);
        string tag = NewTag();
        try
        {
            User user = await RegisterAsync(auth, tag, UserRole.Student);
            await using var conn = await db.CreateOpenConnectionAsync();
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = "UPDATE Users SET IsActive = 0 WHERE UserId = @id;";
            cmd.Parameters.AddWithValue("@id", user.UserId);
            await cmd.ExecuteNonQueryAsync();

            var (ok, msg, _) = await auth.AuthenticateAsync(user.Username, "Passw0rd!" + tag);
            Assert.False(ok);
            Assert.Contains("deactivated", msg);
        }
        finally
        {
            await CleanupAsync(db, tag);
        }
    }

    [Fact]
    public void Faculty_HasAdminPermissions()
    {
        var faculty = new User { UserId = 1, Role = UserRole.Faculty, IsActive = true };
        Assert.True(RbacGuard.CanManageCourses(faculty));
        Assert.True(RbacGuard.CanGradeRepository(faculty));
        Assert.True(RbacGuard.CanModerateComment(faculty));
        Assert.True(RbacGuard.CanManageRepoBin(faculty));
    }

    [Fact]
    public void Student_LacksAdminPermissions()
    {
        var student = new User { UserId = 2, Role = UserRole.Student, IsActive = true };
        Assert.False(RbacGuard.CanManageCourses(student));
        Assert.False(RbacGuard.CanGradeRepository(student));
        Assert.False(RbacGuard.CanModerateComment(student));
        Assert.False(RbacGuard.CanManageRepoBin(student));
    }

    [Fact]
    public void InactiveFaculty_LacksAdminPermissions()
    {
        var faculty = new User { UserId = 1, Role = UserRole.Faculty, IsActive = false };
        Assert.False(RbacGuard.CanManageCourses(faculty));
        Assert.False(RbacGuard.CanGradeRepository(faculty));
        Assert.False(RbacGuard.CanModerateComment(faculty));
        Assert.False(RbacGuard.CanManageRepoBin(faculty));
    }

    [Fact]
    public void CanEditOrDeleteComment_Matrix()
    {
        var faculty = new User { UserId = 1, Role = UserRole.Faculty, IsActive = true };
        var student = new User { UserId = 2, Role = UserRole.Student, IsActive = true };
        var inactive = new User { UserId = 3, Role = UserRole.Student, IsActive = false };

        Assert.True(RbacGuard.CanEditOrDeleteComment(faculty, 2));
        Assert.True(RbacGuard.CanEditOrDeleteComment(student, 2));
        Assert.False(RbacGuard.CanEditOrDeleteComment(student, 1));
        Assert.False(RbacGuard.CanEditOrDeleteComment(inactive, 3));
    }

    private sealed record PushFixture(
        User Faculty, User Member, User Outsider, int OpenRepo, int LockedRepo,
        int ExpiredRepo, int TrashedRepo);

    private static async Task<PushFixture> SetupPushFixtureAsync(DbConnectionFactory db, AuthService auth, string tag)
    {
        User faculty = await RegisterAsync(auth, "F" + tag, UserRole.Faculty);
        User member = await RegisterAsync(auth, "M" + tag, UserRole.Student);
        User outsider = await RegisterAsync(auth, "O" + tag, UserRole.Student);

        int courseId = await CreateCourseAsync(db, faculty.UserId, tag);
        int open = await CreateRepoAsync(db, courseId, tag, "open", false, "Active", DateTime.UtcNow.AddDays(7));
        int locked = await CreateRepoAsync(db, courseId, tag, "locked", true, "Active", DateTime.UtcNow.AddDays(7));
        int expired = await CreateRepoAsync(db, courseId, tag, "expired", false, "Active", DateTime.UtcNow.AddDays(-1));
        int trashed = await CreateRepoAsync(db, courseId, tag, "trashed", false, "Trashed", DateTime.UtcNow.AddDays(7));
        await AddContributorAsync(db, open, member.UserId);

        return new PushFixture(faculty, member, outsider, open, locked, expired, trashed);
    }

    [Fact]
    public async Task Faculty_CanPushToOpenRepo()
    {
        var db = new DbConnectionFactory();
        var auth = new AuthService(db);
        var guard = new RbacGuard(db);
        string tag = NewTag();
        try
        {
            PushFixture fix = await SetupPushFixtureAsync(db, auth, tag);
            Assert.True(await guard.CanPushToRepositoryAsync(fix.Faculty, fix.OpenRepo));
        }
        finally
        {
            await CleanupAsync(db, tag);
        }
    }

    [Fact]
    public async Task ContributorStudent_CanPushToOpenRepo()
    {
        var db = new DbConnectionFactory();
        var auth = new AuthService(db);
        var guard = new RbacGuard(db);
        string tag = NewTag();
        try
        {
            PushFixture fix = await SetupPushFixtureAsync(db, auth, tag);
            Assert.True(await guard.CanPushToRepositoryAsync(fix.Member, fix.OpenRepo));
        }
        finally
        {
            await CleanupAsync(db, tag);
        }
    }

    [Fact]
    public async Task OutsiderStudent_CannotPush()
    {
        var db = new DbConnectionFactory();
        var auth = new AuthService(db);
        var guard = new RbacGuard(db);
        string tag = NewTag();
        try
        {
            PushFixture fix = await SetupPushFixtureAsync(db, auth, tag);
            Assert.False(await guard.CanPushToRepositoryAsync(fix.Outsider, fix.OpenRepo));
        }
        finally
        {
            await CleanupAsync(db, tag);
        }
    }

    [Fact]
    public async Task LockedRepo_RejectsEveryone()
    {
        var db = new DbConnectionFactory();
        var auth = new AuthService(db);
        var guard = new RbacGuard(db);
        string tag = NewTag();
        try
        {
            PushFixture fix = await SetupPushFixtureAsync(db, auth, tag);
            Assert.False(await guard.CanPushToRepositoryAsync(fix.Faculty, fix.LockedRepo));
            Assert.False(await guard.CanPushToRepositoryAsync(fix.Member, fix.LockedRepo));
        }
        finally
        {
            await CleanupAsync(db, tag);
        }
    }

    [Fact]
    public async Task ExpiredDeadline_RejectsPush()
    {
        var db = new DbConnectionFactory();
        var auth = new AuthService(db);
        var guard = new RbacGuard(db);
        string tag = NewTag();
        try
        {
            PushFixture fix = await SetupPushFixtureAsync(db, auth, tag);
            Assert.False(await guard.CanPushToRepositoryAsync(fix.Faculty, fix.ExpiredRepo));
            Assert.False(await guard.CanPushToRepositoryAsync(fix.Member, fix.ExpiredRepo));
        }
        finally
        {
            await CleanupAsync(db, tag);
        }
    }

    [Fact]
    public async Task TrashedRepo_RejectsPush()
    {
        var db = new DbConnectionFactory();
        var auth = new AuthService(db);
        var guard = new RbacGuard(db);
        string tag = NewTag();
        try
        {
            PushFixture fix = await SetupPushFixtureAsync(db, auth, tag);
            Assert.False(await guard.CanPushToRepositoryAsync(fix.Faculty, fix.TrashedRepo));
            Assert.False(await guard.CanPushToRepositoryAsync(fix.Member, fix.TrashedRepo));
        }
        finally
        {
            await CleanupAsync(db, tag);
        }
    }

    [Fact]
    public async Task MissingRepo_ReturnsFalse()
    {
        var db = new DbConnectionFactory();
        var guard = new RbacGuard(db);
        var faculty = new User { UserId = 1, Role = UserRole.Faculty, IsActive = true };
        Assert.False(await guard.CanPushToRepositoryAsync(faculty, int.MaxValue));
    }

    [Fact]
    public async Task InactiveUser_CannotPush()
    {
        var db = new DbConnectionFactory();
        var auth = new AuthService(db);
        var guard = new RbacGuard(db);
        string tag = NewTag();
        try
        {
            PushFixture fix = await SetupPushFixtureAsync(db, auth, tag);
            var quiet = new User
            {
                UserId = fix.Member.UserId,
                Role = UserRole.Student,
                IsActive = false,
            };
            Assert.False(await guard.CanPushToRepositoryAsync(quiet, fix.OpenRepo));
        }
        finally
        {
            await CleanupAsync(db, tag);
        }
    }
}
