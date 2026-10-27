namespace RepoVault.Core.Models;

public sealed class RepositoryContributor
{
    public int RepoId { get; set; }
    public int StudentUserId { get; set; }
    public string TeamRole { get; set; } = string.Empty;
    public AccessLevel AccessLevel { get; set; } = AccessLevel.GroupCollaborator;
    public TicketApprovalStatus ApprovalStatus { get; set; } = TicketApprovalStatus.Approved;
    public int? ApprovedByFacultyId { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public DateTime AssignedAt { get; set; }
}

public enum AccessLevel
{
    Owner,
    GroupCollaborator,
    PeerViewer,
    FacultyReviewer
}

public enum TicketApprovalStatus
{
    Pending,
    Approved,
    Rejected
}
