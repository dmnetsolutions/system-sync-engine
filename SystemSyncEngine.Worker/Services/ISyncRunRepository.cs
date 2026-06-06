using SystemSyncEngine.Worker.Models;

namespace SystemSyncEngine.Worker.Services;

public interface ISyncRunRepository
{
    Task SaveRunAsync(
        SyncRunRecord runRecord,
        CancellationToken cancellationToken);
}
