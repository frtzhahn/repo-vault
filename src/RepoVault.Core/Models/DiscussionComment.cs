namespace RepoVault.Core.Models;

public sealed class DiscussionComment
{
    public int CommentId { get; set; }
    public int RepoId { get; set; }
    public int AuthorUserId { get; set; }
    public string CommentBody { get; set; } = string.Empty;
    public bool IsModeratedHidden { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
