namespace RepoVault.Core.Models;

public sealed class EvaluationRecord
{
    public int EvaluationId { get; set; }
    public int RepoId { get; set; }
    public int FacultyUserId { get; set; }
    public int ArchitectureScore { get; set; }
    public int FunctionalityScore { get; set; }
    public int CodeQualityScore { get; set; }
    public int DocumentationScore { get; set; }
    public int TotalScore { get; set; }
    public string? CritiqueNotes { get; set; }
    public DateTime EvaluatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
