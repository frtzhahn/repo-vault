namespace RepoVault.Core.Models;

public sealed class PushLedgerEntry
{
    public long PushId { get; set; }
    public int RepoId { get; set; }
    public int PusherUserId { get; set; }
    public string BranchRef { get; set; } = string.Empty;
    public string OldCommitHash { get; set; } = string.Empty;
    public string NewCommitHash { get; set; } = string.Empty;
    public string? ClientIpAddress { get; set; }
    public DateTime PushedAt { get; set; }
}
