namespace SystemSyncEngine.Worker.Models;

public sealed class SyncResult
{
    public int RecordsRead { get; init; }
    public int RecordsUpserted { get; init; }
    public int RecordsSkipped { get; init; }
    public int RecordsFailed { get; init; }

    public bool Succeeded => RecordsFailed == 0;
}
