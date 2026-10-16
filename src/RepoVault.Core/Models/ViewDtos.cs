using RepoVault.Core.Models;

namespace RepoVault.Core.Models;

public sealed record UserSummaryDto(
    int UserId, string IdNumber, string Username, string FullName,
    string Email, UserRole Role, bool IsActive, DateTime CreatedAt);

public sealed record PushLedgerViewDto(
    int PushId, int RepoId, int PusherUserId, string PusherUsername, string PusherFullName,
    string BranchRef, string OldCommitHash, string NewCommitHash,
    string? ClientIpAddress, DateTime PushedAt);

public sealed record DiscussionCommentViewDto(
    int CommentId, int RepoId, int AuthorUserId, string AuthorUsername, string AuthorFullName,
    UserRole AuthorRole, string CommentBody, bool IsModeratedHidden,
    DateTime CreatedAt, DateTime? UpdatedAt);

public sealed record ContributorViewDto(
    int RepoId, int StudentUserId, string Username, string FullName,
    string Email, string TeamRole, DateTime AssignedAt);
