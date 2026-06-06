namespace SystemSyncEngine.Worker.Models;

public sealed class SyncRunRecord
{
    public required string SyncName { get; init; }
    public DateTime StartedAtUtc { get; init; }
    public DateTime CompletedAtUtc { get; init; }
    public int RecordsRead { get; init; }
    public int RecordsUpserted { get; init; }
    public int RecordsSkipped { get; init; }
    public int RecordsFailed { get; init; }
    public bool Succeeded { get; init; }
    public string? ErrorMessage { get; init; }
}
