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
    string Email, string TeamRole, DateTime AssignedAt,
    AccessLevel AccessLevel = AccessLevel.GroupCollaborator,
    TicketApprovalStatus ApprovalStatus = TicketApprovalStatus.Approved);

public sealed record RepositoryViewDto(
    int RepoId, int CourseId, string CourseCode, string RepoSlug, string Title,
    string? Description, int? OwnerStudentId, string? OwnerUsername, int? TaskId, string? TaskTitle,
    DateTime? DeadlineUtc, string DeadlineStatus, bool IsLocked, bool IsSubmitted,
    DateTime? SubmittedAt, int PushCount, LifecycleStatus LifecycleStatus,
    string? PrimaryLanguage, int TotalFiles, int SourceLines, DateTime CreatedAt);

public sealed record CodingTaskViewDto(
    int TaskId, int CourseId, string CourseCode, string Title, string? Instructions,
    string StarterTemplate, DateTime? DeadlineUtc, string DeadlineStatus,
    bool AllowGroupWork, int MaxGroupSize, int CreatedByFacultyId,
    string CreatedByFacultyName, DateTime CreatedAt, int SubmissionsCount = 0);
