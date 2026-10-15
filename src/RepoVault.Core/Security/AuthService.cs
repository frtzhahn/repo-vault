using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using MySqlConnector;
using RepoVault.Core.Data;
using RepoVault.Core.Models;

namespace RepoVault.Core.Security;

public sealed class AuthService
{
    public const int MinPasswordLength = 8;

    // git sends the same basic header 2-3 times per push/clone, so cache brief auth wins
    private static readonly TimeSpan BasicCacheTtl = TimeSpan.FromSeconds(30);

    private readonly DbConnectionFactory _db;
    private readonly ConcurrentDictionary<string, (User User, DateTime ExpiresAt)> _basicCache = new();

    public AuthService(DbConnectionFactory db)
    {
        _db = db;
    }

    public async Task<(bool Ok, string Message, User? User)> RegisterUserAsync(
        string idNumber, string username, string password, string fullName,
        string email, UserRole role, CancellationToken ct = default)
    {
        idNumber = idNumber.Trim();
        username = username.Trim();
        fullName = fullName.Trim();
        email = email.Trim();

        if (idNumber.Length == 0)
        {
            return (false, "id number is required.", null);
        }

        if (username.Length == 0)
        {
            return (false, "username is required.", null);
        }

        if (password.Length < MinPasswordLength)
        {
            return (false, $"password must be at least {MinPasswordLength} characters.", null);
        }

        if (fullName.Length == 0)
        {
            return (false, "full name is required.", null);
        }

        if (email.Length == 0 || !email.Contains('@'))
        {
            return (false, "a valid email is required.", null);
        }

        if (!Enum.IsDefined(role))
        {
            return (false, "role must be Student or Faculty.", null);
        }

        await using var conn = await _db.CreateOpenConnectionAsync(ct).ConfigureAwait(false);

        await using (var dup = conn.CreateCommand())
        {
            dup.CommandText = "SELECT IdNumber, Username, Email FROM Users " +
                              "WHERE IdNumber = @id OR Username = @u OR Email = @e LIMIT 1;";
            dup.Parameters.AddWithValue("@id", idNumber);
            dup.Parameters.AddWithValue("@u", username);
            dup.Parameters.AddWithValue("@e", email);

            await using var reader = await dup.ExecuteReaderAsync(ct).ConfigureAwait(false);
            if (await reader.ReadAsync(ct).ConfigureAwait(false))
            {
                string clash = string.Equals(reader["IdNumber"]?.ToString(), idNumber, StringComparison.Ordinal)
                    ? "id number"
                    : string.Equals(reader["Username"]?.ToString(), username, StringComparison.Ordinal)
                        ? "username"
                        : "email";
                return (false, $"{clash} already exists.", null);
            }
        }

        var (hash, salt, iterations) = PasswordHasher.HashPassword(password);

        try
        {
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = "INSERT INTO Users (IdNumber, Username, PasswordHash, PasswordSalt, " +
                              "Iterations, FullName, Email, Role) " +
                              "VALUES (@id, @u, @h, @s, @i, @n, @e, @r); " +
                              "SELECT LAST_INSERT_ID();";
            cmd.Parameters.AddWithValue("@id", idNumber);
            cmd.Parameters.AddWithValue("@u", username);
            cmd.Parameters.AddWithValue("@h", hash);
            cmd.Parameters.AddWithValue("@s", salt);
            cmd.Parameters.AddWithValue("@i", iterations);
            cmd.Parameters.AddWithValue("@n", fullName);
            cmd.Parameters.AddWithValue("@e", email);
            cmd.Parameters.AddWithValue("@r", role.ToString());

            object? id = await cmd.ExecuteScalarAsync(ct).ConfigureAwait(false);
            var created = new User
            {
                UserId = Convert.ToInt32(id),
                IdNumber = idNumber,
                Username = username,
                PasswordHash = hash,
                PasswordSalt = salt,
                Iterations = iterations,
                FullName = fullName,
                Email = email,
                Role = role,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
            };
            return (true, "ok", created);
        }
        catch (MySqlException ex) when (ex.Number == 1062)
        {
            // lost a race with another registration, report it cleanly
            return (false, "id number, username, or email already exists.", null);
        }
    }

    public async Task<(bool Ok, string Message, User? User)> AuthenticateAsync(
        string username, string password, CancellationToken ct = default)
    {
        username = username.Trim();
        if (username.Length == 0 || password.Length == 0)
        {
            return (false, "username and password are required.", null);
        }

        await using var conn = await _db.CreateOpenConnectionAsync(ct).ConfigureAwait(false);
        User? found = await FindByUsernameAsync(conn, username, ct).ConfigureAwait(false);
        if (found is null)
        {
            return (false, "invalid username or password.", null);
        }

        if (!found.IsActive)
        {
            return (false, "account is deactivated.", null);
        }

        bool match = PasswordHasher.VerifyPassword(
            password, found.PasswordHash, found.PasswordSalt, found.Iterations);
        if (!match)
        {
            return (false, "invalid username or password.", null);
        }

        return (true, "ok", found);
    }

    public async Task<(bool Ok, string Message, User? User)> AuthenticateBasicHeaderAsync(
        string? authorizationHeader, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(authorizationHeader) ||
            !authorizationHeader.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
        {
            return (false, "missing or invalid authorization header.", null);
        }

        string encoded = authorizationHeader.Substring("Basic ".Length).Trim();
        string decoded;
        try
        {
            decoded = Encoding.UTF8.GetString(Convert.FromBase64String(encoded));
        }
        catch (FormatException)
        {
            return (false, "malformed authorization header.", null);
        }

        // split on the first colon only, passwords may contain colons
        int sep = decoded.IndexOf(':');
        if (sep <= 0)
        {
            return (false, "malformed authorization header.", null);
        }

        string username = decoded.Substring(0, sep);
        string password = decoded.Substring(sep + 1);

        string key = username + ":" + Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(password)));
        if (_basicCache.TryGetValue(key, out var cached))
        {
            if (cached.ExpiresAt > DateTime.UtcNow)
            {
                return (true, "ok", cached.User);
            }

            _basicCache.TryRemove(key, out _);
        }

        var result = await AuthenticateAsync(username, password, ct).ConfigureAwait(false);
        if (result.Ok && result.User is not null)
        {
            _basicCache[key] = (result.User, DateTime.UtcNow.Add(BasicCacheTtl));
        }

        return result;
    }

    internal static async Task<User?> FindByUsernameAsync(
        MySqlConnection conn, string username, CancellationToken ct)
    {
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT UserId, IdNumber, Username, PasswordHash, PasswordSalt, Iterations, " +
                          "FullName, Email, Role, IsActive, CreatedAt FROM Users WHERE Username = @u LIMIT 1;";
        cmd.Parameters.AddWithValue("@u", username);

        await using var reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false);
        if (!await reader.ReadAsync(ct).ConfigureAwait(false))
        {
            return null;
        }

        return MapUser(reader);
    }

    internal static User MapUser(MySqlDataReader reader)
    {
        return new User
        {
            UserId = Convert.ToInt32(reader["UserId"]),
            IdNumber = reader["IdNumber"]?.ToString() ?? string.Empty,
            Username = reader["Username"]?.ToString() ?? string.Empty,
            PasswordHash = reader["PasswordHash"]?.ToString() ?? string.Empty,
            PasswordSalt = reader["PasswordSalt"]?.ToString() ?? string.Empty,
            Iterations = Convert.ToInt32(reader["Iterations"]),
            FullName = reader["FullName"]?.ToString() ?? string.Empty,
            Email = reader["Email"]?.ToString() ?? string.Empty,
            Role = Enum.Parse<UserRole>(reader["Role"]?.ToString() ?? "Student"),
            IsActive = Convert.ToBoolean(reader["IsActive"]),
            CreatedAt = (DateTime)reader["CreatedAt"],
        };
    }
}
