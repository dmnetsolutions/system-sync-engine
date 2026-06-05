namespace SystemSyncEngine.Worker.Services;

public interface ISyncStateRepository
{
    Task<DateTime?> GetLastSuccessfulSyncUtcAsync(
        string syncName,
        CancellationToken cancellationToken);

    Task SaveLastSuccessfulSyncUtcAsync(
        string syncName,
        DateTime completedAtUtc,
        CancellationToken cancellationToken);
}
