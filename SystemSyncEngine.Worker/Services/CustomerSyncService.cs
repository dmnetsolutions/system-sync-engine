using SystemSyncEngine.Worker.Models;
using Microsoft.Extensions.Options;
using SystemSyncEngine.Worker.Options;

namespace SystemSyncEngine.Worker.Services;

public sealed class CustomerSyncService
{
    private readonly ISourceSystemClient _sourceClient;
    private readonly IDestinationRepository _destinationRepository;
    private readonly SyncEngineOptions _options;
    private readonly ISyncErrorRepository _syncErrorRepository;
    private readonly ILogger<CustomerSyncService> _logger;
    private readonly IRetryPolicy _retryPolicy;

    public CustomerSyncService(
    ISourceSystemClient sourceClient,
    IDestinationRepository destinationRepository,
    ISyncErrorRepository syncErrorRepository,
    IRetryPolicy retryPolicy,
    IOptions<SyncEngineOptions> options,
    ILogger<CustomerSyncService> logger)
    {
        _sourceClient = sourceClient;
        _destinationRepository = destinationRepository;
        _syncErrorRepository = syncErrorRepository;
        _retryPolicy = retryPolicy;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<SyncResult> SyncCustomersAsync(
        DateTime sinceUtc,
        CancellationToken cancellationToken)
    {
        var sourceCustomers = await _retryPolicy.ExecuteAsync(
                                                token => _sourceClient.GetUpdatedCustomersAsync(sinceUtc, token),
                                                            "Fetch updated customers from source system",
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

                await _retryPolicy.ExecuteAsync(
                        token => _destinationRepository.UpsertCustomerAsync(customerRecord, token),
                                    $"Upsert customer {customerRecord.ExternalId}",
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
            RecordsUpserted = recordsWritten,
            RecordsSkipped = recordsSkipped,
            RecordsFailed = recordsFailed
        };
    }
}
