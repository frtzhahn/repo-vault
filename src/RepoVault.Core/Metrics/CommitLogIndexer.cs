using RepoVault.Core.Data;
using RepoVault.Core.GitServer;

namespace RepoVault.Core.Metrics;

public sealed class CommitLogIndexer
{
    private readonly DbHelper _db;

    public CommitLogIndexer(DbHelper db)
    {
        _db = db;
    }

    // returns how many new commits were inserted
    public async Task<int> IndexCommitsAsync(
        int repoId, string bareRepoPath, CancellationToken ct = default)
    {
        var log = await GitProcessRunner.RunAsync(
            ["-C", bareRepoPath, "log", "HEAD", "--format=%H%x1f%an%x1f%ae%x1f%at%x1f%s"],
            null, null, ct).ConfigureAwait(false);

        // exit 128 means HEAD does not resolve yet, the repo is empty
        if (log.ExitCode != 0)
        {
            return 0;
        }

        await using var conn = await _db.CreateOpenConnectionAsync(ct).ConfigureAwait(false);
        int added = 0;

        foreach (string line in log.Stdout.Split('\n', StringSplitOptions.RemoveEmptyEntries))
        {
            string[] parts = line.Split('\x1f');
            if (parts.Length < 5)
            {
                continue;
            }

            if (!long.TryParse(parts[3].Trim(), out long stamp))
            {
                continue;
            }

            // the subject itself could contain the separator, glue it back
            string message = string.Join('\x1f', parts, 4, parts.Length - 4);
            DateTime committed = DateTimeOffset.FromUnixTimeSeconds(stamp).UtcDateTime;

            await using var cmd = conn.CreateCommand();
            cmd.CommandText = "INSERT IGNORE INTO CommitLogs (RepoId, CommitHash, AuthorName, " +
                              "AuthorEmail, CommittedAt, CommitMessage) " +
                              "VALUES (@r, @h, @n, @e, @d, @m);";
            cmd.Parameters.AddWithValue("@r", repoId);
            cmd.Parameters.AddWithValue("@h", parts[0].Trim());
            cmd.Parameters.AddWithValue("@n", parts[1].Trim());
            cmd.Parameters.AddWithValue("@e", parts[2].Trim());
            cmd.Parameters.AddWithValue("@d", committed);
            cmd.Parameters.AddWithValue("@m", message);
            added += await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
        }

        return added;
    }
}
