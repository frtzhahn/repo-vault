namespace RepoVault.Core.Models;

public sealed class RepositoryContributor
{
    public int RepoId { get; set; }
    public int StudentUserId { get; set; }
    public string TeamRole { get; set; } = string.Empty;
    public DateTime AssignedAt { get; set; }
}
