using SystemSyncEngine.Worker.Models;

namespace SystemSyncEngine.Worker.Services;

public sealed class FakeSourceSystemClient : ISourceSystemClient
{
    private static readonly DateTime SampleDataAnchorUtc =
        new(2026, 06, 05, 15, 00, 00, DateTimeKind.Utc);

    private static readonly IReadOnlyList<SourceCustomer> Customers =
    [
        new SourceCustomer
        {
            ExternalId = "CRM-1001",
            FullName = "Sarah Johnson",
            Email = "sarah.johnson@example.com",
            PhoneNumber = "555-1001",
            UpdatedAtUtc = SampleDataAnchorUtc.AddMinutes(-30)
        },
        new SourceCustomer
        {
            ExternalId = "CRM-1002",
            FullName = "Michael Chen",
            Email = "MICHAEL.CHEN@EXAMPLE.COM",
            PhoneNumber = "555-1002",
            UpdatedAtUtc = SampleDataAnchorUtc.AddMinutes(-20)
        },
        new SourceCustomer
        {
            ExternalId = "CRM-1003",
            FullName = "  Angela Rivera  ",
            Email = "angela.rivera@example.com",
            PhoneNumber = null,
            UpdatedAtUtc = SampleDataAnchorUtc.AddMinutes(-10)
        },
        new SourceCustomer
        {
            ExternalId = "CRM-1004",
            FullName = "Invalid Missing Email",
            Email = "",
            PhoneNumber = "555-9999",
            UpdatedAtUtc = SampleDataAnchorUtc.AddMinutes(-5)
        }
    ];

    public Task<IReadOnlyList<SourceCustomer>> GetUpdatedCustomersAsync(
        DateTime sinceUtc,
        CancellationToken cancellationToken)
    {
        var updatedCustomers = Customers
            .Where(customer => customer.UpdatedAtUtc >= sinceUtc)
            .OrderBy(customer => customer.UpdatedAtUtc)
            .ToList()
            .AsReadOnly();

        return Task.FromResult<IReadOnlyList<SourceCustomer>>(updatedCustomers);
    }
}