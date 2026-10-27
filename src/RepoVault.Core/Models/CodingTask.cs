namespace RepoVault.Core.Models;

public sealed class CodingTask
{
    public int TaskId { get; set; }
    public int CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Instructions { get; set; }
    public string StarterTemplate { get; set; } = "csharp";
    public DateTime? DeadlineUtc { get; set; }
    public bool AllowGroupWork { get; set; }
    public int MaxGroupSize { get; set; } = 1;
    public int CreatedByFacultyId { get; set; }
    public DateTime CreatedAt { get; set; }
}
