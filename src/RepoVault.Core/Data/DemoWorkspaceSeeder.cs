using System.Diagnostics;
using RepoVault.Core.GitServer;
using RepoVault.Core.Metrics;
using RepoVault.Core.Models;
using RepoVault.Core.Services;

namespace RepoVault.Core.Data;

public sealed record SeederResult(int ReposSeeded, int CommitsIndexed);

public sealed class DemoWorkspaceSeeder
{
    private readonly DbHelper _db;

    public DemoWorkspaceSeeder(DbHelper db)
    {
        _db = db;
    }

    public async Task<SeederResult> EnsureDemoSeededAsync(
        string baseRepoRoot, CancellationToken ct = default)
    {
        Directory.CreateDirectory(baseRepoRoot);
        DateTime now = DateTime.UtcNow;
        DateTime rolledDeadline = now.AddDays(14);

        await using var conn = await _db.CreateOpenConnectionAsync(ct).ConfigureAwait(false);

        // roll expired unsubmitted demo repo deadlines forward while keeping locked/submitted exam repos untouched
        await using (var rollRepos = conn.CreateCommand())
        {
            rollRepos.CommandText =
                "UPDATE Repositories SET DeadlineUtc = @future " +
                "WHERE IsLocked = 0 AND IsSubmitted = 0 AND DeadlineUtc IS NOT NULL AND DeadlineUtc < @now;";
            rollRepos.Parameters.AddWithValue("@future", rolledDeadline);
            rollRepos.Parameters.AddWithValue("@now", now);
            await rollRepos.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
        }

        // roll expired coding task deadlines forward except the locked midterm exam task
        await using (var rollTasks = conn.CreateCommand())
        {
            rollTasks.CommandText =
                "UPDATE CodingTasks SET DeadlineUtc = @future " +
                "WHERE DeadlineUtc IS NOT NULL AND DeadlineUtc < @now AND Title NOT LIKE 'Midterm Lab%';";
            rollTasks.Parameters.AddWithValue("@future", rolledDeadline);
            rollTasks.Parameters.AddWithValue("@now", now);
            await rollTasks.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
        }

        var rows = new List<(int RepoId, string Slug, string DiskPath, int TotalFiles)>();
        await using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText =
                "SELECT RepoId, RepoSlug, BareRepoDiskPath, TotalFiles " +
                "FROM Repositories ORDER BY RepoId;";
            await using var reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false);
            while (await reader.ReadAsync(ct).ConfigureAwait(false))
            {
                rows.Add((
                    Convert.ToInt32(reader["RepoId"]),
                    reader["RepoSlug"]?.ToString() ?? string.Empty,
                    reader["BareRepoDiskPath"]?.ToString() ?? string.Empty,
                    Convert.ToInt32(reader["TotalFiles"])));
            }
        }

        int reposSeeded = 0;
        int commitsIndexed = 0;
        foreach (var row in rows)
        {
            // preserve existing rooted paths (such as student task repos at <repoId>)
            bool hasExistingRootedDir =
                !string.IsNullOrWhiteSpace(row.DiskPath) &&
                Path.IsPathRooted(row.DiskPath) &&
                Directory.Exists(row.DiskPath);

            string targetPath = hasExistingRootedDir
                ? row.DiskPath
                : Path.Combine(baseRepoRoot, RepoService.NormalizeSlug(row.Slug) + ".git");

            bool needsInit = !Directory.Exists(targetPath);
            if (!needsInit)
            {
                Dictionary<string, string> refs =
                    await GitProcessRunner.SnapshotRefsAsync(targetPath, ct).ConfigureAwait(false);
                needsInit = refs.Count == 0;
            }

            bool pathChanged = !string.Equals(row.DiskPath, targetPath, StringComparison.Ordinal);
            if (needsInit || pathChanged || row.TotalFiles == 0)
            {
                SeederResult one = await SeedRepoInternalAsync(
                    row.RepoId, row.Slug, row.DiskPath, targetPath, ct).ConfigureAwait(false);
                reposSeeded += one.ReposSeeded;
                commitsIndexed += one.CommitsIndexed;
            }
        }

        return new SeederResult(reposSeeded, commitsIndexed);
    }

    public async Task<SeederResult> SeedAllAsync(
        string baseRepoRoot, CancellationToken ct = default)
    {
        await using var conn = await _db.CreateOpenConnectionAsync(ct).ConfigureAwait(false);
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT RepoId FROM Repositories ORDER BY RepoId;";

        var ids = new List<int>();
        await using (var reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false))
        {
            while (await reader.ReadAsync(ct).ConfigureAwait(false))
            {
                ids.Add(Convert.ToInt32(reader["RepoId"]));
            }
        }

        int repos = 0;
        int commits = 0;
        foreach (int id in ids)
        {
            SeederResult one = await SeedRepoAsync(id, baseRepoRoot, ct).ConfigureAwait(false);
            repos += one.ReposSeeded;
            commits += one.CommitsIndexed;
        }

        return new SeederResult(repos, commits);
    }

    public async Task<SeederResult> SeedRepoAsync(
        int repoId, string baseRepoRoot, CancellationToken ct = default)
    {
        var catalog = new RepoService(_db);
        RepositoryRecord? repo = await catalog.GetByIdAsync(repoId, ct).ConfigureAwait(false);
        if (repo is null)
        {
            throw new InvalidOperationException("repository not found.");
        }

        string diskPath = Path.Combine(
            baseRepoRoot, RepoService.NormalizeSlug(repo.RepoSlug) + ".git");

        return await SeedRepoInternalAsync(
            repoId, repo.RepoSlug, repo.BareRepoDiskPath, diskPath, ct).ConfigureAwait(false);
    }

    private async Task<SeederResult> SeedRepoInternalAsync(
        int repoId, string slug, string currentDiskPath, string targetDiskPath, CancellationToken ct)
    {
        if (!Directory.Exists(targetDiskPath))
        {
            await GitProcessRunner.InitializeBareRepositoryAsync(targetDiskPath, ct).ConfigureAwait(false);
        }

        Dictionary<string, string> refs =
            await GitProcessRunner.SnapshotRefsAsync(targetDiskPath, ct).ConfigureAwait(false);
        if (refs.Count == 0)
        {
            await PushInitialCommitAsync(targetDiskPath, slug, ct).ConfigureAwait(false);
        }

        if (!string.Equals(currentDiskPath, targetDiskPath, StringComparison.Ordinal))
        {
            await using var conn = await _db.CreateOpenConnectionAsync(ct).ConfigureAwait(false);
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = "UPDATE Repositories SET BareRepoDiskPath = @p WHERE RepoId = @id;";
            cmd.Parameters.AddWithValue("@p", targetDiskPath);
            cmd.Parameters.AddWithValue("@id", repoId);
            await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
        }

        int commits = await new CommitLogIndexer(_db)
            .IndexCommitsAsync(repoId, targetDiskPath, ct).ConfigureAwait(false);
        await new CodeMetricsScanner(_db)
            .ScanRepositoryMetricsAsync(repoId, targetDiskPath, ct).ConfigureAwait(false);

        return new SeederResult(1, commits);
    }

    private static async Task PushInitialCommitAsync(
        string barePath, string slug, CancellationToken ct)
    {
        string work = Path.Combine(Path.GetTempPath(), "repovault-seed-" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(work);
        try
        {
            AssertExit(await RunGitAsync(work, ct, "init", "--initial-branch=main", "."), "init");
            AssertExit(await RunGitAsync(work, ct, "config", "user.name", "RepoVault Seeder"), "config");
            AssertExit(await RunGitAsync(work, ct, "config", "user.email", "seeder@ucc.edu.ph"), "config");

            foreach (var (name, body) in SampleFiles(slug))
            {
                await File.WriteAllTextAsync(Path.Combine(work, name), body, ct).ConfigureAwait(false);
            }

            AssertExit(await RunGitAsync(work, ct, "add", "."), "add");
            AssertExit(await RunGitAsync(work, ct, "commit", "-m", "initial commit"), "commit");
            AssertExit(await RunGitAsync(work, ct, "push", barePath, "HEAD:main"), "push");
        }
        finally
        {
            try
            {
                if (Directory.Exists(work))
                {
                    foreach (string file in Directory.GetFiles(work, "*", SearchOption.AllDirectories))
                    {
                        File.SetAttributes(file, File.GetAttributes(file) & ~FileAttributes.ReadOnly);
                    }

                    Directory.Delete(work, true);
                }
            }
            catch
            {
            }
        }
    }

    private static async Task<int> RunGitAsync(string dir, CancellationToken ct, params string[] args)
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
        Task<string> stdout = proc.StandardOutput.ReadToEndAsync(ct);
        Task<string> stderr = proc.StandardError.ReadToEndAsync(ct);
        await proc.WaitForExitAsync(ct).ConfigureAwait(false);
        await stdout.ConfigureAwait(false);
        await stderr.ConfigureAwait(false);
        return proc.ExitCode;
    }

    private static void AssertExit(int code, string step)
    {
        if (code != 0)
        {
            throw new InvalidOperationException("git " + step + " failed.");
        }
    }

    private static Dictionary<string, string> SampleFiles(string slug)
    {
        string readme = "# " + slug + "\n\nSeeded demo project for UCC BSCS 2A.\n";
        if (string.Equals(slug, "randfacts", StringComparison.OrdinalIgnoreCase))
        {
            return new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["README.md"] = readme,
                ["Main.java"] = "import java.util.*;\n\n// random fact picker\npublic class Main {\n    public static void main(String[] args) {\n        List<String> facts = List.of(\"java runs everywhere\");\n        /* print one fact */\n        System.out.println(facts.get(0));\n    }\n}\n",
            };
        }

        if (string.Equals(slug, "web-archive", StringComparison.OrdinalIgnoreCase))
        {
            return new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["README.md"] = readme,
                ["app.js"] = "// tiny archive index\nconst fs = require(\"fs\");\n\nfunction list(dir) {\n    /* read entries */\n    return fs.readdirSync(dir);\n}\n\nmodule.exports = { list };\n",
            };
        }

        return new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["README.md"] = readme,
            ["Program.cs"] = "using System;\n\n// demo entry point\nvar items = new[] { 1, 2, 3 };\nforeach (int n in items)\n{\n    /* print each number */\n    Console.WriteLine(n);\n}\n",
        };
    }
}
