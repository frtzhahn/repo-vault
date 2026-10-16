using System.Text;
using MySqlConnector;
using RepoVault.Core.Data;
using RepoVault.Core.Models;
using RepoVault.Core.Security;

namespace RepoVault.Core.Services;

public sealed class RepositoryCatalogService
{
    private readonly DbConnectionFactory _db;
    private readonly string _repoRoot;

    public RepositoryCatalogService(DbConnectionFactory db, string? repoRoot = null)
    {
        _db = db;
        _repoRoot = repoRoot ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "RepoVault", "repos");
    }

    public string RepoRoot => _repoRoot;

    public static string NormalizeSlug(string raw)
    {
        var sb = new StringBuilder(raw.Length);
        bool lastDash = true;
        foreach (char c in raw.ToLowerInvariant())
        {
            if ((c >= 'a' && c <= 'z') || (c >= '0' && c <= '9') || c == '_' )
            {
                sb.Append(c);
                lastDash = false;
            }
            else if (c == '-' && !lastDash)
            {
                sb.Append(c);
                lastDash = true;
            }
            else if (!lastDash)
            {
                sb.Append('-');
                lastDash = true;
            }
        }

        return sb.ToString().Trim('-');
    }

    public async Task<(bool Ok, string Message, RepositoryRecord? Repo)> CreateRepositoryAsync(
        User actor, int courseId, string repoSlug, string title,
        string? description, DateTime? deadlineUtc, CancellationToken ct = default)
    {
        if (!actor.IsActive)
        {
            return (false, "deactivated accounts cannot create repositories.", null);
        }

        string slug = NormalizeSlug(repoSlug);
        if (slug.Length == 0)
        {
            return (false, "repository slug must contain letters or numbers.", null);
        }

        title = title.Trim();
        if (title.Length == 0)
        {
            return (false, "repository title is required.", null);
        }

        await using var conn = await _db.CreateOpenConnectionAsync(ct).ConfigureAwait(false);

        int repoId;
        await using (var cmd = conn.CreateCommand())
        {
            // insert with a throwaway path, the real path needs the new id
            cmd.CommandText = "INSERT INTO Repositories (CourseId, RepoSlug, Title, Description, " +
                              "BareRepoDiskPath, DeadlineUtc, CreatedAt) " +
                              "VALUES (@c, @s, @t, @d, @p, @dl, @now);";
            cmd.Parameters.AddWithValue("@c", courseId);
            cmd.Parameters.AddWithValue("@s", slug);
            cmd.Parameters.AddWithValue("@t", title);
            cmd.Parameters.AddWithValue("@d", (object?)description?.Trim() ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@p", "pending/" + Guid.NewGuid().ToString("N"));
            cmd.Parameters.AddWithValue("@dl", (object?)deadlineUtc ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@now", DateTime.UtcNow);

            try
            {
                await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
            }
            catch (MySqlException ex) when (ex.Number == 1062)
            {
                return (false, "a repository with this slug already exists in the course.", null);
            }
            catch (MySqlException ex) when (ex.Number == 1452)
            {
                return (false, "course not found.", null);
            }

            repoId = (int)cmd.LastInsertedId;
        }

        string diskPath = Path.Combine(_repoRoot, repoId.ToString());
        await using (var path = conn.CreateCommand())
        {
            path.CommandText = "UPDATE Repositories SET BareRepoDiskPath = @p WHERE RepoId = @id;";
            path.Parameters.AddWithValue("@p", diskPath);
            path.Parameters.AddWithValue("@id", repoId);
            await path.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
        }

        RepositoryRecord? created = await GetByIdAsync(repoId, ct).ConfigureAwait(false);
        return (true, "ok", created);
    }

    public async Task<RepositoryRecord?> GetByIdAsync(int repoId, CancellationToken ct = default)
    {
        await using var conn = await _db.CreateOpenConnectionAsync(ct).ConfigureAwait(false);
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = SelectAll + " WHERE RepoId = @id LIMIT 1;";
        cmd.Parameters.AddWithValue("@id", repoId);

        await using var reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false);
        if (!await reader.ReadAsync(ct).ConfigureAwait(false))
        {
            return null;
        }

        return MapRepo(reader);
    }

    public async Task<RepositoryRecord?> GetBySlugAsync(
        int courseId, string repoSlug, CancellationToken ct = default)
    {
        await using var conn = await _db.CreateOpenConnectionAsync(ct).ConfigureAwait(false);
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = SelectAll + " WHERE CourseId = @c AND RepoSlug = @s LIMIT 1;";
        cmd.Parameters.AddWithValue("@c", courseId);
        cmd.Parameters.AddWithValue("@s", NormalizeSlug(repoSlug));

        await using var reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false);
        if (!await reader.ReadAsync(ct).ConfigureAwait(false))
        {
            return null;
        }

        return MapRepo(reader);
    }

    // the git http route carries only the slug, so look it up without a course id
    public async Task<RepositoryRecord?> GetBySlugGlobalAsync(
        string repoSlug, CancellationToken ct = default)
    {
        await using var conn = await _db.CreateOpenConnectionAsync(ct).ConfigureAwait(false);
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = SelectAll + " WHERE RepoSlug = @s LIMIT 1;";
        cmd.Parameters.AddWithValue("@s", NormalizeSlug(repoSlug));

        await using var reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false);
        if (!await reader.ReadAsync(ct).ConfigureAwait(false))
        {
            return null;
        }

        return MapRepo(reader);
    }

    public async Task<List<RepositoryRecord>> ListActiveByCourseAsync(
        int courseId, CancellationToken ct = default)
    {
        await using var conn = await _db.CreateOpenConnectionAsync(ct).ConfigureAwait(false);
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = SelectAll + " WHERE CourseId = @c AND LifecycleStatus = 'Active';";
        cmd.Parameters.AddWithValue("@c", courseId);

        var repos = new List<RepositoryRecord>();
        await using var reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false);
        while (await reader.ReadAsync(ct).ConfigureAwait(false))
        {
            repos.Add(MapRepo(reader));
        }

        return repos;
    }

    public async Task<List<RepositoryRecord>> ListByStudentContributorAsync(
        int studentUserId, CancellationToken ct = default)
    {
        await using var conn = await _db.CreateOpenConnectionAsync(ct).ConfigureAwait(false);
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = SelectAll + " JOIN RepositoryContributors rc USING (RepoId) " +
                          "WHERE rc.StudentUserId = @u AND LifecycleStatus = 'Active';";
        cmd.Parameters.AddWithValue("@u", studentUserId);

        var repos = new List<RepositoryRecord>();
        await using var reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false);
        while (await reader.ReadAsync(ct).ConfigureAwait(false))
        {
            repos.Add(MapRepo(reader));
        }

        return repos;
    }

    public async Task<(bool Ok, string Message, List<RepositoryRecord> Repos)> ListTrashedInRepoBinAsync(
        User actor, CancellationToken ct = default)
    {
        if (!RbacGuard.CanManageRepoBin(actor))
        {
            return (false, "only faculty can browse the repo bin.", new List<RepositoryRecord>());
        }

        await using var conn = await _db.CreateOpenConnectionAsync(ct).ConfigureAwait(false);
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = SelectAll + " WHERE LifecycleStatus = 'Trashed';";

        var repos = new List<RepositoryRecord>();
        await using var reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false);
        while (await reader.ReadAsync(ct).ConfigureAwait(false))
        {
            repos.Add(MapRepo(reader));
        }

        return (true, "ok", repos);
    }

    public async Task<(bool Ok, string Message)> UpdateRepositoryAsync(
        User actor, int repoId, string title, string? description,
        DateTime? deadlineUtc, bool isLocked, CancellationToken ct = default)
    {
        if (!actor.IsActive)
        {
            return (false, "deactivated accounts cannot update repositories.");
        }

        title = title.Trim();
        if (title.Length == 0)
        {
            return (false, "repository title is required.");
        }

        RepositoryRecord? current = await GetByIdAsync(repoId, ct).ConfigureAwait(false);
        if (current is null)
        {
            return (false, "repository not found.");
        }

        // only faculty may flip the lock through a metadata update
        if (isLocked != current.IsLocked && !RbacGuard.CanGradeRepository(actor))
        {
            return (false, "only faculty can lock repositories.");
        }

        await using var conn = await _db.CreateOpenConnectionAsync(ct).ConfigureAwait(false);
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "UPDATE Repositories SET Title = @t, Description = @d, " +
                          "DeadlineUtc = @dl, IsLocked = @l WHERE RepoId = @id;";
        cmd.Parameters.AddWithValue("@t", title);
        cmd.Parameters.AddWithValue("@d", (object?)description?.Trim() ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@dl", (object?)deadlineUtc ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@l", isLocked);
        cmd.Parameters.AddWithValue("@id", repoId);
        await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);

        return (true, "ok");
    }

    public async Task<(bool Ok, string Message)> SetLockStateAsync(
        User actor, int repoId, bool isLocked, CancellationToken ct = default)
    {
        if (!RbacGuard.CanGradeRepository(actor))
        {
            return (false, "only faculty can lock repositories.");
        }

        await using var conn = await _db.CreateOpenConnectionAsync(ct).ConfigureAwait(false);
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "UPDATE Repositories SET IsLocked = @l WHERE RepoId = @id;";
        cmd.Parameters.AddWithValue("@l", isLocked);
        cmd.Parameters.AddWithValue("@id", repoId);

        int rows = await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
        if (rows == 0)
        {
            return (false, "repository not found.");
        }

        return (true, "ok");
    }

    public async Task<(bool Ok, string Message)> MoveToRepoBinAsync(
        User actor, int repoId, CancellationToken ct = default)
    {
        if (!RbacGuard.CanManageRepoBin(actor))
        {
            return (false, "only faculty can manage the repo bin.");
        }

        RepositoryRecord? current = await GetByIdAsync(repoId, ct).ConfigureAwait(false);
        if (current is null)
        {
            return (false, "repository not found.");
        }

        if (current.LifecycleStatus == LifecycleStatus.Trashed)
        {
            return (false, "repository is already in the repo bin.");
        }

        await using var conn = await _db.CreateOpenConnectionAsync(ct).ConfigureAwait(false);
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "UPDATE Repositories SET LifecycleStatus = 'Trashed', TrashedAt = @now " +
                          "WHERE RepoId = @id;";
        cmd.Parameters.AddWithValue("@now", DateTime.UtcNow);
        cmd.Parameters.AddWithValue("@id", repoId);
        await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);

        return (true, "ok");
    }

    public async Task<(bool Ok, string Message)> RestoreFromRepoBinAsync(
        User actor, int repoId, CancellationToken ct = default)
    {
        if (!RbacGuard.CanManageRepoBin(actor))
        {
            return (false, "only faculty can manage the repo bin.");
        }

        RepositoryRecord? current = await GetByIdAsync(repoId, ct).ConfigureAwait(false);
        if (current is null)
        {
            return (false, "repository not found.");
        }

        if (current.LifecycleStatus == LifecycleStatus.Active)
        {
            return (false, "repository is not in the repo bin.");
        }

        await using var conn = await _db.CreateOpenConnectionAsync(ct).ConfigureAwait(false);
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "UPDATE Repositories SET LifecycleStatus = 'Active', TrashedAt = NULL " +
                          "WHERE RepoId = @id;";
        cmd.Parameters.AddWithValue("@id", repoId);
        await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);

        return (true, "ok");
    }

    public async Task<(bool Ok, string Message)> PurgeFromRepoBinAsync(
        User actor, int repoId, CancellationToken ct = default)
    {
        if (!RbacGuard.CanManageRepoBin(actor))
        {
            return (false, "only faculty can manage the repo bin.");
        }

        RepositoryRecord? current = await GetByIdAsync(repoId, ct).ConfigureAwait(false);
        if (current is null)
        {
            return (false, "repository not found.");
        }

        if (current.LifecycleStatus != LifecycleStatus.Trashed)
        {
            return (false, "only trashed repositories can be purged, move it to the repo bin first.");
        }

        // rename the bare repo aside so a db failure can restore it
        string purging = current.BareRepoDiskPath + ".purging";
        bool renamed = false;
        try
        {
            if (Directory.Exists(current.BareRepoDiskPath))
            {
                if (Directory.Exists(purging))
                {
                    Directory.Delete(purging, true);
                }

                Directory.Move(current.BareRepoDiskPath, purging);
                renamed = true;
            }
        }
        catch (IOException ex)
        {
            return (false, "could not stage the bare repo for purge: " + ex.Message);
        }

        await using var conn = await _db.CreateOpenConnectionAsync(ct).ConfigureAwait(false);
        await using var tx = await conn.BeginTransactionAsync(ct).ConfigureAwait(false);
        try
        {
            await using (var ledger = conn.CreateCommand())
            {
                ledger.Transaction = tx;
                ledger.CommandText = "DELETE FROM PushLedger WHERE RepoId = @id;";
                ledger.Parameters.AddWithValue("@id", repoId);
                await ledger.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
            }

            await using (var repo = conn.CreateCommand())
            {
                repo.Transaction = tx;
                repo.CommandText = "DELETE FROM Repositories WHERE RepoId = @id;";
                repo.Parameters.AddWithValue("@id", repoId);
                int rows = await repo.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
                if (rows == 0)
                {
                    await tx.RollbackAsync(ct).ConfigureAwait(false);
                    if (renamed)
                    {
                        Directory.Move(purging, current.BareRepoDiskPath);
                    }

                    return (false, "repository not found.");
                }
            }

            await tx.CommitAsync(ct).ConfigureAwait(false);
        }
        catch
        {
            await tx.RollbackAsync(ct).ConfigureAwait(false);
            if (renamed)
            {
                Directory.Move(purging, current.BareRepoDiskPath);
            }

            throw;
        }

        // db row is gone, clear read-only flags (windows pack files) and delete the staged dir
        if (renamed)
        {
            ClearReadOnly(purging);
            Directory.Delete(purging, true);
        }

        return (true, "ok");
    }

    // windows marks git pack files read-only, which blocks recursive deletes
    private static void ClearReadOnly(string dir)
    {
        foreach (string file in Directory.GetFiles(dir, "*", SearchOption.AllDirectories))
        {
            File.SetAttributes(file, File.GetAttributes(file) & ~FileAttributes.ReadOnly);
        }
    }

    private const string SelectAll =
        "SELECT RepoId, CourseId, RepoSlug, Title, Description, BareRepoDiskPath, DeadlineUtc, " +
        "IsLocked, LifecycleStatus, TrashedAt, TotalFiles, SourceLines, CommentLines, BlankLines, " +
        "PrimaryLanguage, CreatedAt FROM Repositories";

    internal static RepositoryRecord MapRepo(MySqlDataReader reader)
    {
        return new RepositoryRecord
        {
            RepoId = Convert.ToInt32(reader["RepoId"]),
            CourseId = Convert.ToInt32(reader["CourseId"]),
            RepoSlug = reader["RepoSlug"]?.ToString() ?? string.Empty,
            Title = reader["Title"]?.ToString() ?? string.Empty,
            Description = reader.IsDBNull(reader.GetOrdinal("Description"))
                ? null : reader["Description"]?.ToString(),
            BareRepoDiskPath = reader["BareRepoDiskPath"]?.ToString() ?? string.Empty,
            DeadlineUtc = reader.IsDBNull(reader.GetOrdinal("DeadlineUtc"))
                ? null : (DateTime?)reader["DeadlineUtc"],
            IsLocked = Convert.ToBoolean(reader["IsLocked"]),
            LifecycleStatus = Enum.Parse<LifecycleStatus>(reader["LifecycleStatus"]?.ToString() ?? "Active"),
            TrashedAt = reader.IsDBNull(reader.GetOrdinal("TrashedAt"))
                ? null : (DateTime?)reader["TrashedAt"],
            TotalFiles = Convert.ToInt32(reader["TotalFiles"]),
            SourceLines = Convert.ToInt32(reader["SourceLines"]),
            CommentLines = Convert.ToInt32(reader["CommentLines"]),
            BlankLines = Convert.ToInt32(reader["BlankLines"]),
            PrimaryLanguage = reader.IsDBNull(reader.GetOrdinal("PrimaryLanguage"))
                ? null : reader["PrimaryLanguage"]?.ToString(),
            CreatedAt = (DateTime)reader["CreatedAt"],
        };
    }
}
