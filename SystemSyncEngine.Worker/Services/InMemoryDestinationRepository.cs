using System.Collections.Concurrent;
using SystemSyncEngine.Worker.Models;

namespace SystemSyncEngine.Worker.Services;

public sealed class InMemoryDestinationRepository : IDestinationRepository
{
    private readonly ConcurrentDictionary<string, CustomerRecord> _customers = new();

    public Task UpsertCustomerAsync(
        CustomerRecord customer,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(customer.ExternalId);

        _customers.AddOrUpdate(
            customer.ExternalId,
            customer,
            (_, _) => customer);

        return Task.CompletedTask;
    }

    public IReadOnlyCollection<CustomerRecord> GetAllCustomers()
    {
        return _customers.Values
            .OrderBy(customer => customer.ExternalId)
            .ToList()
            .AsReadOnly();
    }
}
