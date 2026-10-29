using MySqlConnector;
using RepoVault.Core.Data;
using RepoVault.Core.Models;
using RepoVault.Core.Security;

namespace RepoVault.Core.Services;

public sealed class EvaluationService
{
    public const int MinScore = 0;
    public const int MaxScore = 25;

    private readonly DbHelper _db;

    public EvaluationService(DbHelper db)
    {
        _db = db;
    }

    public Task<(bool Ok, string Message, EvaluationRecord? Evaluation)> SubmitOrUpdateEvaluationAsync(
        User actor, int repoId, int architectureScore, int functionalityScore,
        int codeQualityScore, int documentationScore, string critiqueNotes,
        bool lockRepository = true, CancellationToken ct = default)
    {
        return SaveEvaluationAsync(
            actor, repoId, architectureScore, functionalityScore,
            codeQualityScore, documentationScore, critiqueNotes, lockRepository, ct);
    }

    public async Task<(bool Ok, string Message, EvaluationRecord? Evaluation)> SaveEvaluationAsync(
        User actor, int repoId, int architectureScore, int functionalityScore,
        int codeQualityScore, int documentationScore, string critiqueNotes,
        bool lockRepository = true, CancellationToken ct = default)
    {
        if (!RbacGuard.CanGradeRepository(actor))
        {
            return (false, "only faculty can grade repositories.", null);
        }

        int[] scores = [architectureScore, functionalityScore, codeQualityScore, documentationScore];
        foreach (int score in scores)
        {
            if (score < MinScore || score > MaxScore)
            {
                return (false, $"each rubric score must be between {MinScore} and {MaxScore}.", null);
            }
        }

        await using var conn = await _db.CreateOpenConnectionAsync(ct).ConfigureAwait(false);

        await using (var exists = conn.CreateCommand())
        {
            exists.CommandText = "SELECT 1 FROM Repositories WHERE RepoId = @id LIMIT 1;";
            exists.Parameters.AddWithValue("@id", repoId);
            object? hit = await exists.ExecuteScalarAsync(ct).ConfigureAwait(false);
            if (hit is null)
            {
                return (false, "repository not found.", null);
            }
        }

        await using var tx = await conn.BeginTransactionAsync(ct).ConfigureAwait(false);
        try
        {
            await using (var upsert = conn.CreateCommand())
            {
                upsert.Transaction = tx;
                upsert.CommandText = "INSERT INTO Evaluations (RepoId, FacultyUserId, ArchitectureScore, " +
                                     "FunctionalityScore, CodeQualityScore, DocumentationScore, CritiqueNotes, " +
                                     "EvaluatedAt, UpdatedAt) VALUES (@r, @f, @a, @fn, @q, @d, @n, @now, @now) " +
                                     "ON DUPLICATE KEY UPDATE FacultyUserId = @f, ArchitectureScore = @a, " +
                                     "FunctionalityScore = @fn, CodeQualityScore = @q, DocumentationScore = @d, " +
                                     "CritiqueNotes = @n;";
                upsert.Parameters.AddWithValue("@r", repoId);
                upsert.Parameters.AddWithValue("@f", actor.UserId);
                upsert.Parameters.AddWithValue("@a", architectureScore);
                upsert.Parameters.AddWithValue("@fn", functionalityScore);
                upsert.Parameters.AddWithValue("@q", codeQualityScore);
                upsert.Parameters.AddWithValue("@d", documentationScore);
                upsert.Parameters.AddWithValue("@n", (object?)critiqueNotes.Trim() ?? DBNull.Value);
                upsert.Parameters.AddWithValue("@now", DateTime.UtcNow);
                await upsert.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
            }

            await using (var lockRepo = conn.CreateCommand())
            {
                lockRepo.Transaction = tx;
                lockRepo.CommandText = "UPDATE Repositories SET IsLocked = @l WHERE RepoId = @id;";
                lockRepo.Parameters.AddWithValue("@l", lockRepository);
                lockRepo.Parameters.AddWithValue("@id", repoId);
                await lockRepo.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
            }

            await tx.CommitAsync(ct).ConfigureAwait(false);
        }
        catch
        {
            await tx.RollbackAsync(ct).ConfigureAwait(false);
            throw;
        }

        EvaluationRecord? saved = await GetEvaluationByRepoAsync(repoId, ct).ConfigureAwait(false);
        return (true, "ok", saved);
    }

    public async Task<EvaluationRecord?> GetEvaluationByRepoAsync(
        int repoId, CancellationToken ct = default)
    {
        await using var conn = await _db.CreateOpenConnectionAsync(ct).ConfigureAwait(false);
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT EvaluationId, RepoId, FacultyUserId, ArchitectureScore, FunctionalityScore, " +
                          "CodeQualityScore, DocumentationScore, TotalScore, CritiqueNotes, EvaluatedAt, UpdatedAt " +
                          "FROM Evaluations WHERE RepoId = @r LIMIT 1;";
        cmd.Parameters.AddWithValue("@r", repoId);

        await using var reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false);
        if (!await reader.ReadAsync(ct).ConfigureAwait(false))
        {
            return null;
        }

        return MapEvaluation(reader);
    }

    public async Task<(bool Ok, string Message)> DeleteEvaluationAsync(
        User actor, int repoId, CancellationToken ct = default)
    {
        if (!RbacGuard.CanGradeRepository(actor))
        {
            return (false, "only faculty can grade repositories.");
        }

        await using var conn = await _db.CreateOpenConnectionAsync(ct).ConfigureAwait(false);
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM Evaluations WHERE RepoId = @r;";
        cmd.Parameters.AddWithValue("@r", repoId);

        int rows = await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
        if (rows == 0)
        {
            return (false, "evaluation not found.");
        }

        return (true, "ok");
    }

    internal static EvaluationRecord MapEvaluation(MySqlDataReader reader)
    {
        return new EvaluationRecord
        {
            EvaluationId = Convert.ToInt32(reader["EvaluationId"]),
            RepoId = Convert.ToInt32(reader["RepoId"]),
            FacultyUserId = Convert.ToInt32(reader["FacultyUserId"]),
            ArchitectureScore = Convert.ToInt32(reader["ArchitectureScore"]),
            FunctionalityScore = Convert.ToInt32(reader["FunctionalityScore"]),
            CodeQualityScore = Convert.ToInt32(reader["CodeQualityScore"]),
            DocumentationScore = Convert.ToInt32(reader["DocumentationScore"]),
            TotalScore = Convert.ToInt32(reader["TotalScore"]),
            CritiqueNotes = reader.IsDBNull(reader.GetOrdinal("CritiqueNotes"))
                ? null : reader["CritiqueNotes"]?.ToString(),
            EvaluatedAt = (DateTime)reader["EvaluatedAt"],
            UpdatedAt = (DateTime)reader["UpdatedAt"],
        };
    }
}
