using MySqlConnector;
using RepoVault.Core.Data;
using RepoVault.Core.Models;
using RepoVault.Core.Security;

namespace RepoVault.Core.Services;

public sealed class DiscussionService
{
    private readonly DbConnectionFactory _db;

    public DiscussionService(DbConnectionFactory db)
    {
        _db = db;
    }

    public async Task<(bool Ok, string Message, DiscussionComment? Comment)> AddCommentAsync(
        User actor, int repoId, string commentBody, CancellationToken ct = default)
    {
        if (!actor.IsActive)
        {
            return (false, "deactivated accounts cannot comment.", null);
        }

        commentBody = commentBody.Trim();
        if (commentBody.Length == 0)
        {
            return (false, "comment body is required.", null);
        }

        await using var conn = await _db.CreateOpenConnectionAsync(ct).ConfigureAwait(false);
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "INSERT INTO DiscussionComments (RepoId, AuthorUserId, CommentBody, CreatedAt, UpdatedAt) " +
                          "VALUES (@r, @u, @b, @now, @now);";
        cmd.Parameters.AddWithValue("@r", repoId);
        cmd.Parameters.AddWithValue("@u", actor.UserId);
        cmd.Parameters.AddWithValue("@b", commentBody);
        cmd.Parameters.AddWithValue("@now", DateTime.UtcNow);

        try
        {
            await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
        }
        catch (MySqlException ex) when (ex.Number == 1452)
        {
            return (false, "repository not found.", null);
        }

        int commentId = (int)cmd.LastInsertedId;
        DiscussionComment? created = await GetByIdAsync(commentId, ct).ConfigureAwait(false);
        return (true, "ok", created);
    }

    public async Task<(bool Ok, string Message)> EditCommentAsync(
        User actor, int commentId, string newBody, CancellationToken ct = default)
    {
        newBody = newBody.Trim();
        if (newBody.Length == 0)
        {
            return (false, "comment body is required.");
        }

        DiscussionComment? current = await GetByIdAsync(commentId, ct).ConfigureAwait(false);
        if (current is null)
        {
            return (false, "comment not found.");
        }

        if (!RbacGuard.CanEditOrDeleteComment(actor, current.AuthorUserId))
        {
            return (false, "you can only edit your own comments.");
        }

        await using var conn = await _db.CreateOpenConnectionAsync(ct).ConfigureAwait(false);
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "UPDATE DiscussionComments SET CommentBody = @b WHERE CommentId = @id;";
        cmd.Parameters.AddWithValue("@b", newBody);
        cmd.Parameters.AddWithValue("@id", commentId);
        await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);

        return (true, "ok");
    }

    public async Task<(bool Ok, string Message)> DeleteCommentAsync(
        User actor, int commentId, CancellationToken ct = default)
    {
        DiscussionComment? current = await GetByIdAsync(commentId, ct).ConfigureAwait(false);
        if (current is null)
        {
            return (false, "comment not found.");
        }

        if (!RbacGuard.CanEditOrDeleteComment(actor, current.AuthorUserId))
        {
            return (false, "you can only delete your own comments.");
        }

        await using var conn = await _db.CreateOpenConnectionAsync(ct).ConfigureAwait(false);
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM DiscussionComments WHERE CommentId = @id;";
        cmd.Parameters.AddWithValue("@id", commentId);
        await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);

        return (true, "ok");
    }

    public async Task<(bool Ok, string Message)> SetModerationHiddenAsync(
        User actor, int commentId, bool isHidden, CancellationToken ct = default)
    {
        if (!RbacGuard.CanModerateComment(actor))
        {
            return (false, "only faculty can moderate comments.");
        }

        await using var conn = await _db.CreateOpenConnectionAsync(ct).ConfigureAwait(false);
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "UPDATE DiscussionComments SET IsModeratedHidden = @h WHERE CommentId = @id;";
        cmd.Parameters.AddWithValue("@h", isHidden);
        cmd.Parameters.AddWithValue("@id", commentId);

        int rows = await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
        if (rows == 0)
        {
            return (false, "comment not found.");
        }

        return (true, "ok");
    }

    public async Task<List<DiscussionComment>> ListCommentsByRepoAsync(
        int repoId, bool includeModeratedHidden, CancellationToken ct = default)
    {
        await using var conn = await _db.CreateOpenConnectionAsync(ct).ConfigureAwait(false);
        await using var cmd = conn.CreateCommand();

        string sql = "SELECT CommentId, RepoId, AuthorUserId, CommentBody, IsModeratedHidden, " +
                     "CreatedAt, UpdatedAt FROM DiscussionComments WHERE RepoId = @r";
        if (!includeModeratedHidden)
        {
            sql += " AND IsModeratedHidden = 0";
        }

        cmd.CommandText = sql + ";";
        cmd.Parameters.AddWithValue("@r", repoId);

        var comments = new List<DiscussionComment>();
        await using var reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false);
        while (await reader.ReadAsync(ct).ConfigureAwait(false))
        {
            comments.Add(MapComment(reader));
        }

        return comments;
    }

    private async Task<DiscussionComment?> GetByIdAsync(int commentId, CancellationToken ct)
    {
        await using var conn = await _db.CreateOpenConnectionAsync(ct).ConfigureAwait(false);
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT CommentId, RepoId, AuthorUserId, CommentBody, IsModeratedHidden, " +
                          "CreatedAt, UpdatedAt FROM DiscussionComments WHERE CommentId = @id LIMIT 1;";
        cmd.Parameters.AddWithValue("@id", commentId);

        await using var reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false);
        if (!await reader.ReadAsync(ct).ConfigureAwait(false))
        {
            return null;
        }

        return MapComment(reader);
    }

    internal static DiscussionComment MapComment(MySqlDataReader reader)
    {
        return new DiscussionComment
        {
            CommentId = Convert.ToInt32(reader["CommentId"]),
            RepoId = Convert.ToInt32(reader["RepoId"]),
            AuthorUserId = Convert.ToInt32(reader["AuthorUserId"]),
            CommentBody = reader["CommentBody"]?.ToString() ?? string.Empty,
            IsModeratedHidden = Convert.ToBoolean(reader["IsModeratedHidden"]),
            CreatedAt = (DateTime)reader["CreatedAt"],
            UpdatedAt = (DateTime)reader["UpdatedAt"],
        };
    }
}
