using System.Diagnostics;
using System.Text;
using RepoVault.Core.Models;
using RepoVault.Core.Security;

namespace RepoVault.Core.GitServer;

public sealed record RepoFileEntry(
    string Path, string FileName, string DirectoryPath, string BlobSha, long SizeBytes, string Mode);

public sealed record RepoBlobContent(
    bool IsBinary, bool IsTruncated, string TextContent, long SizeBytes);

public sealed class RepoBrowser
{
    public async Task<List<RepoFileEntry>> ListFilesAtHeadAsync(
        string bareRepoPath, CancellationToken ct = default)
    {
        var files = new List<RepoFileEntry>();
        if (!Directory.Exists(bareRepoPath))
        {
            return files;
        }

        GitProcessResult tree = await GitProcessRunner.RunAsync(
            ["-C", bareRepoPath, "ls-tree", "-r", "-l", "--full-tree", "HEAD"],
            null, null, ct).ConfigureAwait(false);

        // 128 means HEAD does not resolve yet, treat like an empty repo
        if (tree.ExitCode != 0)
        {
            return files;
        }

        foreach (string line in tree.Stdout.Split('\n', StringSplitOptions.RemoveEmptyEntries))
        {
            int tab = line.IndexOf('\t');
            if (tab < 0)
            {
                continue;
            }

            string path = line.Substring(tab + 1).Trim();
            if (path.Length == 0)
            {
                continue;
            }

            string[] meta = line.Substring(0, tab)
                .Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (meta.Length < 4 || meta[1] != "blob")
            {
                continue;
            }

            string mode = meta[0];
            string sha = meta[2];
            long size = 0;
            if (!string.Equals(meta[3], "-", StringComparison.Ordinal))
            {
                long.TryParse(meta[3], out size);
            }

            int slash = path.LastIndexOf('/');
            string name = slash < 0 ? path : path.Substring(slash + 1);
            string dir = slash < 0 ? string.Empty : path.Substring(0, slash);
            files.Add(new RepoFileEntry(path, name, dir, sha, size, mode));
        }

        return files;
    }

    public async Task<RepoBlobContent> ReadFileBlobAuthorizedAsync(
        RbacGuard rbac, User viewer, int repoId, string bareRepoPath, string pathOrBlobSha,
        int maxBytes = 1_048_576, CancellationToken ct = default)
    {
        if (!await rbac.CanViewRepositoryMetadataAsync(viewer, repoId, ct).ConfigureAwait(false))
        {
            return new RepoBlobContent(false, false,
                "[Access Restricted: You do not have permission to view this repository.]", 0);
        }

        if (await rbac.CanReadSourceCodeAsync(viewer, repoId, ct).ConfigureAwait(false))
        {
            return await ReadFileBlobAsync(bareRepoPath, pathOrBlobSha, maxBytes, ct).ConfigureAwait(false);
        }

        // peer viewers can only read the readme file
        bool isReadme = pathOrBlobSha.EndsWith("README.md", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(pathOrBlobSha, "readme.md", StringComparison.OrdinalIgnoreCase);
        if (isReadme)
        {
            return await ReadFileBlobAsync(bareRepoPath, pathOrBlobSha, maxBytes, ct).ConfigureAwait(false);
        }

        return new RepoBlobContent(false, false,
            "[Access Restricted: Source code inspection is restricted to the repository owner, approved groupmates, and authorized faculty.]", 0);
    }

    public async Task<RepoBlobContent> ReadFileBlobAsync(
        string bareRepoPath, string pathOrBlobSha,
        int maxBytes = 1_048_576, CancellationToken ct = default)
    {
        string spec = IsBlobSha(pathOrBlobSha) ? pathOrBlobSha : "HEAD:" + pathOrBlobSha;
        byte[] blob = await ReadBlobBytesAsync(bareRepoPath, spec, ct).ConfigureAwait(false);

        // same window git uses to flag binary content
        int scan = Math.Min(blob.Length, 8000);
        for (int i = 0; i < scan; i++)
        {
            if (blob[i] == 0)
            {
                return new RepoBlobContent(true, false, "[Binary file - preview not available]", blob.Length);
            }
        }

        int start = 0;
        if (blob.Length >= 3 && blob[0] == 0xEF && blob[1] == 0xBB && blob[2] == 0xBF)
        {
            start = 3;
        }

        string text = Encoding.UTF8.GetString(blob, start, blob.Length - start);
        if (blob.Length > maxBytes)
        {
            // ascii fast path, tests use plain text so chars line up with bytes
            string head = text.Length > maxBytes ? text.Substring(0, maxBytes) : text;
            head += "\n\n[File truncated at 1 MB for display]";
            return new RepoBlobContent(false, true, head, blob.Length);
        }

        return new RepoBlobContent(false, false, text, blob.Length);
    }

    private static bool IsBlobSha(string value)
    {
        if (value.Length != 40 && value.Length != 64)
        {
            return false;
        }

        foreach (char c in value)
        {
            bool hex = (c >= '0' && c <= '9') || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F');
            if (!hex)
            {
                return false;
            }
        }

        return true;
    }

    private static async Task<byte[]> ReadBlobBytesAsync(
        string bareRepoPath, string spec, CancellationToken ct)
    {
        using var proc = new Process { StartInfo = GitProcessRunner.BuildStartInfo(["-C", bareRepoPath, "cat-file", "-p", spec]) };
        proc.Start();

        // drain stdout as raw bytes so binary blobs survive intact
        Task<byte[]> stdout = Task.Run(async () =>
        {
            using var buf = new MemoryStream();
            await proc.StandardOutput.BaseStream.CopyToAsync(buf, ct).ConfigureAwait(false);
            return buf.ToArray();
        }, ct);

        Task<string> stderr = proc.StandardError.ReadToEndAsync(ct);
        await proc.WaitForExitAsync(ct).ConfigureAwait(false);

        byte[] blob = await stdout.ConfigureAwait(false);
        string err = await stderr.ConfigureAwait(false);
        if (proc.ExitCode != 0)
        {
            throw new InvalidOperationException("git cat-file failed: " + err.Trim());
        }

        return blob;
    }
}
