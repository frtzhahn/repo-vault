using RepoVault.Core.Data;
using RepoVault.Core.Models;

namespace RepoVault.Core.Security;

public sealed class RbacGuard
{
    private readonly DbHelper _db;

    public RbacGuard(DbHelper db)
    {
        _db = db;
    }

    public static bool CanManageCourses(User user) => user.IsActive && user.Role == UserRole.Faculty;

    public static bool CanGradeRepository(User user) => user.IsActive && user.Role == UserRole.Faculty;

    public static bool CanModerateComment(User user) => user.IsActive && user.Role == UserRole.Faculty;

    public static bool CanManageRepoBin(User user) => user.IsActive && user.Role == UserRole.Faculty;

    public static bool CanEditOrDeleteComment(User user, int commentAuthorUserId) =>
        user.IsActive && (user.Role == UserRole.Faculty || user.UserId == commentAuthorUserId);

    public async Task<bool> CanViewRepositoryMetadataAsync(User user, int repoId, CancellationToken ct = default)
    {
        if (!user.IsActive)
        {
            return false;
        }

        await using var conn = await _db.CreateOpenConnectionAsync(ct).ConfigureAwait(false);

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT r.OwnerStudentId, c.CreatedByFacultyId " +
                          "FROM Repositories r JOIN Courses c ON c.CourseId = r.CourseId " +
                          "WHERE r.RepoId = @id LIMIT 1;";
        cmd.Parameters.AddWithValue("@id", repoId);

        int? ownerStudentId = null;
        int courseFacultyId = 0;
        await using (var reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false))
        {
            if (!await reader.ReadAsync(ct).ConfigureAwait(false))
            {
                return false;
            }

            if (!reader.IsDBNull(reader.GetOrdinal("OwnerStudentId")))
            {
                ownerStudentId = Convert.ToInt32(reader["OwnerStudentId"]);
            }

            courseFacultyId = Convert.ToInt32(reader["CreatedByFacultyId"]);
        }

        // repo owner and course instructor have metadata access
        if (ownerStudentId.HasValue && ownerStudentId.Value == user.UserId)
        {
            return true;
        }

        if (user.Role == UserRole.Faculty && courseFacultyId == user.UserId)
        {
            return true;
        }

        // check granted membership or reviewer roles
        await using var member = conn.CreateCommand();
        member.CommandText = "SELECT AccessLevel, ApprovalStatus FROM RepositoryContributors " +
                             "WHERE RepoId = @r AND StudentUserId = @u LIMIT 1;";
        member.Parameters.AddWithValue("@r", repoId);
        member.Parameters.AddWithValue("@u", user.UserId);

        await using (var mReader = await member.ExecuteReaderAsync(ct).ConfigureAwait(false))
        {
            if (await mReader.ReadAsync(ct).ConfigureAwait(false))
            {
                var access = Enum.Parse<AccessLevel>(mReader["AccessLevel"]?.ToString() ?? "GroupCollaborator");
                var status = Enum.Parse<TicketApprovalStatus>(mReader["ApprovalStatus"]?.ToString() ?? "Approved");

                if (access == AccessLevel.Owner)
                {
                    return true;
                }

                if (status == TicketApprovalStatus.Approved)
                {
                    return true;
                }
            }
        }

        return false;
    }

    public async Task<bool> CanReadSourceCodeAsync(User user, int repoId, CancellationToken ct = default)
    {
        if (!user.IsActive)
        {
            return false;
        }

        await using var conn = await _db.CreateOpenConnectionAsync(ct).ConfigureAwait(false);

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT r.OwnerStudentId, c.CreatedByFacultyId " +
                          "FROM Repositories r JOIN Courses c ON c.CourseId = r.CourseId " +
                          "WHERE r.RepoId = @id LIMIT 1;";
        cmd.Parameters.AddWithValue("@id", repoId);

        int? ownerStudentId = null;
        int courseFacultyId = 0;
        await using (var reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false))
        {
            if (!await reader.ReadAsync(ct).ConfigureAwait(false))
            {
                return false;
            }

            if (!reader.IsDBNull(reader.GetOrdinal("OwnerStudentId")))
            {
                ownerStudentId = Convert.ToInt32(reader["OwnerStudentId"]);
            }

            courseFacultyId = Convert.ToInt32(reader["CreatedByFacultyId"]);
        }

        if (ownerStudentId.HasValue && ownerStudentId.Value == user.UserId)
        {
            return true;
        }

        if (user.Role == UserRole.Faculty && courseFacultyId == user.UserId)
        {
            return true;
        }

        await using var member = conn.CreateCommand();
        member.CommandText = "SELECT AccessLevel, ApprovalStatus FROM RepositoryContributors " +
                             "WHERE RepoId = @r AND StudentUserId = @u LIMIT 1;";
        member.Parameters.AddWithValue("@r", repoId);
        member.Parameters.AddWithValue("@u", user.UserId);

        await using (var mReader = await member.ExecuteReaderAsync(ct).ConfigureAwait(false))
        {
            if (await mReader.ReadAsync(ct).ConfigureAwait(false))
            {
                var access = Enum.Parse<AccessLevel>(mReader["AccessLevel"]?.ToString() ?? "GroupCollaborator");
                var status = Enum.Parse<TicketApprovalStatus>(mReader["ApprovalStatus"]?.ToString() ?? "Approved");

                if (access == AccessLevel.Owner)
                {
                    return true;
                }

                if (status == TicketApprovalStatus.Approved &&
                    (access == AccessLevel.GroupCollaborator || access == AccessLevel.FacultyReviewer))
                {
                    return true;
                }
            }
        }

        return false;
    }

    public async Task<bool> CanCloneRepositoryAsync(User user, int repoId, CancellationToken ct = default)
    {
        return await CanReadSourceCodeAsync(user, repoId, ct).ConfigureAwait(false);
    }

    public async Task<bool> CanPushToRepositoryAsync(User user, int repoId, CancellationToken ct = default)
    {
        // faculty accounts never push to student repos to preserve chain of custody
        if (!user.IsActive || user.Role != UserRole.Student)
        {
            return false;
        }

        await using var conn = await _db.CreateOpenConnectionAsync(ct).ConfigureAwait(false);

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT OwnerStudentId, IsLocked, LifecycleStatus, DeadlineUtc " +
                          "FROM Repositories WHERE RepoId = @id LIMIT 1;";
        cmd.Parameters.AddWithValue("@id", repoId);

        int? ownerStudentId = null;
        bool locked;
        LifecycleStatus status;
        DateTime? deadline;

        await using (var reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false))
        {
            if (!await reader.ReadAsync(ct).ConfigureAwait(false))
            {
                return false;
            }

            if (!reader.IsDBNull(reader.GetOrdinal("OwnerStudentId")))
            {
                ownerStudentId = Convert.ToInt32(reader["OwnerStudentId"]);
            }

            locked = Convert.ToBoolean(reader["IsLocked"]);
            status = Enum.Parse<LifecycleStatus>(reader["LifecycleStatus"]?.ToString() ?? "Trashed");
            deadline = reader.IsDBNull(reader.GetOrdinal("DeadlineUtc"))
                ? null
                : (DateTime?)reader["DeadlineUtc"];
        }

        if (status != LifecycleStatus.Active || locked)
        {
            return false;
        }

        if (deadline.HasValue && deadline.Value < DateTime.UtcNow)
        {
            return false;
        }

        if (ownerStudentId.HasValue && ownerStudentId.Value == user.UserId)
        {
            return true;
        }

        await using var member = conn.CreateCommand();
        member.CommandText = "SELECT AccessLevel, ApprovalStatus FROM RepositoryContributors " +
                             "WHERE RepoId = @r AND StudentUserId = @u LIMIT 1;";
        member.Parameters.AddWithValue("@r", repoId);
        member.Parameters.AddWithValue("@u", user.UserId);

        await using (var mReader = await member.ExecuteReaderAsync(ct).ConfigureAwait(false))
        {
            if (await mReader.ReadAsync(ct).ConfigureAwait(false))
            {
                var access = Enum.Parse<AccessLevel>(mReader["AccessLevel"]?.ToString() ?? "GroupCollaborator");
                var approval = Enum.Parse<TicketApprovalStatus>(mReader["ApprovalStatus"]?.ToString() ?? "Approved");

                if (access == AccessLevel.Owner)
                {
                    return true;
                }

                if (access == AccessLevel.GroupCollaborator && approval == TicketApprovalStatus.Approved)
                {
                    return true;
                }
            }
        }

        return false;
    }
}
