using Microsoft.Data.Sqlite;
using SystemSyncEngine.Worker.Models;

namespace SystemSyncEngine.Worker.Services;

public sealed class SqliteDestinationRepository : IDestinationRepository
{
    private readonly string _connectionString;
    private readonly ILogger<SqliteDestinationRepository> _logger;

    public SqliteDestinationRepository(
        IConfiguration configuration,
        ILogger<SqliteDestinationRepository> logger)
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
            CREATE TABLE IF NOT EXISTS Customers
            (
                ExternalId TEXT NOT NULL PRIMARY KEY,
                FullName TEXT NOT NULL,
                Email TEXT NOT NULL,
                PhoneNumber TEXT NULL,
                SourceUpdatedAtUtc TEXT NOT NULL,
                SyncedAtUtc TEXT NOT NULL
            );
            """;

        await command.ExecuteNonQueryAsync(cancellationToken);

        _logger.LogInformation("SQLite destination database initialized.");
    }

    public async Task UpsertCustomerAsync(
        CustomerRecord customer,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(customer.ExternalId);

        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        var command = connection.CreateCommand();

        command.CommandText =
            """
            INSERT INTO Customers
            (
                ExternalId,
                FullName,
                Email,
                PhoneNumber,
                SourceUpdatedAtUtc,
                SyncedAtUtc
            )
            VALUES
            (
                $ExternalId,
                $FullName,
                $Email,
                $PhoneNumber,
                $SourceUpdatedAtUtc,
                $SyncedAtUtc
            )
            ON CONFLICT(ExternalId) DO UPDATE SET
                FullName = excluded.FullName,
                Email = excluded.Email,
                PhoneNumber = excluded.PhoneNumber,
                SourceUpdatedAtUtc = excluded.SourceUpdatedAtUtc,
                SyncedAtUtc = excluded.SyncedAtUtc;
            """;

        command.Parameters.AddWithValue("$ExternalId", customer.ExternalId);
        command.Parameters.AddWithValue("$FullName", customer.FullName);
        command.Parameters.AddWithValue("$Email", customer.Email);
        command.Parameters.AddWithValue("$PhoneNumber", (object?)customer.PhoneNumber ?? DBNull.Value);
        command.Parameters.AddWithValue("$SourceUpdatedAtUtc", customer.SourceUpdatedAtUtc.ToString("O"));
        command.Parameters.AddWithValue("$SyncedAtUtc", customer.SyncedAtUtc.ToString("O"));

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<CustomerRecord>> GetAllCustomersAsync(
        CancellationToken cancellationToken)
    {
        var customers = new List<CustomerRecord>();

        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        var command = connection.CreateCommand();

        command.CommandText =
            """
            SELECT
                ExternalId,
                FullName,
                Email,
                PhoneNumber,
                SourceUpdatedAtUtc,
                SyncedAtUtc
            FROM Customers
            ORDER BY ExternalId;
            """;

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            customers.Add(new CustomerRecord
            {
                ExternalId = reader.GetString(0),
                FullName = reader.GetString(1),
                Email = reader.GetString(2),
                PhoneNumber = reader.IsDBNull(3) ? null : reader.GetString(3),
                SourceUpdatedAtUtc = DateTime.Parse(reader.GetString(4)),
                SyncedAtUtc = DateTime.Parse(reader.GetString(5))
            });
        }

        return customers;
    }
}
