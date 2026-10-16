using System.Diagnostics;
using System.Text;
using RepoVault.Core.Data;
using RepoVault.Core.GitServer;
using RepoVault.Core.Metrics;
using RepoVault.Core.Models;
using RepoVault.Core.Security;
using RepoVault.Core.Services;

namespace RepoVault.Tests;

public sealed class SeederTests : IAsyncLifetime
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

    private static string FindSeedSql()
    {
        string? dir = AppContext.BaseDirectory;
        for (int i = 0; i < 10 && dir is not null; i++)
        {
            string candidate = Path.Combine(dir, "database", "seed.sql");
            if (File.Exists(candidate))
            {
                return candidate;
            }

            candidate = Path.Combine(dir, "repo-vault", "database", "seed.sql");
            if (File.Exists(candidate))
            {
                return candidate;
            }

            dir = Directory.GetParent(dir)?.FullName;
        }

        // fall back to a checkout-relative guess for local runs
        string guess = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "database", "seed.sql"));
        if (File.Exists(guess))
        {
            return guess;
        }

        throw new FileNotFoundException("database/seed.sql was not found from the test run dir.");
    }

    [Fact]
    public async Task SeedSql_CanBeParsedAndContainsRequiredEntities()
    {
        string path = FindSeedSql();
        string script = await File.ReadAllTextAsync(path);
        Assert.Contains("prof_santos", script);
        Assert.Contains("frtzhahn", script);
        Assert.Contains("CS201", script);
        Assert.Contains("micro-8", script);

        // script is idempotent, applying it here also proves the syntax runs
        var body = new StringBuilder();
        foreach (string line in script.Split('\n'))
        {
            if (line.TrimStart().StartsWith("--", StringComparison.Ordinal))
            {
                continue;
            }

            body.AppendLine(line);
        }

        await using var conn = await _db.CreateOpenConnectionAsync();
        foreach (string chunk in body.ToString().Split(';'))
        {
            string stmt = chunk.Trim();
            if (stmt.Length == 0)
            {
                continue;
            }

            await using var cmd = conn.CreateCommand();
            cmd.CommandText = stmt + ";";
            await cmd.ExecuteNonQueryAsync();
        }

        await using (var users = conn.CreateCommand())
        {
            users.CommandText = "SELECT COUNT(*) FROM Users WHERE Username IN " +
                                "('prof_santos','frtzhahn','sduites','epapina','aobrence','jdelacruz');";
            Assert.Equal(6, Convert.ToInt32(await users.ExecuteScalarAsync()));
        }

        await using (var courses = conn.CreateCommand())
        {
            courses.CommandText = "SELECT COUNT(*) FROM Courses WHERE CourseCode IN ('CS201','CS204','CS206');";
            Assert.Equal(3, Convert.ToInt32(await courses.ExecuteScalarAsync()));
        }

        await using (var repos = conn.CreateCommand())
        {
            repos.CommandText = "SELECT COUNT(*) FROM Repositories WHERE RepoSlug IN " +
                                "('micro-8','randfacts','lockedin','web-archive','trashed-calc','archived-exam');";
            Assert.Equal(6, Convert.ToInt32(await repos.ExecuteScalarAsync()));
        }

        // seed hashes must verify against Password123!
        var auth = new AuthService(_db);
        var (ok, _, _) = await auth.AuthenticateAsync("frtzhahn", "Password123!");
        Assert.True(ok);
    }

    private async Task<RepositoryRecord> MakeDbRepoAsync(string tag, string slug)
    {
        var auth = new AuthService(_db);
        var (fok, fmsg, faculty) = await auth.RegisterUserAsync(
            "IDF" + tag, "uF" + tag, "Passw0rd!" + tag,
            "Fac " + tag, "uF" + tag + "@example.com", UserRole.Faculty);
        Assert.True(fok, fmsg);

        var courses = new CourseService(_db);
        var (cok, cmsg, course) = await courses.CreateCourseAsync(
            faculty!, "C" + tag, "Course " + tag, "CSD", "2026-1st");
        Assert.True(cok, cmsg);

        string root = TempDir("repovault-seed-", tag);
        var catalog = new RepoService(_db, root);
        var (rok, rmsg, repo) = await catalog.CreateRepositoryAsync(
            faculty!, course!.CourseId, slug, "Repo " + tag, null,
            DateTime.UtcNow.AddDays(7));
        Assert.True(rok, rmsg);
        return repo!;
    }

    private static async Task<string> GitOutputAsync(string dir, params string[] args)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "git",
            WorkingDirectory = dir,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };
        psi.Environment["LC_ALL"] = "C";
        foreach (string arg in args)
        {
            psi.ArgumentList.Add(arg);
        }

        using var proc = new Process { StartInfo = psi };
        proc.Start();
        string stdout = await proc.StandardOutput.ReadToEndAsync();
        await proc.StandardError.ReadToEndAsync();
        await proc.WaitForExitAsync();
        Assert.Equal(0, proc.ExitCode);
        return stdout.Trim();
    }

    [Fact]
    public async Task DemoWorkspaceSeeder_InitializesBareReposAndSampleCommits()
    {
        string tag = Tag();
        RepositoryRecord repo = await MakeDbRepoAsync(tag, "seed" + tag);
        string root = TempDir("repovault-seedroot-", tag);

        var seeder = new DemoWorkspaceSeeder(_db);
        SeederResult first = await seeder.SeedRepoAsync(repo.RepoId, root);
        Assert.Equal(1, first.ReposSeeded);
        Assert.Equal(1, first.CommitsIndexed);

        string bare = Path.Combine(root, repo.RepoSlug + ".git");
        Assert.True(Directory.Exists(bare));

        Dictionary<string, string> refs = await GitProcessRunner.SnapshotRefsAsync(bare);
        Assert.True(refs.ContainsKey("refs/heads/main"));

        string count = await GitOutputAsync(bare, "rev-list", "--count", "refs/heads/main");
        Assert.Equal("1", count);

        // second run is a no-op for commits, proves idempotency
        SeederResult second = await seeder.SeedRepoAsync(repo.RepoId, root);
        Assert.Equal(1, second.ReposSeeded);
        Assert.Equal(0, second.CommitsIndexed);

        var catalog = new RepoService(_db);
        RepositoryRecord? fresh = await catalog.GetByIdAsync(repo.RepoId);
        Assert.Equal(bare, fresh!.BareRepoDiskPath);
    }

    [Fact]
    public async Task DemoWorkspaceSeeder_AllowsRepoBrowserAndMetricsScanning()
    {
        string tag = Tag();
        RepositoryRecord repo = await MakeDbRepoAsync(tag, "browse" + tag);
        string root = TempDir("repovault-seedbrowse-", tag);

        var seeder = new DemoWorkspaceSeeder(_db);
        await seeder.SeedRepoAsync(repo.RepoId, root);

        string bare = Path.Combine(root, repo.RepoSlug + ".git");
        var browser = new RepoBrowser();
        List<RepoFileEntry> files = await browser.ListFilesAtHeadAsync(bare);
        Assert.Contains(files, f => f.Path == "README.md");
        Assert.Contains(files, f => f.Path == "Program.cs");

        RepoBlobContent readme = await browser.ReadFileBlobAsync(bare, "README.md");
        Assert.False(readme.IsBinary);
        Assert.Contains(repo.RepoSlug, readme.TextContent);

        var scanner = new CodeMetricsScanner(_db);
        RepositoryMetricsResult metrics =
            await scanner.ScanRepositoryMetricsAsync(repo.RepoId, bare);
        Assert.Equal(2, metrics.TotalFiles);
        Assert.True(metrics.SourceLines > 0);
        Assert.NotNull(metrics.PrimaryLanguage);

        FileMetricsResult one = LineCounter.ClassifyFile(
            "Program.cs", Encoding.UTF8.GetBytes("var x = 1;\n"));
        Assert.Equal(1, one.SourceLines);
    }
}
