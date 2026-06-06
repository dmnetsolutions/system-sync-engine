using Microsoft.Data.Sqlite;
using SystemSyncEngine.Worker.Models;

namespace SystemSyncEngine.Worker.Services;

public sealed class SqliteSyncRunRepository : ISyncRunRepository
{
    private readonly string _connectionString;
    private readonly ILogger<SqliteSyncRunRepository> _logger;

    public SqliteSyncRunRepository(
        IConfiguration configuration,
        ILogger<SqliteSyncRunRepository> logger)
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
            CREATE TABLE IF NOT EXISTS SyncRuns
            (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                SyncName TEXT NOT NULL,
                StartedAtUtc TEXT NOT NULL,
                CompletedAtUtc TEXT NOT NULL,
                RecordsRead INTEGER NOT NULL,
                RecordsUpserted INTEGER NOT NULL,
                RecordsSkipped INTEGER NOT NULL,
                RecordsFailed INTEGER NOT NULL,
                Succeeded INTEGER NOT NULL,
                ErrorMessage TEXT NULL
            );
            """;

        await command.ExecuteNonQueryAsync(cancellationToken);

        _logger.LogInformation("SQLite sync runs table initialized.");
    }

    public async Task SaveRunAsync(
        SyncRunRecord runRecord,
        CancellationToken cancellationToken)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        var command = connection.CreateCommand();

        command.CommandText =
            """
            INSERT INTO SyncRuns
            (
                SyncName,
                StartedAtUtc,
                CompletedAtUtc,
                RecordsRead,
                RecordsUpserted,
                RecordsSkipped,
                RecordsFailed,
                Succeeded,
                ErrorMessage
            )
            VALUES
            (
                $SyncName,
                $StartedAtUtc,
                $CompletedAtUtc,
                $RecordsRead,
                $RecordsUpserted,
                $RecordsSkipped,
                $RecordsFailed,
                $Succeeded,
                $ErrorMessage
            );
            """;

        command.Parameters.AddWithValue("$SyncName", runRecord.SyncName);
        command.Parameters.AddWithValue("$StartedAtUtc", runRecord.StartedAtUtc.ToString("O"));
        command.Parameters.AddWithValue("$CompletedAtUtc", runRecord.CompletedAtUtc.ToString("O"));
        command.Parameters.AddWithValue("$RecordsRead", runRecord.RecordsRead);
        command.Parameters.AddWithValue("$RecordsUpserted", runRecord.RecordsUpserted);
        command.Parameters.AddWithValue("$RecordsSkipped", runRecord.RecordsSkipped);
        command.Parameters.AddWithValue("$RecordsFailed", runRecord.RecordsFailed);
        command.Parameters.AddWithValue("$Succeeded", runRecord.Succeeded ? 1 : 0);
        command.Parameters.AddWithValue("$ErrorMessage", (object?)runRecord.ErrorMessage ?? DBNull.Value);

        await command.ExecuteNonQueryAsync(cancellationToken);

        _logger.LogInformation(
            "Saved sync run for {SyncName}. Succeeded: {Succeeded}, Read: {RecordsRead}, Upserted: {RecordsUpserted}, Skipped: {RecordsSkipped}, Failed: {RecordsFailed}",
            runRecord.SyncName,
            runRecord.Succeeded,
            runRecord.RecordsRead,
            runRecord.RecordsUpserted,
            runRecord.RecordsSkipped,
            runRecord.RecordsFailed);
    }
}