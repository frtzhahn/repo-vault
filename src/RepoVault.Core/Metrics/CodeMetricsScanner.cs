using System.Diagnostics;
using System.Text;
using RepoVault.Core.Data;
using RepoVault.Core.GitServer;

namespace RepoVault.Core.Metrics;

public sealed record RepositoryMetricsResult(
    int TotalFiles, int SourceLines, int CommentLines, int BlankLines, string? PrimaryLanguage);

public sealed class CodeMetricsScanner
{
    private readonly DbHelper _db;

    public CodeMetricsScanner(DbHelper db)
    {
        _db = db;
    }

    public async Task<RepositoryMetricsResult> ScanRepositoryMetricsAsync(
        int repoId, string bareRepoPath, CancellationToken ct = default)
    {
        var tree = await GitProcessRunner.RunAsync(
            ["-C", bareRepoPath, "ls-tree", "-r", "-l", "--full-tree", "HEAD"],
            null, null, ct).ConfigureAwait(false);

        // exit 128 means HEAD does not resolve yet, treat like an empty repo
        var blobs = new List<(string Sha, string Path)>();
        if (tree.ExitCode == 0)
        {
            foreach (string line in tree.Stdout.Split('\n', StringSplitOptions.RemoveEmptyEntries))
            {
                int tab = line.IndexOf('\t');
                if (tab < 0)
                {
                    continue;
                }

                string[] meta = line.Substring(0, tab)
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (meta.Length < 4 || meta[1] != "blob")
                {
                    continue;
                }

                // skip submodules and symlinks, only real files are counted
                if (meta[0] == "160000" || meta[0] == "120000")
                {
                    continue;
                }

                blobs.Add((meta[2], line.Substring(tab + 1)));
            }
        }

        var files = new List<FileMetricsResult>();
        if (blobs.Count > 0)
        {
            files = await ReadBlobsAsync(bareRepoPath, blobs, ct).ConfigureAwait(false);
        }

        return await PersistMetricsAsync(repoId, files, ct).ConfigureAwait(false);
    }

    private static async Task<List<FileMetricsResult>> ReadBlobsAsync(
        string bareRepoPath, List<(string Sha, string Path)> blobs, CancellationToken ct)
    {
        var files = new List<FileMetricsResult>();
        using var proc = new Process
        {
            StartInfo = GitProcessRunner.BuildStartInfo(["-C", bareRepoPath, "cat-file", "--batch"]),
        };
        proc.Start();

        // feed shas while the reader below drains stdout, so big repos never block
        Task writer = Task.Run(async () =>
        {
            foreach (var (sha, _) in blobs)
            {
                byte[] line = Encoding.ASCII.GetBytes(sha + "\n");
                await proc.StandardInput.BaseStream.WriteAsync(line, ct).ConfigureAwait(false);
            }

            proc.StandardInput.Close();
        }, ct);

        Stream stdout = proc.StandardOutput.BaseStream;
        for (int i = 0; i < blobs.Count; i++)
        {
            string? header = await ReadHeaderLineAsync(stdout, ct).ConfigureAwait(false);
            if (header is null)
            {
                break;
            }

            string[] parts = header.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 3 || parts[1] != "blob" || !int.TryParse(parts[2], out int size))
            {
                continue;
            }

            byte[] content = await ReadExactAsync(stdout, size, ct).ConfigureAwait(false);
            await ReadExactAsync(stdout, 1, ct).ConfigureAwait(false);

            FileMetricsResult metrics = LineCounter.ClassifyFile(blobs[i].Path, content);
            if (!metrics.IsBinary)
            {
                files.Add(metrics);
            }
        }

        await writer.ConfigureAwait(false);
        await proc.WaitForExitAsync(ct).ConfigureAwait(false);
        return files;
    }

    private static async Task<string?> ReadHeaderLineAsync(Stream stdout, CancellationToken ct)
    {
        var bytes = new List<byte>(64);
        var one = new byte[1];
        while (true)
        {
            int read = await stdout.ReadAsync(one, ct).ConfigureAwait(false);
            if (read == 0)
            {
                return bytes.Count == 0 ? null : Encoding.ASCII.GetString(bytes.ToArray());
            }

            if (one[0] == (byte)'\n')
            {
                return Encoding.ASCII.GetString(bytes.ToArray());
            }

            bytes.Add(one[0]);
        }
    }

    private static async Task<byte[]> ReadExactAsync(Stream stdout, int size, CancellationToken ct)
    {
        var content = new byte[size];
        int done = 0;
        while (done < size)
        {
            int read = await stdout.ReadAsync(content.AsMemory(done, size - done), ct).ConfigureAwait(false);
            if (read == 0)
            {
                throw new InvalidOperationException("git cat-file ended early.");
            }

            done += read;
        }

        return content;
    }

    private async Task<RepositoryMetricsResult> PersistMetricsAsync(
        int repoId, List<FileMetricsResult> files, CancellationToken ct)
    {
        var langs = files
            .GroupBy(f => f.Language)
            .Select(g => new
            {
                Language = g.Key,
                Files = g.Count(),
                Source = g.Sum(f => f.SourceLines),
                Comment = g.Sum(f => f.CommentLines),
                Blank = g.Sum(f => f.BlankLines),
            })
            .OrderByDescending(l => l.Source)
            .ThenByDescending(l => l.Files)
            .ToList();

        int totalSource = langs.Sum(l => l.Source);
        string? primary = langs.Count == 0 ? null : langs[0].Language;

        await using var conn = await _db.CreateOpenConnectionAsync(ct).ConfigureAwait(false);
        await using var tx = await conn.BeginTransactionAsync(ct).ConfigureAwait(false);
        try
        {
            await using (var clear = conn.CreateCommand())
            {
                clear.Transaction = tx;
                clear.CommandText = "DELETE FROM RepositoryLanguages WHERE RepoId = @r;";
                clear.Parameters.AddWithValue("@r", repoId);
                await clear.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
            }

            if (langs.Count > 0)
            {
                await using var ins = conn.CreateCommand();
                ins.Transaction = tx;

                var values = new StringBuilder(
                    "INSERT INTO RepositoryLanguages (RepoId, LanguageName, FileCount, SourceLines, " +
                    "CommentLines, BlankLines, PercentageShare) VALUES ");
                for (int i = 0; i < langs.Count; i++)
                {
                    if (i > 0)
                    {
                        values.Append(", ");
                    }

                    values.Append($"(@r, @l{i}, @f{i}, @s{i}, @c{i}, @b{i}, @p{i})");
                    ins.Parameters.AddWithValue($"@l{i}", langs[i].Language);
                    ins.Parameters.AddWithValue($"@f{i}", langs[i].Files);
                    ins.Parameters.AddWithValue($"@s{i}", langs[i].Source);
                    ins.Parameters.AddWithValue($"@c{i}", langs[i].Comment);
                    ins.Parameters.AddWithValue($"@b{i}", langs[i].Blank);
                    decimal share = totalSource == 0
                        ? 0m
                        : Math.Round((decimal)langs[i].Source * 100m / totalSource, 2);
                    ins.Parameters.AddWithValue($"@p{i}", share);
                }

                ins.CommandText = values.ToString() + ";";
                ins.Parameters.AddWithValue("@r", repoId);
                await ins.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
            }

            await using (var repo = conn.CreateCommand())
            {
                repo.Transaction = tx;
                repo.CommandText = "UPDATE Repositories SET TotalFiles = @f, SourceLines = @s, " +
                                   "CommentLines = @c, BlankLines = @b, PrimaryLanguage = @p " +
                                   "WHERE RepoId = @r;";
                repo.Parameters.AddWithValue("@f", files.Count);
                repo.Parameters.AddWithValue("@s", langs.Sum(l => l.Source));
                repo.Parameters.AddWithValue("@c", langs.Sum(l => l.Comment));
                repo.Parameters.AddWithValue("@b", langs.Sum(l => l.Blank));
                repo.Parameters.AddWithValue("@p", (object?)primary ?? DBNull.Value);
                repo.Parameters.AddWithValue("@r", repoId);
                await repo.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
            }

            await tx.CommitAsync(ct).ConfigureAwait(false);
        }
        catch
        {
            await tx.RollbackAsync(ct).ConfigureAwait(false);
            throw;
        }

        return new RepositoryMetricsResult(
            files.Count, langs.Sum(l => l.Source), langs.Sum(l => l.Comment),
            langs.Sum(l => l.Blank), primary);
    }
}
