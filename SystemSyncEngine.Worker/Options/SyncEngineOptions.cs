namespace SystemSyncEngine.Worker.Options;

public sealed class SyncEngineOptions
{
    public string SyncName { get; init; } = "CustomerSync";
    public int DefaultLookbackHours { get; init; } = 24;
    public bool LogSyncedCustomers { get; init; } = true;
}
