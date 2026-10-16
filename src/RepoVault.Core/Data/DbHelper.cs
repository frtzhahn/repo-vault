using MySqlConnector;

namespace RepoVault.Core.Data;

public sealed class DbHelper
{
    public const string EnvVarName = "REPOVAULT_DB_CONN";

    public const string DefaultConnectionString =
        "Server=127.0.0.1;Port=3306;Database=repo_vault_db;User ID=root;Password=;ConvertZeroDateTime=true;AllowUserVariables=true;";

    private readonly string _connString;

    public DbHelper(string? connectionString = null)
    {
        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            _connString = connectionString;
            return;
        }

        string? fromEnv = Environment.GetEnvironmentVariable(EnvVarName);
        _connString = string.IsNullOrWhiteSpace(fromEnv) ? DefaultConnectionString : fromEnv;
    }

    public string ConnectionString => _connString;

    public async Task<MySqlConnection> CreateOpenConnectionAsync(CancellationToken ct = default)
    {
        var conn = new MySqlConnection(_connString);
        await conn.OpenAsync(ct).ConfigureAwait(false);
        return conn;
    }

    // returns (false, message) instead of throwing, so callers survive XAMPP being stopped
    public async Task<(bool Ok, string Message)> TestConnectionAsync(CancellationToken ct = default)
    {
        try
        {
            await using var conn = await CreateOpenConnectionAsync(ct).ConfigureAwait(false);
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT 1;";
            await cmd.ExecuteScalarAsync(ct).ConfigureAwait(false);
            return (true, "ok");
        }
        catch (MySqlException ex)
        {
            return (false, $"mysql error {ex.Number}: {ex.Message}");
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }
}
