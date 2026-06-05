using SystemSyncEngine.Worker.Models;

namespace SystemSyncEngine.Worker.Services;

public interface IDestinationRepository
{
    Task UpsertCustomerAsync(
        CustomerRecord customer,
        CancellationToken cancellationToken);
}
