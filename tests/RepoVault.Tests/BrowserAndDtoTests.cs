using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using RepoVault.Core.Data;
using RepoVault.Core.GitServer;
using RepoVault.Core.Metrics;
using RepoVault.Core.Models;
using RepoVault.Core.Security;
using RepoVault.Core.Services;

namespace RepoVault.Tests;

public sealed class BrowserAndDtoTests : IAsyncLifetime
{
    private readonly DbConnectionFactory _db = new();
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

    private string Tag()
    {
        string tag = Guid.NewGuid().ToString("N")[..8];
        _tags.Add(tag);
        return tag;
    }

    private string TempDir(string prefix, string tag)
    {
        string dir = Path.Combine(Path.GetTempPath(), prefix + tag);
        _tempDirs.Add(dir);
        return dir;
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

    private async Task<(User Faculty, User Student, Course Course, RepositoryRecord Repo)> MakeDbRepoAsync(
        string tag, string slug)
    {
        var auth = new AuthService(_db);
        var (fok, fmsg, faculty) = await auth.RegisterUserAsync(
            "IDF" + tag, "uF" + tag, "Passw0rd!" + tag,
            "Fac " + tag, "uF" + tag + "@example.com", UserRole.Faculty);
        Assert.True(fok, fmsg);
        var (sok, smsg, student) = await auth.RegisterUserAsync(
            "IDS" + tag, "uS" + tag, "Passw0rd!" + tag,
            "Stu " + tag, "uS" + tag + "@example.com", UserRole.Student);
        Assert.True(sok, smsg);

        var courses = new CourseService(_db);
        var (cok, cmsg, course) = await courses.CreateCourseAsync(
            faculty!, "C" + tag, "Course " + tag, "CSD", "2026-1st");
        Assert.True(cok, cmsg);

        string root = TempDir("repovault-browser-", tag);
        var catalog = new RepositoryCatalogService(_db, root);
        var (rok, rmsg, repo) = await catalog.CreateRepositoryAsync(
            faculty!, course!.CourseId, slug, "Repo " + tag, null,
            DateTime.UtcNow.AddDays(7));
        Assert.True(rok, rmsg);
        return (faculty!, student!, course!, repo!);
    }

    private static async Task<int> GitAsync(string dir, params string[] args)
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

            return -1;
        }

        await stdout;
        await stderr;
        return proc.ExitCode;
    }

    private static async Task PushSampleRepoAsync(string barePath, string clientDir)
    {
        Directory.CreateDirectory(clientDir);
        Assert.Equal(0, await GitAsync(clientDir, "init", "--initial-branch=main", "."));
        Assert.Equal(0, await GitAsync(clientDir, "config", "user.name", "Test"));
        Assert.Equal(0, await GitAsync(clientDir, "config", "user.email", "test@test.local"));
        Assert.Equal(0, await GitAsync(clientDir, "push", barePath, "main"));
    }

    [Fact]
    public async Task Browser_EmptyRepo_ReturnsEmptyList()
    {
        string tag = Tag();
        var (_, _, _, repo) = await MakeDbRepoAsync(tag, "empty" + tag);
        await GitProcessRunner.InitializeBareRepositoryAsync(repo.BareRepoDiskPath);

        var browser = new RepositoryBrowserService();
        List<RepoFileEntry> files = await browser.ListFilesAtHeadAsync(repo.BareRepoDiskPath);
        Assert.Empty(files);

        List<RepoFileEntry> missing =
            await browser.ListFilesAtHeadAsync(Path.Combine(Path.GetTempPath(), "nope-" + tag));
        Assert.Empty(missing);
    }

    [Fact]
    public async Task Browser_NestedTextFiles_ListsAndReadsBlobs()
    {
        string tag = Tag();
        var (_, _, _, repo) = await MakeDbRepoAsync(tag, "nested" + tag);
        await GitProcessRunner.InitializeBareRepositoryAsync(repo.BareRepoDiskPath);

        string client = TempDir("repovault-bcli-", tag);
        Directory.CreateDirectory(client);
        Assert.Equal(0, await GitAsync(client, "init", "--initial-branch=main", "."));
        Assert.Equal(0, await GitAsync(client, "config", "user.name", "Test"));
        Assert.Equal(0, await GitAsync(client, "config", "user.email", "test@test.local"));

        Directory.CreateDirectory(Path.Combine(client, "src"));
        Directory.CreateDirectory(Path.Combine(client, "docs"));
        await File.WriteAllTextAsync(Path.Combine(client, "src", "App.cs"), "var x = 1;\n// note\n");
        await File.WriteAllTextAsync(Path.Combine(client, "docs", "notes.txt"), "hello\nworld\n");
        await File.WriteAllTextAsync(Path.Combine(client, "README.md"), "# Title\n");
        Assert.Equal(0, await GitAsync(client, "add", "."));
        Assert.Equal(0, await GitAsync(client, "commit", "-m", "add nested files"));
        Assert.Equal(0, await GitAsync(client, "push", repo.BareRepoDiskPath, "main"));

        var browser = new RepositoryBrowserService();
        List<RepoFileEntry> files = await browser.ListFilesAtHeadAsync(repo.BareRepoDiskPath);
        Assert.Equal(3, files.Count);

        RepoFileEntry app = files.Single(f => f.Path == "src/App.cs");
        Assert.Equal("App.cs", app.FileName);
        Assert.Equal("src", app.DirectoryPath);
        Assert.Equal(40, app.BlobSha.Length);
        Assert.True(app.SizeBytes > 0);
        Assert.Equal("100644", app.Mode);

        RepoFileEntry readme = files.Single(f => f.Path == "README.md");
        Assert.Equal("README.md", readme.FileName);
        Assert.Equal(string.Empty, readme.DirectoryPath);

        RepoBlobContent byPath = await browser.ReadFileBlobAsync(repo.BareRepoDiskPath, "src/App.cs");
        Assert.False(byPath.IsBinary);
        Assert.False(byPath.IsTruncated);
        Assert.Contains("var x = 1;", byPath.TextContent);
        Assert.True(byPath.SizeBytes > 0);

        RepoBlobContent bySha = await browser.ReadFileBlobAsync(repo.BareRepoDiskPath, app.BlobSha);
        Assert.Equal(byPath.TextContent, bySha.TextContent);
        Assert.Equal(byPath.SizeBytes, bySha.SizeBytes);
    }

    [Fact]
    public async Task Browser_BinaryBlob_ReturnsPlaceholder()
    {
        string tag = Tag();
        var (_, _, _, repo) = await MakeDbRepoAsync(tag, "binary" + tag);
        await GitProcessRunner.InitializeBareRepositoryAsync(repo.BareRepoDiskPath);

        string client = TempDir("repovault-bincli-", tag);
        Directory.CreateDirectory(client);
        Assert.Equal(0, await GitAsync(client, "init", "--initial-branch=main", "."));
        Assert.Equal(0, await GitAsync(client, "config", "user.name", "Test"));
        Assert.Equal(0, await GitAsync(client, "config", "user.email", "test@test.local"));

        var raw = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x00, 0x01, 0x02, 0x03 };
        await File.WriteAllBytesAsync(Path.Combine(client, "logo.bin"), raw);
        await File.WriteAllTextAsync(Path.Combine(client, "ok.txt"), "plain\n");
        Assert.Equal(0, await GitAsync(client, "add", "."));
        Assert.Equal(0, await GitAsync(client, "commit", "-m", "add binary"));
        Assert.Equal(0, await GitAsync(client, "push", repo.BareRepoDiskPath, "main"));

        var browser = new RepositoryBrowserService();
        List<RepoFileEntry> files = await browser.ListFilesAtHeadAsync(repo.BareRepoDiskPath);
        Assert.Equal(2, files.Count);

        RepoBlobContent bin = await browser.ReadFileBlobAsync(repo.BareRepoDiskPath, "logo.bin");
        Assert.True(bin.IsBinary);
        Assert.False(bin.IsTruncated);
        Assert.Equal("[Binary file - preview not available]", bin.TextContent);
        Assert.Equal(raw.Length, bin.SizeBytes);
    }

    [Fact]
    public async Task Browser_LargeBlob_TruncatesAtMaxBytes()
    {
        string tag = Tag();
        var (_, _, _, repo) = await MakeDbRepoAsync(tag, "big" + tag);
        await GitProcessRunner.InitializeBareRepositoryAsync(repo.BareRepoDiskPath);

        string client = TempDir("repovault-bigcli-", tag);
        Directory.CreateDirectory(client);
        Assert.Equal(0, await GitAsync(client, "init", "--initial-branch=main", "."));
        Assert.Equal(0, await GitAsync(client, "config", "user.name", "Test"));
        Assert.Equal(0, await GitAsync(client, "config", "user.email", "test@test.local"));

        string big = new string('a', 5000) + "\n";
        await File.WriteAllTextAsync(Path.Combine(client, "big.txt"), big);
        Assert.Equal(0, await GitAsync(client, "add", "."));
        Assert.Equal(0, await GitAsync(client, "commit", "-m", "add big file"));
        Assert.Equal(0, await GitAsync(client, "push", repo.BareRepoDiskPath, "main"));

        var browser = new RepositoryBrowserService();
        RepoBlobContent full = await browser.ReadFileBlobAsync(repo.BareRepoDiskPath, "big.txt");
        Assert.False(full.IsTruncated);
        Assert.True(full.SizeBytes > 1000);

        RepoBlobContent cut = await browser.ReadFileBlobAsync(repo.BareRepoDiskPath, "big.txt", 1000);
        Assert.False(cut.IsBinary);
        Assert.True(cut.IsTruncated);
        Assert.Equal(full.SizeBytes, cut.SizeBytes);
        Assert.EndsWith("[File truncated at 1 MB for display]", cut.TextContent);
    }

    [Fact]
    public async Task Dto_ListUserSummaries_FiltersAndHidesSecrets()
    {
        string tag = Tag();
        var auth = new AuthService(_db);
        var (fok, _, faculty) = await auth.RegisterUserAsync(
            "IDF" + tag, "uF" + tag, "Passw0rd!" + tag,
            "Fac " + tag, "uF" + tag + "@example.com", UserRole.Faculty);
        Assert.True(fok);
        var (sok, _, student) = await auth.RegisterUserAsync(
            "IDS" + tag, "uS" + tag, "Passw0rd!" + tag,
            "Stu " + tag, "uS" + tag + "@example.com", UserRole.Student);
        Assert.True(sok);

        var users = new UserService(_db);
        List<UserSummaryDto> all = await users.ListUserSummariesAsync();
        Assert.Contains(all, u => u.Username == faculty!.Username && u.Role == UserRole.Faculty);
        Assert.Contains(all, u => u.Username == student!.Username && u.Role == UserRole.Student);

        List<UserSummaryDto> onlyStudents = await users.ListUserSummariesAsync(UserRole.Student);
        Assert.Contains(onlyStudents, u => u.Username == student!.Username);
        Assert.DoesNotContain(onlyStudents, u => u.Username == faculty!.Username);

        // dto type has no password fields, check the shape by field access
        UserSummaryDto one = all.Single(u => u.Username == student!.Username);
        Assert.Equal("IDS" + tag, one.IdNumber);
        Assert.Equal("Stu " + tag, one.FullName);
        Assert.True(one.IsActive);
    }

    [Fact]
    public async Task Dto_PushLedgerView_JoinsUserOrderedDesc()
    {
        string tag = Tag();
        var (faculty, student, _, repo) = await MakeDbRepoAsync(tag, "ledger" + tag);

        await using var conn = await _db.CreateOpenConnectionAsync();
        DateTime first = DateTime.UtcNow.AddMinutes(-2);
        DateTime second = DateTime.UtcNow;

        foreach (DateTime when in new[] { first, second })
        {
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = "INSERT INTO PushLedger (RepoId, PusherUserId, BranchRef, OldCommitHash, " +
                              "NewCommitHash, ClientIpAddress, PushedAt) " +
                              "VALUES (@r, @u, @b, @o, @n, @ip, @d);";
            cmd.Parameters.AddWithValue("@r", repo.RepoId);
            cmd.Parameters.AddWithValue("@u", student.UserId);
            cmd.Parameters.AddWithValue("@b", "refs/heads/main");
            cmd.Parameters.AddWithValue("@o", new string('0', 40));
            cmd.Parameters.AddWithValue("@n", new string('a', 40));
            cmd.Parameters.AddWithValue("@ip", "127.0.0.1");
            cmd.Parameters.AddWithValue("@d", when);
            await cmd.ExecuteNonQueryAsync();
        }

        var catalog = new RepositoryCatalogService(_db);
        List<PushLedgerViewDto> rows = await catalog.ListPushLedgerViewAsync(repo.RepoId);
        Assert.Equal(2, rows.Count);
        Assert.True(rows[0].PushedAt >= rows[1].PushedAt);
        Assert.Equal(student.Username, rows[0].PusherUsername);
        Assert.Equal(student.FullName, rows[0].PusherFullName);
        Assert.Equal("refs/heads/main", rows[0].BranchRef);
        Assert.Equal("127.0.0.1", rows[0].ClientIpAddress);
    }

    [Fact]
    public async Task Dto_CommentViews_JoinsUserAndRespectsHidden()
    {
        string tag = Tag();
        var (faculty, student, _, repo) = await MakeDbRepoAsync(tag, "comments" + tag);

        var discussions = new DiscussionService(_db);
        var (ok1, _, c1) = await discussions.AddCommentAsync(student, repo.RepoId, "hello " + tag);
        Assert.True(ok1);
        var (ok2, _, c2) = await discussions.AddCommentAsync(faculty, repo.RepoId, "faculty note " + tag);
        Assert.True(ok2);
        Assert.True((await discussions.SetModerationHiddenAsync(faculty, c2!.CommentId, true)).Ok);

        List<DiscussionCommentViewDto> hidden =
            await discussions.ListCommentViewsByRepoAsync(repo.RepoId, true);
        Assert.Equal(2, hidden.Count);
        Assert.Contains(hidden, v => v.AuthorUsername == student.Username && !v.IsModeratedHidden);
        Assert.Contains(hidden, v => v.AuthorUsername == faculty.Username && v.IsModeratedHidden);

        List<DiscussionCommentViewDto> visible =
            await discussions.ListCommentViewsByRepoAsync(repo.RepoId, false);
        Assert.Single(visible);
        Assert.Equal(student.Username, visible[0].AuthorUsername);
        Assert.Equal(student.FullName, visible[0].AuthorFullName);
        Assert.Equal(UserRole.Student, visible[0].AuthorRole);
    }

    [Fact]
    public async Task Dto_ContributorViews_JoinsUser()
    {
        string tag = Tag();
        var (faculty, student, _, repo) = await MakeDbRepoAsync(tag, "team" + tag);

        var contributors = new ContributorService(_db);
        Assert.True((await contributors.AssignContributorAsync(
            faculty, repo.RepoId, student.UserId, "Lead")).Ok);

        List<ContributorViewDto> rows = await contributors.ListContributorViewsByRepoAsync(repo.RepoId);
        Assert.Single(rows);
        Assert.Equal(repo.RepoId, rows[0].RepoId);
        Assert.Equal(student.UserId, rows[0].StudentUserId);
        Assert.Equal(student.Username, rows[0].Username);
        Assert.Equal(student.FullName, rows[0].FullName);
        Assert.Equal(student.Email, rows[0].Email);
        Assert.Equal("Lead", rows[0].TeamRole);
    }

    [Fact]
    public async Task Dto_CommitLogsAndLanguages_ReadBack()
    {
        string tag = Tag();
        var (_, _, _, repo) = await MakeDbRepoAsync(tag, "logs" + tag);
        await GitProcessRunner.InitializeBareRepositoryAsync(repo.BareRepoDiskPath);

        string client = TempDir("repovault-dcli-", tag);
        Directory.CreateDirectory(client);
        Assert.Equal(0, await GitAsync(client, "init", "--initial-branch=main", "."));
        Assert.Equal(0, await GitAsync(client, "config", "user.name", "Test"));
        Assert.Equal(0, await GitAsync(client, "config", "user.email", "test@test.local"));
        await File.WriteAllTextAsync(Path.Combine(client, "App.cs"), "var x = 1;\n// note\n");
        await File.WriteAllTextAsync(Path.Combine(client, "notes.md"), "# Title\n\ntext\n");
        Assert.Equal(0, await GitAsync(client, "add", "."));
        Assert.Equal(0, await GitAsync(client, "commit", "-m", "add files " + tag));
        Assert.Equal(0, await GitAsync(client, "push", repo.BareRepoDiskPath, "main"));

        var indexer = new CommitLogIndexer(_db);
        Assert.Equal(1, await indexer.IndexCommitsAsync(repo.RepoId, repo.BareRepoDiskPath));

        var scanner = new CodeMetricsScanner(_db);
        await scanner.ScanRepositoryMetricsAsync(repo.RepoId, repo.BareRepoDiskPath);

        var catalog = new RepositoryCatalogService(_db);
        var commits = await catalog.ListCommitLogsAsync(repo.RepoId);
        Assert.Single(commits);
        Assert.Equal("Test", commits[0].AuthorName);
        Assert.Equal("add files " + tag, commits[0].CommitMessage);

        var langs = await catalog.ListLanguagesAsync(repo.RepoId);
        Assert.Equal(2, langs.Count);
        Assert.True(langs[0].SourceLines >= langs[1].SourceLines);
        Assert.Contains(langs, l => l.LanguageName == "C#");
        Assert.Contains(langs, l => l.LanguageName == "Markdown");
    }

    [Fact]
    public void Network_LanIpv4AndCloneUrl()
    {
        string ip = NetworkAddressHelper.GetPreferredLanIpv4Address();
        Assert.False(string.IsNullOrWhiteSpace(ip));
        Assert.True(IPAddress.TryParse(ip, out IPAddress? parsed));
        Assert.Equal(AddressFamily.InterNetwork, parsed!.AddressFamily);

        string url = NetworkAddressHelper.BuildCloneUrl("My Repo!", 5050, "192.168.1.10");
        Assert.Equal("http://192.168.1.10:5050/repos/my-repo.git", url);

        string auto = NetworkAddressHelper.BuildCloneUrl("demo-repo");
        Assert.StartsWith("http://", auto);
        Assert.EndsWith("/repos/demo-repo.git", auto);
        Assert.Contains(":5050/", auto);
    }
}
