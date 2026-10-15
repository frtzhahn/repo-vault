namespace RepoVault.Core.Models;

public sealed class RepositoryRecord
{
    public int RepoId { get; set; }
    public int CourseId { get; set; }
    public string RepoSlug { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string BareRepoDiskPath { get; set; } = string.Empty;
    public DateTime? DeadlineUtc { get; set; }
    public bool IsLocked { get; set; }
    public LifecycleStatus LifecycleStatus { get; set; }
    public DateTime? TrashedAt { get; set; }
    public int TotalFiles { get; set; }
    public int SourceLines { get; set; }
    public int CommentLines { get; set; }
    public int BlankLines { get; set; }
    public string? PrimaryLanguage { get; set; }
    public DateTime CreatedAt { get; set; }
}
