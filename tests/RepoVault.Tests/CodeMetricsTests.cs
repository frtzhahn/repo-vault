using System.Diagnostics;
using System.Net.Sockets;
using System.Net;
using System.Text;
using RepoVault.Core.Data;
using RepoVault.Core.GitServer;
using RepoVault.Core.Metrics;
using RepoVault.Core.Models;
using RepoVault.Core.Security;
using RepoVault.Core.Services;

namespace RepoVault.Tests;

public sealed class CodeMetricsTests : IAsyncLifetime
{
    private readonly DbConnectionFactory _db = new();
    private readonly List<string> _tags = new();
    private readonly List<string> _tempDirs = new();
    private GitHttpServer? _server;

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        if (_server is not null)
        {
            await _server.DisposeAsync();
            _server = null;
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

    private static byte[] Bytes(string text) => Encoding.UTF8.GetBytes(text);

    private static byte[] WithBom(string text)
    {
        byte[] body = Bytes(text);
        var buf = new byte[body.Length + 3];
        buf[0] = 0xEF;
        buf[1] = 0xBB;
        buf[2] = 0xBF;
        body.CopyTo(buf, 3);
        return buf;
    }

    [Fact]
    public void Lexer_CSharp_MixedComments()
    {
        string code =
            "\n" +
            "using System;\n" +
            "\n" +
            "// line comment\n" +
            "var x = 1; // trailing\n" +
            "/* block\n" +
            "   continued */\n" +
            "var y = 2; /* inline */ var z = 3;\n" +
            "\n";

        FileMetricsResult got = SourceLineLexer.ClassifyFile("Program.cs", WithBom(code));
        Assert.False(got.IsBinary);
        Assert.Equal("C#", got.Language);
        Assert.Equal(3, got.SourceLines);
        Assert.Equal(3, got.CommentLines);
        Assert.Equal(3, got.BlankLines);
    }

    [Fact]
    public void Lexer_Python_HashAndDocstrings()
    {
        string code =
            "import os\n" +
            "\n" +
            "# comment\n" +
            "x = 1  # trailing\n" +
            "\"\"\"docstring\n" +
            "more docs\"\"\"\n" +
            "y = 2\n";

        FileMetricsResult got = SourceLineLexer.ClassifyFile("app.py", Bytes(code));
        Assert.False(got.IsBinary);
        Assert.Equal("Python", got.Language);
        Assert.Equal(3, got.SourceLines);
        Assert.Equal(3, got.CommentLines);
        Assert.Equal(1, got.BlankLines);
    }

    [Fact]
    public void Lexer_Sql_DashAndBlockComments()
    {
        string code =
            "-- comment\n" +
            "SELECT 1; -- get one\n" +
            "/* multi\n" +
            "line */\n" +
            "\n";

        FileMetricsResult got = SourceLineLexer.ClassifyFile("query.sql", Bytes(code));
        Assert.Equal("SQL", got.Language);
        Assert.Equal(1, got.SourceLines);
        Assert.Equal(3, got.CommentLines);
        Assert.Equal(1, got.BlankLines);
    }

    [Fact]
    public void Lexer_Markdown_HasNoCommentSyntax()
    {
        string code = "# Title\n\nSome text with // not a comment.\n";

        FileMetricsResult got = SourceLineLexer.ClassifyFile("notes.md", Bytes(code));
        Assert.Equal("Markdown", got.Language);
        Assert.Equal(2, got.SourceLines);
        Assert.Equal(0, got.CommentLines);
        Assert.Equal(1, got.BlankLines);
    }

    [Fact]
    public void Lexer_Binary_Detected()
    {
        var jpeg = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46 };

        FileMetricsResult got = SourceLineLexer.ClassifyFile("photo.jpg", jpeg);
        Assert.True(got.IsBinary);
        Assert.Equal(0, got.SourceLines);
        Assert.Equal(0, got.CommentLines);
        Assert.Equal(0, got.BlankLines);
    }

    [Fact]
    public void Lexer_NulBeyondWindow_IsStillText()
    {
        var buf = new byte[9001];
        Array.Fill(buf, (byte)'a');
        buf[9000] = 0;

        FileMetricsResult got = SourceLineLexer.ClassifyFile("big.txt", buf);
        Assert.False(got.IsBinary);
    }

    [Fact]
    public void Lexer_UnknownExtension_FallsBack()
    {
        FileMetricsResult got = SourceLineLexer.ClassifyFile("data.xyz", Bytes("a\n"));
        Assert.Equal("XYZ", got.Language);
        Assert.Equal(1, got.SourceLines);

        FileMetricsResult plain = SourceLineLexer.ClassifyFile("Makefile", Bytes("a\n"));
        Assert.Equal("Text", plain.Language);
    }

    [Fact]
    public void Lexer_TrailingNewline_DoesNotAddBlankLine()
    {
        string without = "var x = 1;\n// note\nvar y = 2;";
        string withNewline = without + "\n";
        string withCrlf = without + "\r\n";

        FileMetricsResult a = SourceLineLexer.ClassifyFile("App.cs", Bytes(without));
        FileMetricsResult b = SourceLineLexer.ClassifyFile("App.cs", Bytes(withNewline));
        FileMetricsResult c = SourceLineLexer.ClassifyFile("App.cs", Bytes(withCrlf));

        Assert.Equal(a.SourceLines, b.SourceLines);
        Assert.Equal(a.CommentLines, b.CommentLines);
        Assert.Equal(a.BlankLines, b.BlankLines);
        Assert.Equal(a.SourceLines, c.SourceLines);
        Assert.Equal(a.CommentLines, c.CommentLines);
        Assert.Equal(a.BlankLines, c.BlankLines);
        Assert.Equal(2, b.SourceLines);
        Assert.Equal(1, b.CommentLines);
        Assert.Equal(0, b.BlankLines);
    }

    [Fact]
    public void Lexer_EmptyFile_ReturnsZeroes()
    {
        FileMetricsResult got = SourceLineLexer.ClassifyFile("empty.txt", Array.Empty<byte>());
        Assert.False(got.IsBinary);
        Assert.Equal(0, got.SourceLines);
        Assert.Equal(0, got.CommentLines);
        Assert.Equal(0, got.BlankLines);
    }

    private async Task<(User Faculty, Course Course, RepositoryRecord Repo)> MakeDbRepoAsync(
        string tag, string slug)
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

        string root = TempDir("repovault-metrics-", tag);
        var catalog = new RepositoryCatalogService(_db, root);
        var (rok, rmsg, repo) = await catalog.CreateRepositoryAsync(
            faculty!, course!.CourseId, slug, "Repo " + tag, null,
            DateTime.UtcNow.AddDays(7));
        Assert.True(rok, rmsg);
        return (faculty!, course, repo!);
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
    public async Task EmptyRepo_ScanAndIndex_ReturnZeroes()
    {
        string tag = Tag();
        var (_, _, repo) = await MakeDbRepoAsync(tag, "empty" + tag);
        await GitProcessRunner.InitializeBareRepositoryAsync(repo.BareRepoDiskPath);

        var indexer = new CommitLogIndexer(_db);
        Assert.Equal(0, await indexer.IndexCommitsAsync(repo.RepoId, repo.BareRepoDiskPath));

        var scanner = new CodeMetricsScanner(_db);
        RepositoryMetricsResult got =
            await scanner.ScanRepositoryMetricsAsync(repo.RepoId, repo.BareRepoDiskPath);
        Assert.Equal(0, got.TotalFiles);
        Assert.Equal(0, got.SourceLines);
        Assert.Null(got.PrimaryLanguage);

        await using var conn = await _db.CreateOpenConnectionAsync();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT TotalFiles, SourceLines, PrimaryLanguage FROM Repositories WHERE RepoId = @r;";
        cmd.Parameters.AddWithValue("@r", repo.RepoId);
        await using var reader = await cmd.ExecuteReaderAsync();
        Assert.True(await reader.ReadAsync());
        Assert.Equal(0, Convert.ToInt32(reader["TotalFiles"]));
        Assert.True(reader.IsDBNull(reader.GetOrdinal("PrimaryLanguage")));
    }

    [Fact]
    public async Task EndToEnd_ScanAndIndex_MixedFiles()
    {
        string tag = Tag();
        var (_, _, repo) = await MakeDbRepoAsync(tag, "mixed" + tag);
        await GitProcessRunner.InitializeBareRepositoryAsync(repo.BareRepoDiskPath);

        string client = TempDir("repovault-mcli-", tag);
        Directory.CreateDirectory(client);
        Assert.Equal(0, await GitAsync(client, "init", "--initial-branch=main", "."));
        Assert.Equal(0, await GitAsync(client, "config", "user.name", "Test"));
        Assert.Equal(0, await GitAsync(client, "config", "user.email", "test@test.local"));

        string cs =
            "using System;\n\n// line comment\nvar x = 1; // trailing\n" +
            "/* block\n   continued */\nvar y = 2; /* inline */ var z = 3;\n";
        string sql = "-- comment\nSELECT 1; -- get one\n/* multi\nline */\n";
        string md = "# Title\n\nSome text.\n";
        await File.WriteAllTextAsync(Path.Combine(client, "Program.cs"), cs);
        await File.WriteAllTextAsync(Path.Combine(client, "query.sql"), sql);
        await File.WriteAllTextAsync(Path.Combine(client, "notes.md"), md);
        Assert.Equal(0, await GitAsync(client, "add", "."));
        Assert.Equal(0, await GitAsync(client, "commit", "-m", "add sample files"));
        string sha = await GitOutputAsync(client, "rev-parse", "HEAD");
        Assert.Equal(0, await GitAsync(client, "push", repo.BareRepoDiskPath, "main"));

        var indexer = new CommitLogIndexer(_db);
        Assert.Equal(1, await indexer.IndexCommitsAsync(repo.RepoId, repo.BareRepoDiskPath));
        Assert.Equal(0, await indexer.IndexCommitsAsync(repo.RepoId, repo.BareRepoDiskPath));

        await using (var conn = await _db.CreateOpenConnectionAsync())
        {
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT CommitHash, AuthorName, AuthorEmail, CommitMessage FROM CommitLogs WHERE RepoId = @r;";
            cmd.Parameters.AddWithValue("@r", repo.RepoId);
            await using var reader = await cmd.ExecuteReaderAsync();
            Assert.True(await reader.ReadAsync());
            Assert.Equal(sha, reader["CommitHash"]?.ToString());
            Assert.Equal("Test", reader["AuthorName"]?.ToString());
            Assert.Equal("test@test.local", reader["AuthorEmail"]?.ToString());
            Assert.Equal("add sample files", reader["CommitMessage"]?.ToString());
            Assert.False(await reader.ReadAsync());
        }

        var scanner = new CodeMetricsScanner(_db);
        RepositoryMetricsResult got =
            await scanner.ScanRepositoryMetricsAsync(repo.RepoId, repo.BareRepoDiskPath);
        Assert.Equal(3, got.TotalFiles);
        Assert.Equal(6, got.SourceLines);
        Assert.Equal(6, got.CommentLines);
        Assert.Equal(2, got.BlankLines);
        Assert.Equal("C#", got.PrimaryLanguage);

        await using (var conn2 = await _db.CreateOpenConnectionAsync())
        {
            await using var cmd = conn2.CreateCommand();
            cmd.CommandText = "SELECT LanguageName, FileCount, SourceLines, CommentLines, BlankLines, PercentageShare " +
                              "FROM RepositoryLanguages WHERE RepoId = @r ORDER BY SourceLines DESC;";
            cmd.Parameters.AddWithValue("@r", repo.RepoId);
            await using var reader = await cmd.ExecuteReaderAsync();

            Assert.True(await reader.ReadAsync());
            Assert.Equal("C#", reader["LanguageName"]?.ToString());
            Assert.Equal(3, Convert.ToInt32(reader["SourceLines"]));
            Assert.Equal(50.00m, Convert.ToDecimal(reader["PercentageShare"]));

            Assert.True(await reader.ReadAsync());
            Assert.Equal("Markdown", reader["LanguageName"]?.ToString());
            Assert.Equal(2, Convert.ToInt32(reader["SourceLines"]));
            Assert.Equal(33.33m, Convert.ToDecimal(reader["PercentageShare"]));

            Assert.True(await reader.ReadAsync());
            Assert.Equal("SQL", reader["LanguageName"]?.ToString());
            Assert.Equal(1, Convert.ToInt32(reader["SourceLines"]));
            Assert.Equal(16.67m, Convert.ToDecimal(reader["PercentageShare"]));

            Assert.False(await reader.ReadAsync());
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

    private static async Task<bool> WaitForAsync(Func<Task<bool>> check, TimeSpan timeout)
    {
        DateTime end = DateTime.UtcNow + timeout;
        while (DateTime.UtcNow < end)
        {
            if (await check())
            {
                return true;
            }

            await Task.Delay(250);
        }

        return false;
    }

    [Fact]
    public async Task ServerPush_AutoIndexes_CommitsAndMetrics()
    {
        string tag = Tag();
        string password = "Passw0rd!" + tag;
        var auth = new AuthService(_db);

        var (fok, _, faculty) = await auth.RegisterUserAsync(
            "IDF" + tag, "uF" + tag, password, "Fac " + tag,
            "uF" + tag + "@example.com", UserRole.Faculty);
        Assert.True(fok);
        var (sok, _, student) = await auth.RegisterUserAsync(
            "IDS" + tag, "uS" + tag, password, "Stu " + tag,
            "uS" + tag + "@example.com", UserRole.Student);
        Assert.True(sok);

        var courses = new CourseService(_db);
        var (_, _, course) = await courses.CreateCourseAsync(
            faculty!, "C" + tag, "Course " + tag, "CSD", "2026-1st");

        string root = TempDir("repovault-wired-", tag);
        var catalog = new RepositoryCatalogService(_db, root);
        var (rok, _, repo) = await catalog.CreateRepositoryAsync(
            faculty!, course!.CourseId, "wired" + tag, "Wired", null,
            DateTime.UtcNow.AddDays(7));
        Assert.True(rok);

        var contributors = new ContributorService(_db);
        Assert.True((await contributors.AssignContributorAsync(
            faculty!, repo!.RepoId, student!.UserId)).Ok);

        int port = FreePort();
        _server = new GitHttpServer(_db, auth, new RbacGuard(_db), catalog, port);
        await _server.StartAsync();

        string client = TempDir("repovault-wcli-", tag);
        Directory.CreateDirectory(client);
        Assert.Equal(0, await GitAsync(client, "init", "--initial-branch=main", "."));
        Assert.Equal(0, await GitAsync(client, "config", "user.name", "Test"));
        Assert.Equal(0, await GitAsync(client, "config", "user.email", "test@test.local"));
        await File.WriteAllTextAsync(Path.Combine(client, "App.cs"), "var x = 1;\n// note\n");
        Assert.Equal(0, await GitAsync(client, "add", "."));
        Assert.Equal(0, await GitAsync(client, "commit", "-m", "wired push"));

        string url = $"http://{student!.Username}:{password}@127.0.0.1:{port}/repos/wired{tag}.git";
        Assert.Equal(0, await GitAsync(client, "push", url, "main"));

        bool indexed = await WaitForAsync(async () =>
        {
            await using var conn = await _db.CreateOpenConnectionAsync();
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT COUNT(*) FROM CommitLogs WHERE RepoId = @r;";
            cmd.Parameters.AddWithValue("@r", repo.RepoId);
            return Convert.ToInt32(await cmd.ExecuteScalarAsync()) > 0;
        }, TimeSpan.FromSeconds(20));
        Assert.True(indexed);

        bool measured = await WaitForAsync(async () =>
        {
            RepositoryRecord? fresh = await catalog.GetByIdAsync(repo.RepoId);
            return fresh?.PrimaryLanguage == "C#";
        }, TimeSpan.FromSeconds(20));
        Assert.True(measured);
    }
}
