using RepoVault.Core.Data;

namespace RepoVault.Tests;

public sealed class DatabaseSchemaTests
{
    private static readonly string[] ExpectedTables =
    [
        "Users",
        "Courses",
        "Repositories",
        "RepositoryContributors",
        "RepositoryLanguages",
        "PushLedger",
        "CommitLogs",
        "DiscussionComments",
        "Evaluations",
    ];

    [Fact]
    public void Factory_DefaultsToXamppRoot()
    {
        string? saved = Environment.GetEnvironmentVariable(DbHelper.EnvVarName);
        try
        {
            Environment.SetEnvironmentVariable(DbHelper.EnvVarName, null);
            var factory = new DbHelper();
            Assert.Contains("Server=127.0.0.1", factory.ConnectionString);
            Assert.Contains("Database=repo_vault_db", factory.ConnectionString);
        }
        finally
        {
            Environment.SetEnvironmentVariable(DbHelper.EnvVarName, saved);
        }
    }

    [Fact]
    public void Factory_EnvVarOverridesDefault()
    {
        string? saved = Environment.GetEnvironmentVariable(DbHelper.EnvVarName);
        try
        {
            const string custom = "Server=127.0.0.1;Port=3306;Database=repo_vault_db;User ID=repovault;";
            Environment.SetEnvironmentVariable(DbHelper.EnvVarName, custom);
            var factory = new DbHelper();
            Assert.Equal(custom, factory.ConnectionString);
        }
        finally
        {
            Environment.SetEnvironmentVariable(DbHelper.EnvVarName, saved);
        }
    }

    [Fact]
    public async Task TestConnection_ReturnsOk_WhenXamppRunning()
    {
        var factory = new DbHelper();
        var (ok, message) = await factory.TestConnectionAsync();
        Assert.True(ok, message);
    }

    [Fact]
    public async Task AllNineTablesExist_InRepoVaultDb()
    {
        var factory = new DbHelper();
        await using var conn = await factory.CreateOpenConnectionAsync();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT TABLE_NAME FROM information_schema.TABLES WHERE TABLE_SCHEMA = 'repo_vault_db';";

        var found = new HashSet<string>(StringComparer.Ordinal);
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            found.Add(reader.GetString(0));
        }

        foreach (string table in ExpectedTables)
        {
            Assert.Contains(table, found);
        }
    }

    [Fact]
    public async Task PushLedger_PusherFkUsesRestrict()
    {
        var factory = new DbHelper();
        await using var conn = await factory.CreateOpenConnectionAsync();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT DELETE_RULE FROM information_schema.REFERENTIAL_CONSTRAINTS " +
                          "WHERE CONSTRAINT_SCHEMA = 'repo_vault_db' AND CONSTRAINT_NAME = 'fk_push_pusher';";

        object? value = await cmd.ExecuteScalarAsync();
        Assert.NotNull(value);
        Assert.Equal("RESTRICT", value.ToString());
    }

    [Fact]
    public async Task Evaluations_HasGeneratedTotalScore()
    {
        var factory = new DbHelper();
        await using var conn = await factory.CreateOpenConnectionAsync();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT EXTRA FROM information_schema.COLUMNS " +
                          "WHERE TABLE_SCHEMA = 'repo_vault_db' AND TABLE_NAME = 'Evaluations' AND COLUMN_NAME = 'TotalScore';";

        object? value = await cmd.ExecuteScalarAsync();
        Assert.NotNull(value);
        Assert.Contains("GENERATED", value.ToString(), StringComparison.OrdinalIgnoreCase);
    }
}
