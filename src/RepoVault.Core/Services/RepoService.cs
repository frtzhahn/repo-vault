using System.Text;
using MySqlConnector;
using RepoVault.Core.Data;
using RepoVault.Core.GitServer;
using RepoVault.Core.Metrics;
using RepoVault.Core.Models;
using RepoVault.Core.Security;

namespace RepoVault.Core.Services;

public sealed class RepoService
{
    private readonly DbHelper _db;
    private readonly string _repoRoot;

    public RepoService(DbHelper db, string? repoRoot = null)
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
        string? description, DateTime? deadlineUtc, int? taskId = null, CancellationToken ct = default)
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
        int? ownerStudentId = actor.Role == UserRole.Student ? actor.UserId : null;
        await using (var cmd = conn.CreateCommand())
        {
            // insert with a throwaway path, the real path needs the new id
            cmd.CommandText = "INSERT INTO Repositories (CourseId, OwnerStudentId, TaskId, RepoSlug, " +
                              "Title, Description, BareRepoDiskPath, DeadlineUtc, CreatedAt) " +
                              "VALUES (@c, @owner, @task, @s, @t, @d, @p, @dl, @now);";
            cmd.Parameters.AddWithValue("@c", courseId);
            cmd.Parameters.AddWithValue("@owner", (object?)ownerStudentId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@task", (object?)taskId ?? DBNull.Value);
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

        // student creator is owner and lead contributor
        if (actor.Role == UserRole.Student)
        {
            await using var member = conn.CreateCommand();
            member.CommandText = "INSERT INTO RepositoryContributors (RepoId, StudentUserId, TeamRole, " +
                                 "AccessLevel, ApprovalStatus, AssignedAt) " +
                                 "VALUES (@r, @u, 'Lead', 'Owner', 'Approved', @now);";
            member.Parameters.AddWithValue("@r", repoId);
            member.Parameters.AddWithValue("@u", actor.UserId);
            member.Parameters.AddWithValue("@now", DateTime.UtcNow);
            await member.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
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

    public async Task<List<CommitLogEntry>> ListCommitLogsAsync(
        int repoId, CancellationToken ct = default)
    {
        await using var conn = await _db.CreateOpenConnectionAsync(ct).ConfigureAwait(false);
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT CommitId, RepoId, CommitHash, AuthorName, AuthorEmail, " +
                          "CommittedAt, CommitMessage FROM CommitLogs " +
                          "WHERE RepoId = @r ORDER BY CommittedAt DESC;";
        cmd.Parameters.AddWithValue("@r", repoId);

        var rows = new List<CommitLogEntry>();
        await using var reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false);
        while (await reader.ReadAsync(ct).ConfigureAwait(false))
        {
            rows.Add(new CommitLogEntry
            {
                CommitId = Convert.ToInt64(reader["CommitId"]),
                RepoId = Convert.ToInt32(reader["RepoId"]),
                CommitHash = reader["CommitHash"]?.ToString() ?? string.Empty,
                AuthorName = reader["AuthorName"]?.ToString() ?? string.Empty,
                AuthorEmail = reader["AuthorEmail"]?.ToString() ?? string.Empty,
                CommittedAt = (DateTime)reader["CommittedAt"],
                CommitMessage = reader["CommitMessage"]?.ToString() ?? string.Empty,
            });
        }

        return rows;
    }

    public async Task<List<PushLedgerViewDto>> ListPushLedgerViewAsync(
        int repoId, CancellationToken ct = default)
    {
        await using var conn = await _db.CreateOpenConnectionAsync(ct).ConfigureAwait(false);
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT p.PushId, p.RepoId, p.PusherUserId, u.Username, u.FullName, " +
                          "p.BranchRef, p.OldCommitHash, p.NewCommitHash, p.ClientIpAddress, p.PushedAt " +
                          "FROM PushLedger p JOIN Users u ON u.UserId = p.PusherUserId " +
                          "WHERE p.RepoId = @r ORDER BY p.PushedAt DESC;";
        cmd.Parameters.AddWithValue("@r", repoId);

        var rows = new List<PushLedgerViewDto>();
        await using var reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false);
        while (await reader.ReadAsync(ct).ConfigureAwait(false))
        {
            rows.Add(new PushLedgerViewDto(
                Convert.ToInt32(reader["PushId"]),
                Convert.ToInt32(reader["RepoId"]),
                Convert.ToInt32(reader["PusherUserId"]),
                reader["Username"]?.ToString() ?? string.Empty,
                reader["FullName"]?.ToString() ?? string.Empty,
                reader["BranchRef"]?.ToString() ?? string.Empty,
                reader["OldCommitHash"]?.ToString() ?? string.Empty,
                reader["NewCommitHash"]?.ToString() ?? string.Empty,
                reader.IsDBNull(reader.GetOrdinal("ClientIpAddress"))
                    ? null : reader["ClientIpAddress"]?.ToString(),
                (DateTime)reader["PushedAt"]));
        }

        return rows;
    }

    public async Task<List<RepositoryLanguage>> ListLanguagesAsync(
        int repoId, CancellationToken ct = default)
    {
        await using var conn = await _db.CreateOpenConnectionAsync(ct).ConfigureAwait(false);
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT RepoId, LanguageName, FileCount, SourceLines, CommentLines, " +
                          "BlankLines, PercentageShare FROM RepositoryLanguages " +
                          "WHERE RepoId = @r ORDER BY SourceLines DESC;";
        cmd.Parameters.AddWithValue("@r", repoId);

        var rows = new List<RepositoryLanguage>();
        await using var reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false);
        while (await reader.ReadAsync(ct).ConfigureAwait(false))
        {
            rows.Add(new RepositoryLanguage
            {
                RepoId = Convert.ToInt32(reader["RepoId"]),
                LanguageName = reader["LanguageName"]?.ToString() ?? string.Empty,
                FileCount = Convert.ToInt32(reader["FileCount"]),
                SourceLines = Convert.ToInt32(reader["SourceLines"]),
                CommentLines = Convert.ToInt32(reader["CommentLines"]),
                BlankLines = Convert.ToInt32(reader["BlankLines"]),
                PercentageShare = Convert.ToDecimal(reader["PercentageShare"]),
            });
        }

        return rows;
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

    public async Task<(bool Ok, string Message, RepositoryRecord? Repo)> StartCodingTaskAsync(
        User student, int taskId, CancellationToken ct = default)
    {
        if (!student.IsActive || student.Role != UserRole.Student)
        {
            return (false, "only active students can start coding tasks.", null);
        }

        var courseService = new CourseService(_db);
        CodingTask? task = await courseService.GetTaskByIdAsync(taskId, ct).ConfigureAwait(false);
        if (task is null)
        {
            return (false, "coding task not found.", null);
        }

        await using var conn = await _db.CreateOpenConnectionAsync(ct).ConfigureAwait(false);

        // check if student already has an active repository for this task
        await using (var existing = conn.CreateCommand())
        {
            existing.CommandText = "SELECT r.RepoId FROM Repositories r " +
                                   "WHERE r.TaskId = @t AND r.OwnerStudentId = @u AND r.LifecycleStatus = 'Active' LIMIT 1;";
            existing.Parameters.AddWithValue("@t", taskId);
            existing.Parameters.AddWithValue("@u", student.UserId);
            object? hit = await existing.ExecuteScalarAsync(ct).ConfigureAwait(false);
            if (hit is not null)
            {
                RepositoryRecord? existingRepo = await GetByIdAsync(Convert.ToInt32(hit), ct).ConfigureAwait(false);
                return (true, "already started", existingRepo);
            }
        }

        string rawSlug = $"task-{task.TaskId}-{student.Username}";
        string title = $"{task.Title} - {student.FullName}";
        var (createdOk, createdMsg, repo) = await CreateRepositoryAsync(
            student, task.CourseId, rawSlug, title, task.Instructions,
            task.DeadlineUtc, task.TaskId, ct).ConfigureAwait(false);

        if (!createdOk || repo is null)
        {
            return (createdOk, createdMsg, null);
        }

        // initialize bare repo and seed initial starter commit
        await GitProcessRunner.InitializeBareRepositoryAsync(repo.BareRepoDiskPath, ct).ConfigureAwait(false);
        await SeedStarterCommitAsync(repo.BareRepoDiskPath, task, student, ct).ConfigureAwait(false);

        // index initial commit and scanner
        await new CommitLogIndexer(_db).IndexCommitsAsync(repo.RepoId, repo.BareRepoDiskPath, ct).ConfigureAwait(false);
        await new CodeMetricsScanner(_db).ScanRepositoryMetricsAsync(repo.RepoId, repo.BareRepoDiskPath, ct).ConfigureAwait(false);

        RepositoryRecord? fresh = await GetByIdAsync(repo.RepoId, ct).ConfigureAwait(false);
        return (true, "ok", fresh);
    }

    public async Task<(bool Ok, string Message)> SetSubmissionStateAsync(
        User student, int repoId, bool isSubmitted, CancellationToken ct = default)
    {
        if (!student.IsActive)
        {
            return (false, "deactivated accounts cannot update submission status.");
        }

        RepositoryRecord? repo = await GetByIdAsync(repoId, ct).ConfigureAwait(false);
        if (repo is null)
        {
            return (false, "repository not found.");
        }

        if (repo.LifecycleStatus != LifecycleStatus.Active || repo.IsLocked)
        {
            return (false, "repository is locked or trashed.");
        }

        if (repo.DeadlineUtc.HasValue && repo.DeadlineUtc.Value < DateTime.UtcNow)
        {
            return (false, "deadline has passed, submission cannot be updated.");
        }

        // only the owner or an approved groupmate can submit
        var rbac = new RbacGuard(_db);
        if (!await rbac.CanPushToRepositoryAsync(student, repoId, ct).ConfigureAwait(false))
        {
            return (false, "only the repository owner or approved groupmates can submit.");
        }

        await using var conn = await _db.CreateOpenConnectionAsync(ct).ConfigureAwait(false);
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "UPDATE Repositories SET IsSubmitted = @s, SubmittedAt = @time WHERE RepoId = @id;";
        cmd.Parameters.AddWithValue("@s", isSubmitted ? 1 : 0);
        cmd.Parameters.AddWithValue("@time", isSubmitted ? (object)DateTime.UtcNow : DBNull.Value);
        cmd.Parameters.AddWithValue("@id", repoId);
        await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);

        return (true, "ok");
    }

    public async Task<List<RepositoryRecord>> ListVisibleRepositoriesForUserAsync(
        User viewer, int? courseIdFilter = null, CancellationToken ct = default)
    {
        if (!viewer.IsActive)
        {
            return new List<RepositoryRecord>();
        }

        await using var conn = await _db.CreateOpenConnectionAsync(ct).ConfigureAwait(false);
        await using var cmd = conn.CreateCommand();

        string sql;
        if (viewer.Role == UserRole.Faculty)
        {
            // faculty sees repos in courses they teach, plus repos where they are an approved reviewer
            sql = SelectAll + " WHERE LifecycleStatus = 'Active' AND (CourseId IN " +
                  "(SELECT CourseId FROM Courses WHERE CreatedByFacultyId = @uid) " +
                  "OR RepoId IN (SELECT RepoId FROM RepositoryContributors " +
                  "WHERE StudentUserId = @uid AND ApprovalStatus = 'Approved'))";
        }
        else
        {
            // students see repos they own, plus repos where they are approved contributors or peer viewers
            sql = SelectAll + " WHERE LifecycleStatus = 'Active' AND (OwnerStudentId = @uid " +
                  "OR RepoId IN (SELECT RepoId FROM RepositoryContributors " +
                  "WHERE StudentUserId = @uid AND ApprovalStatus = 'Approved'))";
        }

        if (courseIdFilter.HasValue)
        {
            sql += " AND CourseId = @cid";
            cmd.Parameters.AddWithValue("@cid", courseIdFilter.Value);
        }

        cmd.CommandText = sql + " ORDER BY CreatedAt DESC;";
        cmd.Parameters.AddWithValue("@uid", viewer.UserId);

        var list = new List<RepositoryRecord>();
        await using var reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false);
        while (await reader.ReadAsync(ct).ConfigureAwait(false))
        {
            list.Add(MapRepo(reader));
        }

        return list;
    }

    private static async Task SeedStarterCommitAsync(
        string barePath, CodingTask task, User student, CancellationToken ct)
    {
        string work = Path.Combine(Path.GetTempPath(), "repovault-task-" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(work);
        try
        {
            await GitProcessRunner.RunAsync(["init", "--initial-branch=main", "."], work, null, ct).ConfigureAwait(false);
            await GitProcessRunner.RunAsync(["config", "user.name", "RepoVault Classroom"], work, null, ct).ConfigureAwait(false);
            await GitProcessRunner.RunAsync(["config", "user.email", "classroom@repovault.local"], work, null, ct).ConfigureAwait(false);

            string readme = $"# {task.Title}\n\nStudent: {student.FullName} ({student.IdNumber})\n\n## Instructions\n{task.Instructions ?? "Classroom coding task."}\n";
            await File.WriteAllTextAsync(Path.Combine(work, "README.md"), readme, ct).ConfigureAwait(false);

            string starterCode = GetStarterTemplateContent(task.StarterTemplate, task.Title);
            string starterFile = GetStarterFileName(task.StarterTemplate);
            await File.WriteAllTextAsync(Path.Combine(work, starterFile), starterCode, ct).ConfigureAwait(false);

            await GitProcessRunner.RunAsync(["add", "."], work, null, ct).ConfigureAwait(false);
            await GitProcessRunner.RunAsync(["commit", "-m", "starter template for " + task.Title], work, null, ct).ConfigureAwait(false);
            await GitProcessRunner.RunAsync(["push", barePath, "HEAD:main"], work, null, ct).ConfigureAwait(false);
        }
        finally
        {
            try
            {
                if (Directory.Exists(work))
                {
                    Directory.Delete(work, true);
                }
            }
            catch
            {
            }
        }
    }

    private static string GetStarterFileName(string template) => template.ToLowerInvariant() switch
    {
        "java" => "Main.java",
        "python" or "py" => "main.py",
        "c" => "main.c",
        "cpp" or "c++" => "main.cpp",
        _ => "Program.cs",
    };

    private static string GetStarterTemplateContent(string template, string title) => template.ToLowerInvariant() switch
    {
        "java" => $"public class Main {{\n    public static void main(String[] args) {{\n        System.out.println(\"Starter for {title}\");\n    }}\n}}\n",
        "python" or "py" => $"def main():\n    print(\"Starter for {title}\")\n\nif __name__ == '__main__':\n    main()\n",
        "c" => $"#include <stdio.h>\n\nint main(void) {{\n    printf(\"Starter for {title}\\n\");\n    return 0;\n}}\n",
        _ => $"using System;\n\nConsole.WriteLine(\"Starter for {title}\");\n",
    };

    private const string SelectAll =
        "SELECT RepoId, CourseId, OwnerStudentId, TaskId, RepoSlug, Title, Description, " +
        "BareRepoDiskPath, DeadlineUtc, IsLocked, IsSubmitted, SubmittedAt, LifecycleStatus, " +
        "TrashedAt, TotalFiles, SourceLines, CommentLines, BlankLines, PrimaryLanguage, CreatedAt " +
        "FROM Repositories";

    internal static RepositoryRecord MapRepo(MySqlDataReader reader)
    {
        return new RepositoryRecord
        {
            RepoId = Convert.ToInt32(reader["RepoId"]),
            CourseId = Convert.ToInt32(reader["CourseId"]),
            OwnerStudentId = reader.IsDBNull(reader.GetOrdinal("OwnerStudentId"))
                ? null : (int?)Convert.ToInt32(reader["OwnerStudentId"]),
            TaskId = reader.IsDBNull(reader.GetOrdinal("TaskId"))
                ? null : (int?)Convert.ToInt32(reader["TaskId"]),
            RepoSlug = reader["RepoSlug"]?.ToString() ?? string.Empty,
            Title = reader["Title"]?.ToString() ?? string.Empty,
            Description = reader.IsDBNull(reader.GetOrdinal("Description"))
                ? null : reader["Description"]?.ToString(),
            BareRepoDiskPath = reader["BareRepoDiskPath"]?.ToString() ?? string.Empty,
            DeadlineUtc = reader.IsDBNull(reader.GetOrdinal("DeadlineUtc"))
                ? null : (DateTime?)reader["DeadlineUtc"],
            IsLocked = Convert.ToBoolean(reader["IsLocked"]),
            IsSubmitted = Convert.ToBoolean(reader["IsSubmitted"]),
            SubmittedAt = reader.IsDBNull(reader.GetOrdinal("SubmittedAt"))
                ? null : (DateTime?)reader["SubmittedAt"],
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
