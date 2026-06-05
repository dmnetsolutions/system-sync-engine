using SystemSyncEngine.Worker.Models;

namespace SystemSyncEngine.Worker.Services;

public sealed class FakeSourceSystemClient : ISourceSystemClient
{
    public Task<IReadOnlyList<SourceCustomer>> GetUpdatedCustomersAsync(
        DateTime sinceUtc,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<SourceCustomer> customers =
        [
            new SourceCustomer
            {
                ExternalId = "CRM-1001",
                FullName = "Sarah Johnson",
                Email = "sarah.johnson@example.com",
                PhoneNumber = "555-1001",
                UpdatedAtUtc = DateTime.UtcNow.AddMinutes(-30)
            },
            new SourceCustomer
            {
                ExternalId = "CRM-1002",
                FullName = "Michael Chen",
                Email = "MICHAEL.CHEN@EXAMPLE.COM",
                PhoneNumber = "555-1002",
                UpdatedAtUtc = DateTime.UtcNow.AddMinutes(-20)
            },
            new SourceCustomer
            {
                ExternalId = "CRM-1003",
                FullName = "  Angela Rivera  ",
                Email = "angela.rivera@example.com",
                PhoneNumber = null,
                UpdatedAtUtc = DateTime.UtcNow.AddMinutes(-10)
            },
            new SourceCustomer
            {
                ExternalId = "CRM-1004",
                FullName = "Invalid Missing Email",
                Email = "",
                PhoneNumber = "555-9999",
                UpdatedAtUtc = DateTime.UtcNow.AddMinutes(-5)
            }
        ];

        var updatedCustomers = customers
            .Where(customer => customer.UpdatedAtUtc >= sinceUtc)
            .ToList()
            .AsReadOnly();

        return Task.FromResult<IReadOnlyList<SourceCustomer>>(updatedCustomers);
    }
}
