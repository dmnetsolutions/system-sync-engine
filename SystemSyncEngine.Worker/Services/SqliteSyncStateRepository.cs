using Microsoft.Data.Sqlite;

namespace SystemSyncEngine.Worker.Services;

public sealed class SqliteSyncStateRepository : ISyncStateRepository
{
    private readonly string _connectionString;
    private readonly ILogger<SqliteSyncStateRepository> _logger;

    public SqliteSyncStateRepository(
        IConfiguration configuration,
        ILogger<SqliteSyncStateRepository> logger)
    {
        _connectionString = configuration.GetConnectionString("SyncDatabase")
            ?? throw new InvalidOperationException("Connection string 'SyncDatabase' is missing.");

        _logger = logger;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        var command = connection.CreateCommand();

        command.CommandText =
            """
            CREATE TABLE IF NOT EXISTS SyncState
            (
                SyncName TEXT NOT NULL PRIMARY KEY,
                LastSuccessfulSyncUtc TEXT NOT NULL
            );
            """;

        await command.ExecuteNonQueryAsync(cancellationToken);

        _logger.LogInformation("SQLite sync state table initialized.");
    }

    public async Task<DateTime?> GetLastSuccessfulSyncUtcAsync(
        string syncName,
        CancellationToken cancellationToken)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        var command = connection.CreateCommand();

        command.CommandText =
            """
            SELECT LastSuccessfulSyncUtc
            FROM SyncState
            WHERE SyncName = $SyncName;
            """;

        command.Parameters.AddWithValue("$SyncName", syncName);

        var result = await command.ExecuteScalarAsync(cancellationToken);

        if (result is null || result == DBNull.Value)
        {
            return null;
        }

        return DateTime.Parse(
            result.ToString()!, 
            null, 
            System.Globalization.DateTimeStyles.RoundtripKind
            ).ToUniversalTime();
    }

    public async Task SaveLastSuccessfulSyncUtcAsync(
        string syncName,
        DateTime completedAtUtc,
        CancellationToken cancellationToken)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        var command = connection.CreateCommand();

        command.CommandText =
            """
            INSERT INTO SyncState
            (
                SyncName,
                LastSuccessfulSyncUtc
            )
            VALUES
            (
                $SyncName,
                $LastSuccessfulSyncUtc
            )
            ON CONFLICT(SyncName) DO UPDATE SET
                LastSuccessfulSyncUtc = excluded.LastSuccessfulSyncUtc;
            """;

        command.Parameters.AddWithValue("$SyncName", syncName);
        command.Parameters.AddWithValue("$LastSuccessfulSyncUtc", completedAtUtc.ToString("O"));

        await command.ExecuteNonQueryAsync(cancellationToken);

        _logger.LogInformation("Saved sync checkpoint for {SyncName}: {CompletedAtUtc:O}",syncName, completedAtUtc);
    }
}
