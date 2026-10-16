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
        if (!RbacGuard.CanManageCourses(actor))
        {
            return (false, "only faculty can assign contributors.");
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

            if (!string.Equals(role.ToString(), UserRole.Student.ToString(), StringComparison.Ordinal))
            {
                return (false, "only students can be assigned as contributors.");
            }
        }

        await using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = "INSERT INTO RepositoryContributors (RepoId, StudentUserId, TeamRole, AssignedAt) " +
                              "VALUES (@r, @u, @t, @now);";
            cmd.Parameters.AddWithValue("@r", repoId);
            cmd.Parameters.AddWithValue("@u", studentUserId);
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
            catch (MySqlException ex) when (ex.Number == 1452)
            {
                return (false, "repository not found.");
            }
        }

        return (true, "ok");
    }

    public async Task<(bool Ok, string Message)> UpdateTeamRoleAsync(
        User actor, int repoId, int studentUserId, string teamRole,
        CancellationToken ct = default)
    {
        if (!RbacGuard.CanManageCourses(actor))
        {
            return (false, "only faculty can assign contributors.");
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
        if (!RbacGuard.CanManageCourses(actor))
        {
            return (false, "only faculty can assign contributors.");
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
        cmd.CommandText = "SELECT RepoId, StudentUserId, TeamRole, AssignedAt " +
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
                          "rc.TeamRole, rc.AssignedAt FROM RepositoryContributors rc " +
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
                (DateTime)reader["AssignedAt"]));
        }

        return rows;
    }
}
