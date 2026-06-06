namespace SystemSyncEngine.Worker.Models;

public sealed class SyncErrorRecord
{
    public required string SyncName { get; init; }
    public required string ExternalId { get; init; }
    public required string ErrorMessage { get; init; }
    public required string RawRecordJson { get; init; }
    public DateTime FailedAtUtc { get; init; }
}
