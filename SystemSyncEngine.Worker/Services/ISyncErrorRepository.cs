using SystemSyncEngine.Worker.Models;

namespace SystemSyncEngine.Worker.Services;

public interface ISyncErrorRepository
{
    Task SaveErrorAsync(
        SyncErrorRecord errorRecord,
        CancellationToken cancellationToken);
}