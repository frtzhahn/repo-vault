namespace RepoVault.Core.Models;

public sealed class RepositoryLanguage
{
    public int RepoId { get; set; }
    public string LanguageName { get; set; } = string.Empty;
    public int FileCount { get; set; }
    public int SourceLines { get; set; }
    public int CommentLines { get; set; }
    public int BlankLines { get; set; }
    public decimal PercentageShare { get; set; }
}
