namespace RepoVault.Core.Models;

public sealed class CommitLogEntry
{
    public long CommitId { get; set; }
    public int RepoId { get; set; }
    public string CommitHash { get; set; } = string.Empty;
    public string AuthorName { get; set; } = string.Empty;
    public string AuthorEmail { get; set; } = string.Empty;
    public DateTime CommittedAt { get; set; }
    public string CommitMessage { get; set; } = string.Empty;
}
