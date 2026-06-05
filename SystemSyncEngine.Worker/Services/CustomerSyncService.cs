using SystemSyncEngine.Worker.Models;

namespace SystemSyncEngine.Worker.Services;

public sealed class CustomerSyncService
{
    private readonly ISourceSystemClient _sourceClient;
    private readonly IDestinationRepository _destinationRepository;
    private readonly ILogger<CustomerSyncService> _logger;

    public CustomerSyncService(
        ISourceSystemClient sourceClient,
        IDestinationRepository destinationRepository,
        ILogger<CustomerSyncService> logger)
    {
        _sourceClient = sourceClient;
        _destinationRepository = destinationRepository;
        _logger = logger;
    }

    public async Task<SyncResult> SyncCustomersAsync(
        DateTime sinceUtc,
        CancellationToken cancellationToken)
    {
        var sourceCustomers = await _sourceClient.GetUpdatedCustomersAsync(
            sinceUtc,
            cancellationToken);

        var recordsWritten = 0;
        var recordsSkipped = 0;
        var recordsFailed = 0;

        foreach (var sourceCustomer in sourceCustomers)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(sourceCustomer.Email))
                {
                    recordsSkipped++;
                    _logger.LogWarning(
                        "Skipping customer {ExternalId} because email is missing.",
                        sourceCustomer.ExternalId);

                    continue;
                }

                var customerRecord = new CustomerRecord
                {
                    ExternalId = sourceCustomer.ExternalId,
                    FullName = sourceCustomer.FullName.Trim(),
                    Email = sourceCustomer.Email.Trim().ToLowerInvariant(),
                    PhoneNumber = sourceCustomer.PhoneNumber,
                    SourceUpdatedAtUtc = sourceCustomer.UpdatedAtUtc,
                    SyncedAtUtc = DateTime.UtcNow
                };

                await _destinationRepository.UpsertCustomerAsync(
                    customerRecord,
                    cancellationToken);

                recordsWritten++;
            }
            catch (Exception ex)
            {
                recordsFailed++;

                _logger.LogError(
                    ex,
                    "Failed to sync customer {ExternalId}.",
                    sourceCustomer.ExternalId);
            }
        }

        return new SyncResult
        {
            RecordsRead = sourceCustomers.Count,
            RecordsWritten = recordsWritten,
            RecordsSkipped = recordsSkipped,
            RecordsFailed = recordsFailed
        };
    }
}
