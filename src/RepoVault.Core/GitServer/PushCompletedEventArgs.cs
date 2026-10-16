namespace RepoVault.Core.GitServer;

public sealed class PushCompletedEventArgs : EventArgs
{
    public int RepoId { get; init; }
    public int PusherUserId { get; init; }
    public string BranchRef { get; init; } = string.Empty;
    public string OldCommitHash { get; init; } = string.Empty;
    public string NewCommitHash { get; init; } = string.Empty;
    public string? ClientIpAddress { get; init; }
    public DateTime PushedAt { get; init; }
}
