using MySqlConnector;
using RepoVault.Core.Data;
using RepoVault.Core.Models;
using RepoVault.Core.Security;

namespace RepoVault.Core.Services;

public sealed class UserService
{
    private readonly DbHelper _db;

    public UserService(DbHelper db)
    {
        _db = db;
    }

    public async Task<User?> GetByIdAsync(int userId, CancellationToken ct = default)
    {
        await using var conn = await _db.CreateOpenConnectionAsync(ct).ConfigureAwait(false);
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT UserId, IdNumber, Username, PasswordHash, PasswordSalt, Iterations, " +
                          "FullName, Email, Role, IsActive, CreatedAt FROM Users WHERE UserId = @id LIMIT 1;";
        cmd.Parameters.AddWithValue("@id", userId);

        await using var reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false);
        if (!await reader.ReadAsync(ct).ConfigureAwait(false))
        {
            return null;
        }

        return AuthService.MapUser(reader);
    }

    public async Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default)
    {
        await using var conn = await _db.CreateOpenConnectionAsync(ct).ConfigureAwait(false);
        return await AuthService.FindByUsernameAsync(conn, username.Trim(), ct).ConfigureAwait(false);
    }

    public async Task<List<User>> ListUsersAsync(
        UserRole? roleFilter = null, bool includeInactive = true, CancellationToken ct = default)
    {
        await using var conn = await _db.CreateOpenConnectionAsync(ct).ConfigureAwait(false);
        await using var cmd = conn.CreateCommand();

        string sql = "SELECT UserId, IdNumber, Username, PasswordHash, PasswordSalt, Iterations, " +
                     "FullName, Email, Role, IsActive, CreatedAt FROM Users WHERE 1 = 1";
        if (roleFilter.HasValue)
        {
            sql += " AND Role = @r";
            cmd.Parameters.AddWithValue("@r", roleFilter.Value.ToString());
        }

        if (!includeInactive)
        {
            sql += " AND IsActive = 1";
        }

        cmd.CommandText = sql + ";";

        var users = new List<User>();
        await using var reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false);
        while (await reader.ReadAsync(ct).ConfigureAwait(false))
        {
            users.Add(AuthService.MapUser(reader));
        }

        return users;
    }

    public async Task<List<UserSummaryDto>> ListUserSummariesAsync(
        UserRole? roleFilter = null, bool includeInactive = true, CancellationToken ct = default)
    {
        await using var conn = await _db.CreateOpenConnectionAsync(ct).ConfigureAwait(false);
        await using var cmd = conn.CreateCommand();

        string sql = "SELECT UserId, IdNumber, Username, FullName, Email, Role, IsActive, CreatedAt " +
                     "FROM Users WHERE 1 = 1";
        if (roleFilter.HasValue)
        {
            sql += " AND Role = @r";
            cmd.Parameters.AddWithValue("@r", roleFilter.Value.ToString());
        }

        if (!includeInactive)
        {
            sql += " AND IsActive = 1";
        }

        cmd.CommandText = sql + " ORDER BY Username;";

        var rows = new List<UserSummaryDto>();
        await using var reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false);
        while (await reader.ReadAsync(ct).ConfigureAwait(false))
        {
            rows.Add(new UserSummaryDto(
                Convert.ToInt32(reader["UserId"]),
                reader["IdNumber"]?.ToString() ?? string.Empty,
                reader["Username"]?.ToString() ?? string.Empty,
                reader["FullName"]?.ToString() ?? string.Empty,
                reader["Email"]?.ToString() ?? string.Empty,
                Enum.Parse<UserRole>(reader["Role"]?.ToString() ?? "Student"),
                Convert.ToBoolean(reader["IsActive"]),
                (DateTime)reader["CreatedAt"]));
        }

        return rows;
    }

    public async Task<(bool Ok, string Message, User? User)> UpdateProfileAsync(
        User actor, int targetUserId, string fullName, string email,
        UserRole role, bool isActive, CancellationToken ct = default)
    {
        fullName = fullName.Trim();
        email = email.Trim();

        if (fullName.Length == 0)
        {
            return (false, "full name is required.", null);
        }

        if (email.Length == 0 || !email.Contains('@'))
        {
            return (false, "a valid email is required.", null);
        }

        // students may only touch their own name and email
        if (actor.Role != UserRole.Faculty)
        {
            if (actor.UserId != targetUserId)
            {
                return (false, "students can only update their own profile.", null);
            }

            if (role != actor.Role || isActive != actor.IsActive)
            {
                return (false, "only faculty can change roles or active status.", null);
            }
        }

        await using var conn = await _db.CreateOpenConnectionAsync(ct).ConfigureAwait(false);

        await using (var dup = conn.CreateCommand())
        {
            dup.CommandText = "SELECT UserId FROM Users WHERE Email = @e AND UserId <> @id LIMIT 1;";
            dup.Parameters.AddWithValue("@e", email);
            dup.Parameters.AddWithValue("@id", targetUserId);
            object? clash = await dup.ExecuteScalarAsync(ct).ConfigureAwait(false);
            if (clash is not null)
            {
                return (false, "email already exists.", null);
            }
        }

        await using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = "UPDATE Users SET FullName = @n, Email = @e, Role = @r, IsActive = @a " +
                              "WHERE UserId = @id;";
            cmd.Parameters.AddWithValue("@n", fullName);
            cmd.Parameters.AddWithValue("@e", email);
            cmd.Parameters.AddWithValue("@r", role.ToString());
            cmd.Parameters.AddWithValue("@a", isActive);
            cmd.Parameters.AddWithValue("@id", targetUserId);

            int rows = await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
            if (rows == 0)
            {
                return (false, "user not found.", null);
            }
        }

        User? updated = await GetByIdAsync(targetUserId, ct).ConfigureAwait(false);
        return (true, "ok", updated);
    }

    public async Task<(bool Ok, string Message)> ChangePasswordAsync(
        User actor, int targetUserId, string newPassword, CancellationToken ct = default)
    {
        if (newPassword.Length < AuthService.MinPasswordLength)
        {
            return (false, $"password must be at least {AuthService.MinPasswordLength} characters.");
        }

        // users change their own password, faculty may reset anyone
        if (actor.Role != UserRole.Faculty && actor.UserId != targetUserId)
        {
            return (false, "you can only change your own password.");
        }

        var (hash, salt, iterations) = PasswordHasher.HashPassword(newPassword);

        await using var conn = await _db.CreateOpenConnectionAsync(ct).ConfigureAwait(false);
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "UPDATE Users SET PasswordHash = @h, PasswordSalt = @s, Iterations = @i " +
                          "WHERE UserId = @id;";
        cmd.Parameters.AddWithValue("@h", hash);
        cmd.Parameters.AddWithValue("@s", salt);
        cmd.Parameters.AddWithValue("@i", iterations);
        cmd.Parameters.AddWithValue("@id", targetUserId);

        int rows = await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
        if (rows == 0)
        {
            return (false, "user not found.");
        }

        return (true, "ok");
    }

    public async Task<(bool Ok, string Message)> DeleteOrDeactivateUserAsync(
        User actor, int targetUserId, CancellationToken ct = default)
    {
        if (!actor.IsActive || actor.Role != UserRole.Faculty)
        {
            return (false, "only faculty can delete users.");
        }

        await using var conn = await _db.CreateOpenConnectionAsync(ct).ConfigureAwait(false);
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM Users WHERE UserId = @id;";
        cmd.Parameters.AddWithValue("@id", targetUserId);

        try
        {
            int rows = await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
            if (rows == 0)
            {
                return (false, "user not found.");
            }

            return (true, "ok");
        }
        catch (MySqlException ex) when (ex.Number == 1451)
        {
            // referenced by courses or push ledger, deactivate instead of deleting
            await using var off = conn.CreateCommand();
            off.CommandText = "UPDATE Users SET IsActive = 0 WHERE UserId = @id;";
            off.Parameters.AddWithValue("@id", targetUserId);
            await off.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
            return (true, "user is referenced by other records, deactivated instead.");
        }
    }
}
