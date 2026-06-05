using SystemSyncEngine.Worker.Models;

namespace SystemSyncEngine.Worker.Services;

public interface ISourceSystemClient
{
    Task<IReadOnlyList<SourceCustomer>> GetUpdatedCustomersAsync(
        DateTime sinceUtc,
        CancellationToken cancellationToken);
}
