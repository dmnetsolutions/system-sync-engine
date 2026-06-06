using Microsoft.Data.Sqlite;
using SystemSyncEngine.Worker.Models;

namespace SystemSyncEngine.Worker.Services;

public sealed class SqliteSyncErrorRepository : ISyncErrorRepository
{
    private readonly string _connectionString;
    private readonly ILogger<SqliteSyncErrorRepository> _logger;

    public SqliteSyncErrorRepository(
        IConfiguration configuration,
        ILogger<SqliteSyncErrorRepository> logger)
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
            CREATE TABLE IF NOT EXISTS SyncErrors
            (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                SyncName TEXT NOT NULL,
                ExternalId TEXT NOT NULL,
                ErrorMessage TEXT NOT NULL,
                RawRecordJson TEXT NOT NULL,
                FailedAtUtc TEXT NOT NULL
            );
            """;

        await command.ExecuteNonQueryAsync(cancellationToken);

        _logger.LogInformation("SQLite sync errors table initialized.");
    }

    public async Task SaveErrorAsync(
        SyncErrorRecord errorRecord,
        CancellationToken cancellationToken)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        var command = connection.CreateCommand();

        command.CommandText =
            """
            INSERT INTO SyncErrors
            (
                SyncName,
                ExternalId,
                ErrorMessage,
                RawRecordJson,
                FailedAtUtc
            )
            VALUES
            (
                $SyncName,
                $ExternalId,
                $ErrorMessage,
                $RawRecordJson,
                $FailedAtUtc
            );
            """;

        command.Parameters.AddWithValue("$SyncName", errorRecord.SyncName);
        command.Parameters.AddWithValue("$ExternalId", errorRecord.ExternalId);
        command.Parameters.AddWithValue("$ErrorMessage", errorRecord.ErrorMessage);
        command.Parameters.AddWithValue("$RawRecordJson", errorRecord.RawRecordJson);
        command.Parameters.AddWithValue("$FailedAtUtc", errorRecord.FailedAtUtc.ToString("O"));

        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}