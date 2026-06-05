using SystemSyncEngine.Worker.Services;

namespace SystemSyncEngine.Worker;

public sealed class Worker : BackgroundService
{
    private readonly CustomerSyncService _customerSyncService;
    private readonly IDestinationRepository _destinationRepository;
    private readonly ILogger<Worker> _logger;

    public Worker(
        CustomerSyncService customerSyncService,
        IDestinationRepository destinationRepository,
        ILogger<Worker> logger)
    {
        _customerSyncService = customerSyncService;
        _destinationRepository = destinationRepository;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var sinceUtc = DateTime.UtcNow.AddHours(-1);

        _logger.LogInformation(
            "Starting customer sync for records updated since {SinceUtc}.",
            sinceUtc);

        var result = await _customerSyncService.SyncCustomersAsync(
            sinceUtc,
            stoppingToken);

        _logger.LogInformation(
            "Customer sync completed. Read: {RecordsRead}, Written: {RecordsWritten}, Skipped: {RecordsSkipped}, Failed: {RecordsFailed}, Succeeded: {Succeeded}",
            result.RecordsRead,
            result.RecordsWritten,
            result.RecordsSkipped,
            result.RecordsFailed,
            result.Succeeded);

        if (_destinationRepository is InMemoryDestinationRepository inMemoryRepository)
        {
            var syncedCustomers = inMemoryRepository.GetAllCustomers();

            foreach (var customer in syncedCustomers)
            {
                _logger.LogInformation(
                    "Synced customer: {ExternalId} | {FullName} | {Email} | {PhoneNumber}",
                    customer.ExternalId,
                    customer.FullName,
                    customer.Email,
                    customer.PhoneNumber);
            }
        }
    }
}