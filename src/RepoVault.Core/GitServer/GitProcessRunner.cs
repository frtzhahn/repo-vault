using System.Diagnostics;

namespace RepoVault.Core.GitServer;

public sealed record GitProcessResult(int ExitCode, string Stdout, string Stderr);

public static class GitProcessRunner
{
    public const string ZeroSha = "0000000000000000000000000000000000000000";

    public static ProcessStartInfo BuildStartInfo(IEnumerable<string> args, string? workingDir = null)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "git",
            CreateNoWindow = true,
            UseShellExecute = false,
            WindowStyle = ProcessWindowStyle.Hidden,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };

        if (!string.IsNullOrEmpty(workingDir))
        {
            psi.WorkingDirectory = workingDir;
        }

        psi.Environment["GIT_TERMINAL_PROMPT"] = "0";
        psi.Environment["LC_ALL"] = "C";

        // keep git from touching global config and cap its threads on weak lab machines
        psi.ArgumentList.Add("-c");
        psi.ArgumentList.Add("safe.directory=*");
        psi.ArgumentList.Add("-c");
        psi.ArgumentList.Add("pack.threads=1");
        foreach (string arg in args)
        {
            psi.ArgumentList.Add(arg);
        }

        return psi;
    }

    public static async Task<GitProcessResult> RunAsync(
        IEnumerable<string> args, string? workingDir = null,
        byte[]? stdin = null, CancellationToken ct = default)
    {
        using var proc = new Process { StartInfo = BuildStartInfo(args, workingDir) };
        proc.Start();

        if (stdin is { Length: > 0 })
        {
            await proc.StandardInput.BaseStream.WriteAsync(stdin, ct).ConfigureAwait(false);
        }

        proc.StandardInput.Close();

        Task<string> stdout = proc.StandardOutput.ReadToEndAsync(ct);
        Task<string> stderr = proc.StandardError.ReadToEndAsync(ct);
        await proc.WaitForExitAsync(ct).ConfigureAwait(false);

        return new GitProcessResult(proc.ExitCode, await stdout.ConfigureAwait(false), await stderr.ConfigureAwait(false));
    }

    public static async Task InitializeBareRepositoryAsync(
        string diskPath, CancellationToken ct = default)
    {
        var init = await RunAsync(
            ["init", "--bare", "--initial-branch=main", diskPath], null, null, ct).ConfigureAwait(false);
        if (init.ExitCode != 0)
        {
            throw new InvalidOperationException("git init failed: " + init.Stderr.Trim());
        }

        // block history rewrites and cap push sizes on the server side
        string[][] configs =
        [
            ["config", "receive.denyNonFastForwards", "true"],
            ["config", "receive.denyDeletes", "true"],
            ["config", "receive.maxInputSize", "100m"],
        ];

        foreach (string[] cfg in configs)
        {
            var done = await RunAsync(["-C", diskPath, .. cfg], null, null, ct).ConfigureAwait(false);
            if (done.ExitCode != 0)
            {
                throw new InvalidOperationException("git config failed: " + done.Stderr.Trim());
            }
        }
    }

    public static async Task<Dictionary<string, string>> SnapshotRefsAsync(
        string diskPath, CancellationToken ct = default)
    {
        var refs = await RunAsync(
            ["-C", diskPath, "for-each-ref", "--format=%(objectname) %(refname)"],
            null, null, ct).ConfigureAwait(false);

        var map = new Dictionary<string, string>(StringComparer.Ordinal);
        if (refs.ExitCode != 0)
        {
            return map;
        }

        foreach (string line in refs.Stdout.Split('\n', StringSplitOptions.RemoveEmptyEntries))
        {
            int sep = line.IndexOf(' ');
            if (sep > 0)
            {
                map[line.Substring(sep + 1).Trim()] = line.Substring(0, sep);
            }
        }

        return map;
    }
}
