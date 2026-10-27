using MySqlConnector;
using RepoVault.Core.Data;
using RepoVault.Core.Models;
using RepoVault.Core.Security;

namespace RepoVault.Core.Services;

public sealed class ContributorService
{
    private readonly DbHelper _db;

    public ContributorService(DbHelper db)
    {
        _db = db;
    }

    public async Task<(bool Ok, string Message)> AssignContributorAsync(
        User actor, int repoId, int studentUserId, string teamRole = "Member",
        CancellationToken ct = default)
    {
        return await AssignContributorInternalAsync(
            actor, repoId, studentUserId, teamRole, AccessLevel.GroupCollaborator,
            TicketApprovalStatus.Approved, null, ct).ConfigureAwait(false);
    }

    public async Task<(bool Ok, string Message)> GrantPeerViewerAsync(
        User owner, int repoId, int peerStudentUserId, CancellationToken ct = default)
    {
        if (!await IsFacultyOrOwnerAsync(owner, repoId, ct).ConfigureAwait(false))
        {
            return (false, "only repository owners or faculty can grant peer viewer access.");
        }

        await using var conn = await _db.CreateOpenConnectionAsync(ct).ConfigureAwait(false);

        await using (var who = conn.CreateCommand())
        {
            who.CommandText = "SELECT Role FROM Users WHERE UserId = @u AND IsActive = 1 LIMIT 1;";
            who.Parameters.AddWithValue("@u", peerStudentUserId);
            object? role = await who.ExecuteScalarAsync(ct).ConfigureAwait(false);
            if (role is null)
            {
                return (false, "student not found.");
            }

            if (!string.Equals(role.ToString(), UserRole.Student.ToString(), StringComparison.Ordinal))
            {
                return (false, "only students can be granted peer viewer access.");
            }
        }

        await using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = "INSERT INTO RepositoryContributors (RepoId, StudentUserId, TeamRole, " +
                              "AccessLevel, ApprovalStatus, AssignedAt) " +
                              "VALUES (@r, @u, 'Peer Reviewer', 'PeerViewer', 'Approved', @now) " +
                              "ON DUPLICATE KEY UPDATE AccessLevel = 'PeerViewer', ApprovalStatus = 'Approved', TeamRole = 'Peer Reviewer';";
            cmd.Parameters.AddWithValue("@r", repoId);
            cmd.Parameters.AddWithValue("@u", peerStudentUserId);
            cmd.Parameters.AddWithValue("@now", DateTime.UtcNow);
            await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
        }

        return (true, "ok");
    }

    public async Task<(bool Ok, string Message)> GrantFacultyAccessAsync(
        User owner, int repoId, int facultyUserId, CancellationToken ct = default)
    {
        if (!await IsFacultyOrOwnerAsync(owner, repoId, ct).ConfigureAwait(false))
        {
            return (false, "only repository owners or course faculty can grant faculty access.");
        }

        await using var conn = await _db.CreateOpenConnectionAsync(ct).ConfigureAwait(false);

        await using (var who = conn.CreateCommand())
        {
            who.CommandText = "SELECT Role FROM Users WHERE UserId = @u AND IsActive = 1 LIMIT 1;";
            who.Parameters.AddWithValue("@u", facultyUserId);
            object? role = await who.ExecuteScalarAsync(ct).ConfigureAwait(false);
            if (role is null)
            {
                return (false, "faculty user not found.");
            }

            if (!string.Equals(role.ToString(), UserRole.Faculty.ToString(), StringComparison.Ordinal))
            {
                return (false, "only faculty accounts can be granted faculty reviewer access.");
            }
        }

        await using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = "INSERT INTO RepositoryContributors (RepoId, StudentUserId, TeamRole, " +
                              "AccessLevel, ApprovalStatus, AssignedAt) " +
                              "VALUES (@r, @u, 'Faculty Reviewer', 'FacultyReviewer', 'Approved', @now) " +
                              "ON DUPLICATE KEY UPDATE AccessLevel = 'FacultyReviewer', ApprovalStatus = 'Approved', TeamRole = 'Faculty Reviewer';";
            cmd.Parameters.AddWithValue("@r", repoId);
            cmd.Parameters.AddWithValue("@u", facultyUserId);
            cmd.Parameters.AddWithValue("@now", DateTime.UtcNow);
            await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
        }

        return (true, "ok");
    }

    public async Task<(bool Ok, string Message)> RequestGroupmateTicketAsync(
        User owner, int repoId, int classmateUserId, string teamRole = "Developer",
        CancellationToken ct = default)
    {
        if (!await IsFacultyOrOwnerAsync(owner, repoId, ct).ConfigureAwait(false))
        {
            return (false, "only repository owners can request groupmate tickets.");
        }

        teamRole = string.IsNullOrWhiteSpace(teamRole) ? "Developer" : teamRole.Trim();

        await using var conn = await _db.CreateOpenConnectionAsync(ct).ConfigureAwait(false);

        // check classmate user exists and is a student
        await using (var who = conn.CreateCommand())
        {
            who.CommandText = "SELECT Role FROM Users WHERE UserId = @u AND IsActive = 1 LIMIT 1;";
            who.Parameters.AddWithValue("@u", classmateUserId);
            object? role = await who.ExecuteScalarAsync(ct).ConfigureAwait(false);
            if (role is null)
            {
                return (false, "classmate student not found.");
            }

            if (!string.Equals(role.ToString(), UserRole.Student.ToString(), StringComparison.Ordinal))
            {
                return (false, "only students can be added as groupmates.");
            }
        }

        // check linked coding task group work constraints
        int? taskId = null;
        await using (var rCmd = conn.CreateCommand())
        {
            rCmd.CommandText = "SELECT TaskId FROM Repositories WHERE RepoId = @r LIMIT 1;";
            rCmd.Parameters.AddWithValue("@r", repoId);
            object? tid = await rCmd.ExecuteScalarAsync(ct).ConfigureAwait(false);
            if (tid is not null && !Convert.IsDBNull(tid))
            {
                taskId = Convert.ToInt32(tid);
            }
        }

        if (taskId.HasValue)
        {
            await using var tCmd = conn.CreateCommand();
            tCmd.CommandText = "SELECT AllowGroupWork, MaxGroupSize FROM CodingTasks WHERE TaskId = @t LIMIT 1;";
            tCmd.Parameters.AddWithValue("@t", taskId.Value);

            using var reader = await tCmd.ExecuteReaderAsync(ct).ConfigureAwait(false);
            if (await reader.ReadAsync(ct).ConfigureAwait(false))
            {
                bool allowGroup = Convert.ToBoolean(reader["AllowGroupWork"]);
                int maxSize = Convert.ToInt32(reader["MaxGroupSize"]);

                if (!allowGroup)
                {
                    return (false, "This coding task is strictly individual.");
                }

                await reader.CloseAsync().ConfigureAwait(false);

                await using var cCmd = conn.CreateCommand();
                cCmd.CommandText = "SELECT COUNT(*) FROM RepositoryContributors " +
                                  "WHERE RepoId = @r AND AccessLevel IN ('Owner','GroupCollaborator') " +
                                  "AND ApprovalStatus IN ('Approved','Pending');";
                cCmd.Parameters.AddWithValue("@r", repoId);
                int currentGroup = Convert.ToInt32(await cCmd.ExecuteScalarAsync(ct).ConfigureAwait(false));

                if (currentGroup >= maxSize)
                {
                    return (false, "group size limit reached for this task.");
                }
            }
        }

        await using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = "INSERT INTO RepositoryContributors (RepoId, StudentUserId, TeamRole, " +
                              "AccessLevel, ApprovalStatus, AssignedAt) " +
                              "VALUES (@r, @u, @t, 'GroupCollaborator', 'Pending', @now);";
            cmd.Parameters.AddWithValue("@r", repoId);
            cmd.Parameters.AddWithValue("@u", classmateUserId);
            cmd.Parameters.AddWithValue("@t", teamRole);
            cmd.Parameters.AddWithValue("@now", DateTime.UtcNow);

            try
            {
                await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
            }
            catch (MySqlException ex) when (ex.Number == 1062)
            {
                return (false, "student is already assigned to this repository.");
            }
        }

        return (true, "ok");
    }

    public async Task<(bool Ok, string Message)> ReviewGroupmateTicketAsync(
        User facultyActor, int repoId, int studentUserId, bool approve,
        CancellationToken ct = default)
    {
        if (facultyActor.Role != UserRole.Faculty || !facultyActor.IsActive)
        {
            return (false, "only faculty can review groupmate tickets.");
        }

        await using var conn = await _db.CreateOpenConnectionAsync(ct).ConfigureAwait(false);
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "UPDATE RepositoryContributors SET ApprovalStatus = @s, " +
                          "ApprovedByFacultyId = @f, ApprovedAt = @now " +
                          "WHERE RepoId = @r AND StudentUserId = @u AND AccessLevel = 'GroupCollaborator';";
        cmd.Parameters.AddWithValue("@s", approve ? "Approved" : "Rejected");
        cmd.Parameters.AddWithValue("@f", facultyActor.UserId);
        cmd.Parameters.AddWithValue("@now", DateTime.UtcNow);
        cmd.Parameters.AddWithValue("@r", repoId);
        cmd.Parameters.AddWithValue("@u", studentUserId);

        int rows = await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
        if (rows == 0)
        {
            return (false, "groupmate ticket not found.");
        }

        return (true, "ok");
    }

    public async Task<(bool Ok, string Message)> UpdateTeamRoleAsync(
        User actor, int repoId, int studentUserId, string teamRole,
        CancellationToken ct = default)
    {
        if (!await IsFacultyOrOwnerAsync(actor, repoId, ct).ConfigureAwait(false))
        {
            return (false, "only faculty or repository owners can update contributor roles.");
        }

        teamRole = teamRole.Trim();
        if (teamRole.Length == 0)
        {
            return (false, "team role is required.");
        }

        await using var conn = await _db.CreateOpenConnectionAsync(ct).ConfigureAwait(false);
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "UPDATE RepositoryContributors SET TeamRole = @t " +
                          "WHERE RepoId = @r AND StudentUserId = @u;";
        cmd.Parameters.AddWithValue("@t", teamRole);
        cmd.Parameters.AddWithValue("@r", repoId);
        cmd.Parameters.AddWithValue("@u", studentUserId);

        int rows = await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
        if (rows == 0)
        {
            return (false, "contributor assignment not found.");
        }

        return (true, "ok");
    }

    public async Task<(bool Ok, string Message)> RemoveContributorAsync(
        User actor, int repoId, int studentUserId, CancellationToken ct = default)
    {
        if (!await IsFacultyOrOwnerAsync(actor, repoId, ct).ConfigureAwait(false))
        {
            return (false, "only faculty or repository owners can remove contributors.");
        }

        await using var conn = await _db.CreateOpenConnectionAsync(ct).ConfigureAwait(false);
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM RepositoryContributors WHERE RepoId = @r AND StudentUserId = @u;";
        cmd.Parameters.AddWithValue("@r", repoId);
        cmd.Parameters.AddWithValue("@u", studentUserId);

        int rows = await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
        if (rows == 0)
        {
            return (false, "contributor assignment not found.");
        }

        return (true, "ok");
    }

    public async Task<List<RepositoryContributor>> ListContributorsByRepoAsync(
        int repoId, CancellationToken ct = default)
    {
        await using var conn = await _db.CreateOpenConnectionAsync(ct).ConfigureAwait(false);
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT RepoId, StudentUserId, TeamRole, AccessLevel, ApprovalStatus, " +
                          "ApprovedByFacultyId, ApprovedAt, AssignedAt " +
                          "FROM RepositoryContributors WHERE RepoId = @r;";
        cmd.Parameters.AddWithValue("@r", repoId);

        var rows = new List<RepositoryContributor>();
        await using var reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false);
        while (await reader.ReadAsync(ct).ConfigureAwait(false))
        {
            rows.Add(new RepositoryContributor
            {
                RepoId = Convert.ToInt32(reader["RepoId"]),
                StudentUserId = Convert.ToInt32(reader["StudentUserId"]),
                TeamRole = reader["TeamRole"]?.ToString() ?? string.Empty,
                AccessLevel = Enum.Parse<AccessLevel>(reader["AccessLevel"]?.ToString() ?? "GroupCollaborator"),
                ApprovalStatus = Enum.Parse<TicketApprovalStatus>(reader["ApprovalStatus"]?.ToString() ?? "Approved"),
                ApprovedByFacultyId = reader.IsDBNull(reader.GetOrdinal("ApprovedByFacultyId"))
                    ? null : (int?)Convert.ToInt32(reader["ApprovedByFacultyId"]),
                ApprovedAt = reader.IsDBNull(reader.GetOrdinal("ApprovedAt"))
                    ? null : (DateTime?)reader["ApprovedAt"],
                AssignedAt = (DateTime)reader["AssignedAt"],
            });
        }

        return rows;
    }

    public async Task<List<ContributorViewDto>> ListContributorViewsByRepoAsync(
        int repoId, CancellationToken ct = default)
    {
        await using var conn = await _db.CreateOpenConnectionAsync(ct).ConfigureAwait(false);
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT rc.RepoId, rc.StudentUserId, u.Username, u.FullName, u.Email, " +
                          "rc.TeamRole, rc.AssignedAt, rc.AccessLevel, rc.ApprovalStatus " +
                          "FROM RepositoryContributors rc " +
                          "JOIN Users u ON u.UserId = rc.StudentUserId " +
                          "WHERE rc.RepoId = @r ORDER BY u.Username;";
        cmd.Parameters.AddWithValue("@r", repoId);

        var rows = new List<ContributorViewDto>();
        await using var reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false);
        while (await reader.ReadAsync(ct).ConfigureAwait(false))
        {
            rows.Add(new ContributorViewDto(
                Convert.ToInt32(reader["RepoId"]),
                Convert.ToInt32(reader["StudentUserId"]),
                reader["Username"]?.ToString() ?? string.Empty,
                reader["FullName"]?.ToString() ?? string.Empty,
                reader["Email"]?.ToString() ?? string.Empty,
                reader["TeamRole"]?.ToString() ?? string.Empty,
                (DateTime)reader["AssignedAt"],
                Enum.Parse<AccessLevel>(reader["AccessLevel"]?.ToString() ?? "GroupCollaborator"),
                Enum.Parse<TicketApprovalStatus>(reader["ApprovalStatus"]?.ToString() ?? "Approved")));
        }

        return rows;
    }

    private async Task<(bool Ok, string Message)> AssignContributorInternalAsync(
        User actor, int repoId, int studentUserId, string teamRole,
        AccessLevel accessLevel, TicketApprovalStatus approvalStatus,
        int? approvedByFacultyId, CancellationToken ct)
    {
        if (!await IsFacultyOrOwnerAsync(actor, repoId, ct).ConfigureAwait(false))
        {
            return (false, "only faculty or repository owners can assign contributors.");
        }

        teamRole = teamRole.Trim();
        if (teamRole.Length == 0)
        {
            return (false, "team role is required.");
        }

        await using var conn = await _db.CreateOpenConnectionAsync(ct).ConfigureAwait(false);

        await using (var who = conn.CreateCommand())
        {
            who.CommandText = "SELECT Role FROM Users WHERE UserId = @u LIMIT 1;";
            who.Parameters.AddWithValue("@u", studentUserId);
            object? role = await who.ExecuteScalarAsync(ct).ConfigureAwait(false);
            if (role is null)
            {
                return (false, "student not found.");
            }

            if (!string.Equals(role.ToString(), UserRole.Student.ToString(), StringComparison.Ordinal) &&
                accessLevel != AccessLevel.FacultyReviewer)
            {
                return (false, "only students can be assigned as contributors.");
            }
        }

        await using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = "INSERT INTO RepositoryContributors (RepoId, StudentUserId, TeamRole, " +
                              "AccessLevel, ApprovalStatus, ApprovedByFacultyId, ApprovedAt, AssignedAt) " +
                              "VALUES (@r, @u, @t, @al, @as, @f, @fa, @now);";
            cmd.Parameters.AddWithValue("@r", repoId);
            cmd.Parameters.AddWithValue("@u", studentUserId);
            cmd.Parameters.AddWithValue("@t", teamRole);
            cmd.Parameters.AddWithValue("@al", accessLevel.ToString());
            cmd.Parameters.AddWithValue("@as", approvalStatus.ToString());
            cmd.Parameters.AddWithValue("@f", (object?)approvedByFacultyId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@fa", approvedByFacultyId.HasValue ? (object)DateTime.UtcNow : DBNull.Value);
            cmd.Parameters.AddWithValue("@now", DateTime.UtcNow);

            try
            {
                await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
            }
            catch (MySqlException ex) when (ex.Number == 1062)
            {
                return (false, "student is already assigned to this repository.");
            }
            catch (MySqlException ex) when (ex.Number == 1452)
            {
                return (false, "repository not found.");
            }
        }

        return (true, "ok");
    }

    private async Task<bool> IsFacultyOrOwnerAsync(User actor, int repoId, CancellationToken ct)
    {
        if (!actor.IsActive)
        {
            return false;
        }

        if (actor.Role == UserRole.Faculty)
        {
            return true;
        }

        await using var conn = await _db.CreateOpenConnectionAsync(ct).ConfigureAwait(false);
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT OwnerStudentId FROM Repositories WHERE RepoId = @r LIMIT 1;";
        cmd.Parameters.AddWithValue("@r", repoId);
        object? owner = await cmd.ExecuteScalarAsync(ct).ConfigureAwait(false);
        if (owner is not null && !Convert.IsDBNull(owner) && Convert.ToInt32(owner) == actor.UserId)
        {
            return true;
        }

        await using var member = conn.CreateCommand();
        member.CommandText = "SELECT 1 FROM RepositoryContributors " +
                             "WHERE RepoId = @r AND StudentUserId = @u AND AccessLevel = 'Owner' LIMIT 1;";
        member.Parameters.AddWithValue("@r", repoId);
        member.Parameters.AddWithValue("@u", actor.UserId);
        object? hit = await member.ExecuteScalarAsync(ct).ConfigureAwait(false);
        return hit is not null;
    }
}
