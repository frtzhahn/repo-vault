using RepoVault.Core.Data;
using RepoVault.Core.Models;

namespace RepoVault.Core.Security;

public sealed class RbacGuard
{
    private readonly DbConnectionFactory _db;

    public RbacGuard(DbConnectionFactory db)
    {
        _db = db;
    }

    public static bool CanManageCourses(User user) => user.IsActive && user.Role == UserRole.Faculty;

    public static bool CanGradeRepository(User user) => user.IsActive && user.Role == UserRole.Faculty;

    public static bool CanModerateComment(User user) => user.IsActive && user.Role == UserRole.Faculty;

    public static bool CanManageRepoBin(User user) => user.IsActive && user.Role == UserRole.Faculty;

    public static bool CanEditOrDeleteComment(User user, int commentAuthorUserId) =>
        user.IsActive && (user.Role == UserRole.Faculty || user.UserId == commentAuthorUserId);

    public async Task<bool> CanPushToRepositoryAsync(User user, int repoId, CancellationToken ct = default)
    {
        if (!user.IsActive)
        {
            return false;
        }

        await using var conn = await _db.CreateOpenConnectionAsync(ct).ConfigureAwait(false);

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT IsLocked, LifecycleStatus, DeadlineUtc FROM Repositories WHERE RepoId = @id LIMIT 1;";
        cmd.Parameters.AddWithValue("@id", repoId);

        await using var reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false);
        if (!await reader.ReadAsync(ct).ConfigureAwait(false))
        {
            return false;
        }

        bool locked = Convert.ToBoolean(reader["IsLocked"]);
        var status = Enum.Parse<LifecycleStatus>(reader["LifecycleStatus"]?.ToString() ?? "Trashed");
        DateTime? deadline = reader.IsDBNull(reader.GetOrdinal("DeadlineUtc"))
            ? null
            : (DateTime?)reader["DeadlineUtc"];

        if (status != LifecycleStatus.Active || locked)
        {
            return false;
        }

        if (deadline.HasValue && deadline.Value < DateTime.UtcNow)
        {
            return false;
        }

        if (user.Role == UserRole.Faculty)
        {
            return true;
        }

        await reader.CloseAsync().ConfigureAwait(false);

        await using var member = conn.CreateCommand();
        member.CommandText = "SELECT 1 FROM RepositoryContributors WHERE RepoId = @r AND StudentUserId = @u LIMIT 1;";
        member.Parameters.AddWithValue("@r", repoId);
        member.Parameters.AddWithValue("@u", user.UserId);

        object? hit = await member.ExecuteScalarAsync(ct).ConfigureAwait(false);
        return hit is not null;
    }
}
