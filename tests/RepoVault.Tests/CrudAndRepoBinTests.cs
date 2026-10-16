using RepoVault.Core.Data;
using RepoVault.Core.Models;
using RepoVault.Core.Security;
using RepoVault.Core.Services;

namespace RepoVault.Tests;

public sealed class CrudAndRepoBinTests : IAsyncLifetime
{
    private readonly DbHelper _db = new();
    private readonly List<string> _tags = new();
    private readonly List<string> _tempDirs = new();

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        foreach (string tag in _tags)
        {
            try
            {
                await CleanupAsync(tag);
            }
            catch
            {
                // best effort, one dirty tag must not fail the whole run
            }
        }

        foreach (string dir in _tempDirs)
        {
            try
            {
                if (Directory.Exists(dir))
                {
                    foreach (string file in Directory.GetFiles(dir, "*", SearchOption.AllDirectories))
                    {
                        File.SetAttributes(file, File.GetAttributes(file) & ~FileAttributes.ReadOnly);
                    }

                    Directory.Delete(dir, true);
                }
            }
            catch
            {
            }
        }
    }

    private string Tag()
    {
        string tag = Guid.NewGuid().ToString("N")[..8];
        _tags.Add(tag);
        return tag;
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

    private async Task<User> RegisterAsync(string tag, string prefix, UserRole role)
    {
        var auth = new AuthService(_db);
        var (ok, msg, user) = await auth.RegisterUserAsync(
            "ID" + prefix + tag, "u" + prefix + tag, "Passw0rd!" + tag,
            "Name " + tag, "u" + prefix + tag + "@example.com", role);
        Assert.True(ok, msg);
        return user!;
    }

    private async Task<Course> MakeCourseAsync(User faculty, string tag, string codePrefix = "C")
    {
        var courses = new CourseService(_db);
        var (ok, msg, course) = await courses.CreateCourseAsync(
            faculty, codePrefix + tag, "Course " + tag, "CSD", "2026-1st");
        Assert.True(ok, msg);
        return course!;
    }

    private RepoService CatalogFor(string tag)
    {
        string root = Path.Combine(Path.GetTempPath(), "repovault-tests", tag);
        _tempDirs.Add(root);
        return new RepoService(_db, root);
    }

    private async Task<RepositoryRecord> MakeRepoAsync(
        RepoService catalog, User actor, int courseId, string tag, string slug = "repo")
    {
        var (ok, msg, repo) = await catalog.CreateRepositoryAsync(
            actor, courseId, slug + tag, "Repo " + tag, null, DateTime.UtcNow.AddDays(7));
        Assert.True(ok, msg);
        return repo!;
    }

    [Fact]
    public async Task CreateCourse_Success()
    {
        string tag = Tag();
        User faculty = await RegisterAsync(tag, "F", UserRole.Faculty);
        Course course = await MakeCourseAsync(faculty, tag);

        var courses = new CourseService(_db);
        Course? found = await courses.GetByIdAsync(course.CourseId);
        Assert.NotNull(found);
        Assert.Equal("C" + tag, found.CourseCode);
        Assert.Equal(faculty.UserId, found.CreatedByFacultyId);
    }

    [Fact]
    public async Task CreateCourse_DuplicateCodeSemester_Rejected()
    {
        string tag = Tag();
        User faculty = await RegisterAsync(tag, "F", UserRole.Faculty);
        await MakeCourseAsync(faculty, tag);

        var courses = new CourseService(_db);
        var (ok, msg, _) = await courses.CreateCourseAsync(
            faculty, "C" + tag, "Other Title", "CSD", "2026-1st");
        Assert.False(ok);
        Assert.Contains("already exists", msg);
    }

    [Fact]
    public async Task CreateCourse_StudentForbidden()
    {
        string tag = Tag();
        User student = await RegisterAsync(tag, "S", UserRole.Student);

        var courses = new CourseService(_db);
        var (ok, _, _) = await courses.CreateCourseAsync(
            student, "C" + tag, "Course", "CSD", "2026-1st");
        Assert.False(ok);
    }

    [Fact]
    public async Task CreateCourse_SameCodeDifferentSemester_Allowed()
    {
        string tag = Tag();
        User faculty = await RegisterAsync(tag, "F", UserRole.Faculty);
        await MakeCourseAsync(faculty, tag);

        var courses = new CourseService(_db);
        var (ok, msg, _) = await courses.CreateCourseAsync(
            faculty, "C" + tag, "Course", "CSD", "2026-2nd");
        Assert.True(ok, msg);
    }

    [Fact]
    public async Task UpdateCourse_Success()
    {
        string tag = Tag();
        User faculty = await RegisterAsync(tag, "F", UserRole.Faculty);
        Course course = await MakeCourseAsync(faculty, tag);

        var courses = new CourseService(_db);
        var (ok, msg) = await courses.UpdateCourseAsync(
            faculty, course.CourseId, "C" + tag, "Renamed", "CSD", "2026-1st");
        Assert.True(ok, msg);

        Course? found = await courses.GetByIdAsync(course.CourseId);
        Assert.Equal("Renamed", found!.CourseTitle);
    }

    [Fact]
    public async Task UpdateCourse_DuplicateCollision_Rejected()
    {
        string tag = Tag();
        User faculty = await RegisterAsync(tag, "F", UserRole.Faculty);
        await MakeCourseAsync(faculty, tag);
        Course other = await MakeCourseAsync(faculty, tag, "D");

        var courses = new CourseService(_db);
        var (ok, msg) = await courses.UpdateCourseAsync(
            faculty, other.CourseId, "C" + tag, "Clash", "CSD", "2026-1st");
        Assert.False(ok);
        Assert.Contains("already exists", msg);
    }

    [Fact]
    public async Task UpdateCourse_StudentForbidden()
    {
        string tag = Tag();
        User faculty = await RegisterAsync(tag, "F", UserRole.Faculty);
        User student = await RegisterAsync(tag, "S", UserRole.Student);
        Course course = await MakeCourseAsync(faculty, tag);

        var courses = new CourseService(_db);
        var (ok, _) = await courses.UpdateCourseAsync(
            student, course.CourseId, "C" + tag, "Hack", "CSD", "2026-1st");
        Assert.False(ok);
    }

    [Fact]
    public async Task DeleteCourse_WithRepos_Rejected()
    {
        string tag = Tag();
        User faculty = await RegisterAsync(tag, "F", UserRole.Faculty);
        Course course = await MakeCourseAsync(faculty, tag);
        var catalog = CatalogFor(tag);
        await MakeRepoAsync(catalog, faculty, course.CourseId, tag);

        var courses = new CourseService(_db);
        var (ok, msg) = await courses.DeleteCourseAsync(faculty, course.CourseId);
        Assert.False(ok);
        Assert.Contains("repositories", msg);

        // repo must still be there, no cascade happened
        Assert.NotEmpty(await catalog.ListActiveByCourseAsync(course.CourseId));
    }

    [Fact]
    public async Task DeleteCourse_Empty_Success()
    {
        string tag = Tag();
        User faculty = await RegisterAsync(tag, "F", UserRole.Faculty);
        Course course = await MakeCourseAsync(faculty, tag);

        var courses = new CourseService(_db);
        var (ok, msg) = await courses.DeleteCourseAsync(faculty, course.CourseId);
        Assert.True(ok, msg);
        Assert.Null(await courses.GetByIdAsync(course.CourseId));
    }

    [Fact]
    public async Task ListCourses_SemesterFilter()
    {
        string tag = Tag();
        User faculty = await RegisterAsync(tag, "F", UserRole.Faculty);
        await MakeCourseAsync(faculty, tag);

        var courses = new CourseService(_db);
        var (ok, _, _) = await courses.CreateCourseAsync(
            faculty, "D" + tag, "Other", "CSD", "2026-2nd");
        Assert.True(ok);

        List<Course> first = await courses.ListCoursesAsync("2026-1st");
        Assert.Contains(first, c => c.CourseCode == "C" + tag);
        Assert.DoesNotContain(first, c => c.CourseCode == "D" + tag);
    }

    [Fact]
    public async Task UpdateProfile_StudentOwn_Success()
    {
        string tag = Tag();
        User student = await RegisterAsync(tag, "S", UserRole.Student);

        var users = new UserService(_db);
        var (ok, msg, updated) = await users.UpdateProfileAsync(
            student, student.UserId, "New Name", "new" + tag + "@example.com",
            UserRole.Student, true);
        Assert.True(ok, msg);
        Assert.Equal("New Name", updated!.FullName);
    }

    [Fact]
    public async Task UpdateProfile_StudentCannotChangeRole()
    {
        string tag = Tag();
        User student = await RegisterAsync(tag, "S", UserRole.Student);

        var users = new UserService(_db);
        var (ok, _, _) = await users.UpdateProfileAsync(
            student, student.UserId, "Name", "uS" + tag + "@example.com",
            UserRole.Faculty, true);
        Assert.False(ok);
    }

    [Fact]
    public async Task UpdateProfile_StudentCannotEditOthers()
    {
        string tag = Tag();
        User first = await RegisterAsync(tag, "A", UserRole.Student);
        User second = await RegisterAsync(tag, "B", UserRole.Student);

        var users = new UserService(_db);
        var (ok, _, _) = await users.UpdateProfileAsync(
            first, second.UserId, "Hijacked", "uB" + tag + "@example.com",
            UserRole.Student, true);
        Assert.False(ok);
    }

    [Fact]
    public async Task UpdateProfile_FacultyCanChangeRoleAndStatus()
    {
        string tag = Tag();
        User faculty = await RegisterAsync(tag, "F", UserRole.Faculty);
        User student = await RegisterAsync(tag, "S", UserRole.Student);

        var users = new UserService(_db);
        var (ok, msg, updated) = await users.UpdateProfileAsync(
            faculty, student.UserId, student.FullName, student.Email,
            UserRole.Faculty, false);
        Assert.True(ok, msg);
        Assert.Equal(UserRole.Faculty, updated!.Role);
        Assert.False(updated.IsActive);
    }

    [Fact]
    public async Task UpdateProfile_DuplicateEmail_Rejected()
    {
        string tag = Tag();
        User first = await RegisterAsync(tag, "A", UserRole.Student);
        User second = await RegisterAsync(tag, "B", UserRole.Student);

        var users = new UserService(_db);
        var (ok, msg, _) = await users.UpdateProfileAsync(
            second, second.UserId, second.FullName, first.Email,
            UserRole.Student, true);
        Assert.False(ok);
        Assert.Contains("email", msg);
    }

    [Fact]
    public async Task ChangePassword_Success_ThenLogin()
    {
        string tag = Tag();
        User student = await RegisterAsync(tag, "S", UserRole.Student);

        var users = new UserService(_db);
        var (ok, msg) = await users.ChangePasswordAsync(student, student.UserId, "BrandNewPass1");
        Assert.True(ok, msg);

        var auth = new AuthService(_db);
        var (loginOk, _, _) = await auth.AuthenticateAsync(student.Username, "BrandNewPass1");
        Assert.True(loginOk);
    }

    [Fact]
    public async Task ChangePassword_Short_Rejected()
    {
        string tag = Tag();
        User student = await RegisterAsync(tag, "S", UserRole.Student);

        var users = new UserService(_db);
        var (ok, _) = await users.ChangePasswordAsync(student, student.UserId, "short");
        Assert.False(ok);
    }

    [Fact]
    public async Task ChangePassword_StudentCannotChangeOthers()
    {
        string tag = Tag();
        User first = await RegisterAsync(tag, "A", UserRole.Student);
        User second = await RegisterAsync(tag, "B", UserRole.Student);

        var users = new UserService(_db);
        var (ok, _) = await users.ChangePasswordAsync(first, second.UserId, "BrandNewPass1");
        Assert.False(ok);
    }

    [Fact]
    public async Task DeleteUser_Unreferenced_Deletes()
    {
        string tag = Tag();
        User student = await RegisterAsync(tag, "S", UserRole.Student);
        User faculty = await RegisterAsync(tag, "F", UserRole.Faculty);

        var users = new UserService(_db);
        var (ok, msg) = await users.DeleteOrDeactivateUserAsync(faculty, student.UserId);
        Assert.True(ok, msg);
        Assert.Equal("ok", msg);
        Assert.Null(await users.GetByIdAsync(student.UserId));
    }

    [Fact]
    public async Task DeleteUser_Referenced_Deactivates()
    {
        string tag = Tag();
        User owner = await RegisterAsync(tag, "O", UserRole.Faculty);
        User admin = await RegisterAsync(tag, "F", UserRole.Faculty);
        await MakeCourseAsync(owner, tag);

        var users = new UserService(_db);
        var (ok, msg) = await users.DeleteOrDeactivateUserAsync(admin, owner.UserId);
        Assert.True(ok, msg);
        Assert.Contains("deactivated", msg);

        User? kept = await users.GetByIdAsync(owner.UserId);
        Assert.NotNull(kept);
        Assert.False(kept.IsActive);
    }

    [Fact]
    public async Task DeleteUser_StudentForbidden()
    {
        string tag = Tag();
        User first = await RegisterAsync(tag, "A", UserRole.Student);
        User second = await RegisterAsync(tag, "B", UserRole.Student);

        var users = new UserService(_db);
        var (ok, _) = await users.DeleteOrDeactivateUserAsync(first, second.UserId);
        Assert.False(ok);
        Assert.NotNull(await users.GetByIdAsync(second.UserId));
    }

    [Fact]
    public async Task ListUsers_RoleFilter()
    {
        string tag = Tag();
        await RegisterAsync(tag, "F", UserRole.Faculty);
        await RegisterAsync(tag, "S", UserRole.Student);

        var users = new UserService(_db);
        List<User> students = await users.ListUsersAsync(UserRole.Student);
        Assert.Contains(students, u => u.Username == "uS" + tag);
        Assert.DoesNotContain(students, u => u.Username == "uF" + tag);

        List<User> activeOnly = await users.ListUsersAsync(null, false);
        Assert.Contains(activeOnly, u => u.Username == "uS" + tag);
    }

    [Fact]
    public async Task CreateRepository_SlugNormalization()
    {
        string tag = Tag();
        User faculty = await RegisterAsync(tag, "F", UserRole.Faculty);
        Course course = await MakeCourseAsync(faculty, tag);
        var catalog = CatalogFor(tag);

        var (ok, msg, repo) = await catalog.CreateRepositoryAsync(
            faculty, course.CourseId, "My Cool Project!!", "Title", "desc",
            DateTime.UtcNow.AddDays(7));
        Assert.True(ok, msg);
        Assert.Equal("my-cool-project", repo!.RepoSlug);
        Assert.EndsWith(repo.RepoId.ToString(), repo.BareRepoDiskPath);

        // creating only computes the path, the git server makes the dir later
        Assert.False(Directory.Exists(repo.BareRepoDiskPath));
    }

    [Fact]
    public async Task CreateRepository_DuplicateSlug_Rejected()
    {
        string tag = Tag();
        User faculty = await RegisterAsync(tag, "F", UserRole.Faculty);
        Course course = await MakeCourseAsync(faculty, tag);
        var catalog = CatalogFor(tag);
        await MakeRepoAsync(catalog, faculty, course.CourseId, tag);

        var (ok, msg, _) = await catalog.CreateRepositoryAsync(
            faculty, course.CourseId, "repo" + tag, "Again", null, null);
        Assert.False(ok);
        Assert.Contains("slug", msg);
    }

    [Fact]
    public async Task CreateRepository_SameSlugDifferentCourse_Allowed()
    {
        string tag = Tag();
        User faculty = await RegisterAsync(tag, "F", UserRole.Faculty);
        Course first = await MakeCourseAsync(faculty, tag);
        Course second = await MakeCourseAsync(faculty, tag, "D");
        var catalog = CatalogFor(tag);
        await MakeRepoAsync(catalog, faculty, first.CourseId, tag);

        var (ok, msg, _) = await catalog.CreateRepositoryAsync(
            faculty, second.CourseId, "repo" + tag, "Other Course Copy", null, null);
        Assert.True(ok, msg);
    }

    [Fact]
    public async Task CreateRepository_BadCourse_Rejected()
    {
        string tag = Tag();
        User faculty = await RegisterAsync(tag, "F", UserRole.Faculty);
        var catalog = CatalogFor(tag);

        var (ok, msg, _) = await catalog.CreateRepositoryAsync(
            faculty, int.MaxValue, "repo" + tag, "Title", null, null);
        Assert.False(ok);
        Assert.Contains("course", msg);
    }

    [Fact]
    public async Task RepoLookup_And_ContributorListing()
    {
        string tag = Tag();
        User faculty = await RegisterAsync(tag, "F", UserRole.Faculty);
        User student = await RegisterAsync(tag, "S", UserRole.Student);
        Course course = await MakeCourseAsync(faculty, tag);
        var catalog = CatalogFor(tag);
        RepositoryRecord repo = await MakeRepoAsync(catalog, faculty, course.CourseId, tag);

        RepositoryRecord? bySlug = await catalog.GetBySlugAsync(course.CourseId, "repo" + tag);
        Assert.NotNull(bySlug);
        Assert.Equal(repo.RepoId, bySlug.RepoId);

        var contributors = new ContributorService(_db);
        var (ok, msg) = await contributors.AssignContributorAsync(faculty, repo.RepoId, student.UserId);
        Assert.True(ok, msg);

        List<RepositoryRecord> mine = await catalog.ListByStudentContributorAsync(student.UserId);
        Assert.Contains(mine, r => r.RepoId == repo.RepoId);

        List<RepositoryContributor> rows = await contributors.ListContributorsByRepoAsync(repo.RepoId);
        Assert.Single(rows);
        Assert.Equal("Member", rows[0].TeamRole);
    }

    [Fact]
    public async Task Contributor_DuplicateAndRoleRules()
    {
        string tag = Tag();
        User faculty = await RegisterAsync(tag, "F", UserRole.Faculty);
        User other = await RegisterAsync(tag, "O", UserRole.Faculty);
        User student = await RegisterAsync(tag, "S", UserRole.Student);
        Course course = await MakeCourseAsync(faculty, tag);
        var catalog = CatalogFor(tag);
        RepositoryRecord repo = await MakeRepoAsync(catalog, faculty, course.CourseId, tag);

        var contributors = new ContributorService(_db);
        Assert.True((await contributors.AssignContributorAsync(faculty, repo.RepoId, student.UserId, "Lead")).Ok);

        var (dupOk, dupMsg) = await contributors.AssignContributorAsync(
            faculty, repo.RepoId, student.UserId);
        Assert.False(dupOk);
        Assert.Contains("already assigned", dupMsg);

        var (facOk, _) = await contributors.AssignContributorAsync(
            faculty, repo.RepoId, other.UserId);
        Assert.False(facOk);

        var (roleOk, roleMsg) = await contributors.UpdateTeamRoleAsync(
            faculty, repo.RepoId, student.UserId, "Reviewer");
        Assert.True(roleOk, roleMsg);
        Assert.Equal("Reviewer", (await contributors.ListContributorsByRepoAsync(repo.RepoId))[0].TeamRole);

        var (studOk, _) = await contributors.AssignContributorAsync(
            student, repo.RepoId, student.UserId);
        Assert.False(studOk);

        var (rmOk, rmMsg) = await contributors.RemoveContributorAsync(
            faculty, repo.RepoId, student.UserId);
        Assert.True(rmOk, rmMsg);
        Assert.Empty(await contributors.ListContributorsByRepoAsync(repo.RepoId));
    }

    [Fact]
    public async Task UpdateRepository_And_LockRules()
    {
        string tag = Tag();
        User faculty = await RegisterAsync(tag, "F", UserRole.Faculty);
        User student = await RegisterAsync(tag, "S", UserRole.Student);
        Course course = await MakeCourseAsync(faculty, tag);
        var catalog = CatalogFor(tag);
        RepositoryRecord repo = await MakeRepoAsync(catalog, faculty, course.CourseId, tag);

        var (ok, msg) = await catalog.UpdateRepositoryAsync(
            student, repo.RepoId, "Student Title", "student desc", null, false);
        Assert.True(ok, msg);

        var (lockOk, _) = await catalog.UpdateRepositoryAsync(
            student, repo.RepoId, "Student Title", null, null, true);
        Assert.False(lockOk);

        var (setOk, setMsg) = await catalog.SetLockStateAsync(faculty, repo.RepoId, true);
        Assert.True(setOk, setMsg);
        Assert.True((await catalog.GetByIdAsync(repo.RepoId))!.IsLocked);

        var (studLockOk, _) = await catalog.SetLockStateAsync(student, repo.RepoId, false);
        Assert.False(studLockOk);
    }

    [Fact]
    public async Task RepoBin_FullLifecycle_WithReadOnlyPackFile()
    {
        string tag = Tag();
        User faculty = await RegisterAsync(tag, "F", UserRole.Faculty);
        Course course = await MakeCourseAsync(faculty, tag);
        var catalog = CatalogFor(tag);
        RepositoryRecord repo = await MakeRepoAsync(catalog, faculty, course.CourseId, tag);

        // fake a bare repo dir with a read-only pack file like windows git leaves behind
        string objects = Path.Combine(repo.BareRepoDiskPath, "objects", "pack");
        Directory.CreateDirectory(objects);
        string pack = Path.Combine(objects, "pack-abc123.pack");
        await File.WriteAllTextAsync(pack, "fake pack bytes");
        File.SetAttributes(pack, File.GetAttributes(pack) | FileAttributes.ReadOnly);

        var (trashOk, trashMsg) = await catalog.MoveToRepoBinAsync(faculty, repo.RepoId);
        Assert.True(trashOk, trashMsg);

        RepositoryRecord? trashed = await catalog.GetByIdAsync(repo.RepoId);
        Assert.Equal(LifecycleStatus.Trashed, trashed!.LifecycleStatus);
        Assert.NotNull(trashed.TrashedAt);
        Assert.Empty(await catalog.ListActiveByCourseAsync(course.CourseId));

        var (listOk, _, bin) = await catalog.ListTrashedInRepoBinAsync(faculty);
        Assert.True(listOk);
        Assert.Contains(bin, r => r.RepoId == repo.RepoId);

        var (restoreOk, restoreMsg) = await catalog.RestoreFromRepoBinAsync(faculty, repo.RepoId);
        Assert.True(restoreOk, restoreMsg);
        Assert.Equal(LifecycleStatus.Active, (await catalog.GetByIdAsync(repo.RepoId))!.LifecycleStatus);

        Assert.True((await catalog.MoveToRepoBinAsync(faculty, repo.RepoId)).Ok);
        var (purgeOk, purgeMsg) = await catalog.PurgeFromRepoBinAsync(faculty, repo.RepoId);
        Assert.True(purgeOk, purgeMsg);

        Assert.Null(await catalog.GetByIdAsync(repo.RepoId));
        Assert.False(Directory.Exists(repo.BareRepoDiskPath));
        Assert.False(Directory.Exists(repo.BareRepoDiskPath + ".purging"));
    }

    [Fact]
    public async Task RepoBin_BadTransitions_Rejected()
    {
        string tag = Tag();
        User faculty = await RegisterAsync(tag, "F", UserRole.Faculty);
        User student = await RegisterAsync(tag, "S", UserRole.Student);
        Course course = await MakeCourseAsync(faculty, tag);
        var catalog = CatalogFor(tag);
        RepositoryRecord repo = await MakeRepoAsync(catalog, faculty, course.CourseId, tag);

        Assert.False((await catalog.RestoreFromRepoBinAsync(faculty, repo.RepoId)).Ok);
        Assert.False((await catalog.PurgeFromRepoBinAsync(faculty, repo.RepoId)).Ok);
        Assert.False((await catalog.MoveToRepoBinAsync(student, repo.RepoId)).Ok);

        var (_, _, bin) = await catalog.ListTrashedInRepoBinAsync(student);
        Assert.Empty(bin);

        Assert.True((await catalog.MoveToRepoBinAsync(faculty, repo.RepoId)).Ok);
        Assert.False((await catalog.MoveToRepoBinAsync(faculty, repo.RepoId)).Ok);
        Assert.True((await catalog.PurgeFromRepoBinAsync(faculty, repo.RepoId)).Ok);
    }

    [Fact]
    public async Task PurgeFromRepoBin_MissingDir_StillDeletesRow()
    {
        string tag = Tag();
        User faculty = await RegisterAsync(tag, "F", UserRole.Faculty);
        Course course = await MakeCourseAsync(faculty, tag);
        var catalog = CatalogFor(tag);
        RepositoryRecord repo = await MakeRepoAsync(catalog, faculty, course.CourseId, tag);

        Assert.True((await catalog.MoveToRepoBinAsync(faculty, repo.RepoId)).Ok);
        var (ok, msg) = await catalog.PurgeFromRepoBinAsync(faculty, repo.RepoId);
        Assert.True(ok, msg);
        Assert.Null(await catalog.GetByIdAsync(repo.RepoId));
    }

    [Fact]
    public async Task Discussion_PostEditDelete_Flow()
    {
        string tag = Tag();
        User faculty = await RegisterAsync(tag, "F", UserRole.Faculty);
        User first = await RegisterAsync(tag, "A", UserRole.Student);
        User second = await RegisterAsync(tag, "B", UserRole.Student);
        Course course = await MakeCourseAsync(faculty, tag);
        var catalog = CatalogFor(tag);
        RepositoryRecord repo = await MakeRepoAsync(catalog, faculty, course.CourseId, tag);

        var discussion = new DiscussionService(_db);
        var (ok, msg, comment) = await discussion.AddCommentAsync(first, repo.RepoId, "nice work");
        Assert.True(ok, msg);

        var (editOk, editMsg) = await discussion.EditCommentAsync(first, comment!.CommentId, "really nice work");
        Assert.True(editOk, editMsg);

        var (blockedOk, _) = await discussion.EditCommentAsync(second, comment.CommentId, "hijack");
        Assert.False(blockedOk);

        var (facOk, facMsg) = await discussion.EditCommentAsync(faculty, comment.CommentId, "faculty note");
        Assert.True(facOk, facMsg);

        var (delBlockedOk, _) = await discussion.DeleteCommentAsync(second, comment.CommentId);
        Assert.False(delBlockedOk);

        var (delOk, delMsg) = await discussion.DeleteCommentAsync(first, comment.CommentId);
        Assert.True(delOk, delMsg);
        Assert.Empty(await discussion.ListCommentsByRepoAsync(repo.RepoId, true));
    }

    [Fact]
    public async Task Discussion_Moderation_HideAndUnhide()
    {
        string tag = Tag();
        User faculty = await RegisterAsync(tag, "F", UserRole.Faculty);
        User student = await RegisterAsync(tag, "S", UserRole.Student);
        Course course = await MakeCourseAsync(faculty, tag);
        var catalog = CatalogFor(tag);
        RepositoryRecord repo = await MakeRepoAsync(catalog, faculty, course.CourseId, tag);

        var discussion = new DiscussionService(_db);
        var (_, _, comment) = await discussion.AddCommentAsync(student, repo.RepoId, "spammy take");

        var (studModOk, _) = await discussion.SetModerationHiddenAsync(
            student, comment!.CommentId, true);
        Assert.False(studModOk);

        var (hideOk, hideMsg) = await discussion.SetModerationHiddenAsync(
            faculty, comment.CommentId, true);
        Assert.True(hideOk, hideMsg);

        Assert.Empty(await discussion.ListCommentsByRepoAsync(repo.RepoId, false));
        Assert.Single(await discussion.ListCommentsByRepoAsync(repo.RepoId, true));

        var (unhideOk, unhideMsg) = await discussion.SetModerationHiddenAsync(
            faculty, comment.CommentId, false);
        Assert.True(unhideOk, unhideMsg);
        Assert.Single(await discussion.ListCommentsByRepoAsync(repo.RepoId, false));
    }

    [Fact]
    public async Task SaveEvaluation_Upsert_LocksRepo()
    {
        string tag = Tag();
        User faculty = await RegisterAsync(tag, "F", UserRole.Faculty);
        Course course = await MakeCourseAsync(faculty, tag);
        var catalog = CatalogFor(tag);
        RepositoryRecord repo = await MakeRepoAsync(catalog, faculty, course.CourseId, tag);

        var evaluations = new EvaluationService(_db);
        var (ok, msg, saved) = await evaluations.SaveEvaluationAsync(
            faculty, repo.RepoId, 20, 20, 20, 20, "solid work");
        Assert.True(ok, msg);
        Assert.Equal(80, saved!.TotalScore);

        EvaluationRecord? reread = await evaluations.GetEvaluationByRepoAsync(repo.RepoId);
        Assert.Equal(80, reread!.TotalScore);
        Assert.True((await catalog.GetByIdAsync(repo.RepoId))!.IsLocked);
    }

    [Fact]
    public async Task SaveEvaluation_UpdateExisting()
    {
        string tag = Tag();
        User faculty = await RegisterAsync(tag, "F", UserRole.Faculty);
        Course course = await MakeCourseAsync(faculty, tag);
        var catalog = CatalogFor(tag);
        RepositoryRecord repo = await MakeRepoAsync(catalog, faculty, course.CourseId, tag);

        var evaluations = new EvaluationService(_db);
        var (_, _, first) = await evaluations.SaveEvaluationAsync(
            faculty, repo.RepoId, 10, 10, 10, 10, "first pass", false);
        var (ok, msg, second) = await evaluations.SaveEvaluationAsync(
            faculty, repo.RepoId, 25, 25, 25, 25, "second pass", false);
        Assert.True(ok, msg);
        Assert.Equal(first!.EvaluationId, second!.EvaluationId);
        Assert.Equal(100, second.TotalScore);
        Assert.False((await catalog.GetByIdAsync(repo.RepoId))!.IsLocked);
    }

    [Fact]
    public async Task SaveEvaluation_OutOfRange_Rejected()
    {
        string tag = Tag();
        User faculty = await RegisterAsync(tag, "F", UserRole.Faculty);
        Course course = await MakeCourseAsync(faculty, tag);
        var catalog = CatalogFor(tag);
        RepositoryRecord repo = await MakeRepoAsync(catalog, faculty, course.CourseId, tag);

        var evaluations = new EvaluationService(_db);
        var (ok, msg, _) = await evaluations.SaveEvaluationAsync(
            faculty, repo.RepoId, 26, 25, 25, 25, "too high");
        Assert.False(ok);
        Assert.Contains("25", msg);
        Assert.Null(await evaluations.GetEvaluationByRepoAsync(repo.RepoId));
    }

    [Fact]
    public async Task SaveEvaluation_StudentForbidden()
    {
        string tag = Tag();
        User faculty = await RegisterAsync(tag, "F", UserRole.Faculty);
        User student = await RegisterAsync(tag, "S", UserRole.Student);
        Course course = await MakeCourseAsync(faculty, tag);
        var catalog = CatalogFor(tag);
        RepositoryRecord repo = await MakeRepoAsync(catalog, faculty, course.CourseId, tag);

        var evaluations = new EvaluationService(_db);
        var (ok, _, _) = await evaluations.SaveEvaluationAsync(
            student, repo.RepoId, 20, 20, 20, 20, "peer grade");
        Assert.False(ok);
    }

    [Fact]
    public async Task DeleteEvaluation_Flow()
    {
        string tag = Tag();
        User faculty = await RegisterAsync(tag, "F", UserRole.Faculty);
        User student = await RegisterAsync(tag, "S", UserRole.Student);
        Course course = await MakeCourseAsync(faculty, tag);
        var catalog = CatalogFor(tag);
        RepositoryRecord repo = await MakeRepoAsync(catalog, faculty, course.CourseId, tag);

        var evaluations = new EvaluationService(_db);
        await evaluations.SaveEvaluationAsync(faculty, repo.RepoId, 20, 20, 20, 20, "graded");

        var (studOk, _) = await evaluations.DeleteEvaluationAsync(student, repo.RepoId);
        Assert.False(studOk);

        var (ok, msg) = await evaluations.DeleteEvaluationAsync(faculty, repo.RepoId);
        Assert.True(ok, msg);
        Assert.Null(await evaluations.GetEvaluationByRepoAsync(repo.RepoId));

        var (againOk, _) = await evaluations.DeleteEvaluationAsync(faculty, repo.RepoId);
        Assert.False(againOk);
    }
}
