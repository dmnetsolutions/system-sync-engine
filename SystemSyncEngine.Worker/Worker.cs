using SystemSyncEngine.Worker.Models;
using SystemSyncEngine.Worker.Services;
using Microsoft.Extensions.Options;
using SystemSyncEngine.Worker.Options;

namespace SystemSyncEngine.Worker;

public sealed class Worker : BackgroundService
{
    private readonly SyncEngineOptions _options;
    private readonly ISyncRunRepository _syncRunRepository;
    private readonly ISyncErrorRepository _syncErrorRepository;
    private readonly CustomerSyncService _customerSyncService;
    private readonly IDestinationRepository _destinationRepository;
    private readonly ISyncStateRepository _syncStateRepository;
    private readonly IHostApplicationLifetime _applicationLifetime;
    private readonly ILogger<Worker> _logger;

    public Worker(
        CustomerSyncService customerSyncService,
        IDestinationRepository destinationRepository,
        ISyncStateRepository syncStateRepository,
        ISyncRunRepository syncRunRepository,
        ISyncErrorRepository syncErrorRepository,
        IHostApplicationLifetime applicationLifetime,
        IOptions<SyncEngineOptions> options,
        ILogger<Worker> logger)
    {
        _customerSyncService = customerSyncService;
        _destinationRepository = destinationRepository;
        _syncRunRepository = syncRunRepository;
        _syncErrorRepository = syncErrorRepository;
        _syncStateRepository = syncStateRepository;
        _applicationLifetime = applicationLifetime;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var syncStartedAtUtc = DateTime.UtcNow;
        SyncResult? result = null;
        string? unhandledErrorMessage = null;

        try
        {
            await InitializeStorageAsync(stoppingToken);

            var lastSuccessfulSyncUtc =
                await _syncStateRepository.GetLastSuccessfulSyncUtcAsync(
                    _options.SyncName,
                    stoppingToken);

            var sinceUtc = lastSuccessfulSyncUtc ?? DateTime.UtcNow.AddHours(-_options.DefaultLookbackHours);

            _logger.LogInformation(
                "Starting {SyncName} for records updated since {SinceUtc:O}.",
                _options.SyncName,
                sinceUtc);

            result = await _customerSyncService.SyncCustomersAsync(
                sinceUtc,
                stoppingToken);

            _logger.LogInformation(
                "{SyncName} completed. Read: {RecordsRead}, Upserted: {RecordsUpserted}, Skipped: {RecordsSkipped}, Failed: {RecordsFailed}, Succeeded: {Succeeded}",
                _options.SyncName,
                result.RecordsRead,
                result.RecordsUpserted,
                result.RecordsSkipped,
                result.RecordsFailed,
                result.Succeeded);

            if (result.Succeeded)
            {
                await _syncStateRepository.SaveLastSuccessfulSyncUtcAsync(
                    _options.SyncName,
                    syncStartedAtUtc,
                    stoppingToken);
            }
            else
            {
                _logger.LogWarning(
                    "Checkpoint was not updated because {SyncName} had failures.",
                    _options.SyncName);
            }

            if (_options.LogSyncedCustomers)
            {
                await LogSyncedCustomersAsync(stoppingToken);
            }
        }
        catch (Exception ex)
        {
            unhandledErrorMessage = ex.Message;
            _logger.LogError(ex, "Unhandled error while running sync worker.");
        }
        finally
        {
            await SaveRunHistoryAsync(
                syncStartedAtUtc,
                DateTime.UtcNow,
                result,
                unhandledErrorMessage,
                stoppingToken);

            _applicationLifetime.StopApplication();
        }
    }

    private async Task InitializeStorageAsync(CancellationToken cancellationToken)
    {
        if (_syncRunRepository is SqliteSyncRunRepository sqliteSyncRunRepository)
        {
            await sqliteSyncRunRepository.InitializeAsync(cancellationToken);
        }

        if (_syncErrorRepository is SqliteSyncErrorRepository sqliteSyncErrorRepository)
        {
            await sqliteSyncErrorRepository.InitializeAsync(cancellationToken);
        }

        if (_destinationRepository is SqliteDestinationRepository sqliteDestinationRepository)
        {
            await sqliteDestinationRepository.InitializeAsync(cancellationToken);
        }

        if (_syncStateRepository is SqliteSyncStateRepository sqliteSyncStateRepository)
        {
            await sqliteSyncStateRepository.InitializeAsync(cancellationToken);
        }
    }

    private async Task LogSyncedCustomersAsync(CancellationToken cancellationToken)
    {
        if (_destinationRepository is not SqliteDestinationRepository repository)
        {
            return;
        }

        var syncedCustomers = await repository.GetAllCustomersAsync(cancellationToken);

        foreach (var customer in syncedCustomers)
        {
            _logger.LogInformation(
                "Database customer: {ExternalId} | {FullName} | {Email} | {PhoneNumber}",
                customer.ExternalId,
                customer.FullName,
                customer.Email,
                customer.PhoneNumber);
        }
    }

    private async Task SaveRunHistoryAsync(
    DateTime startedAtUtc,
    DateTime completedAtUtc,
    SyncResult? result,
    string? unhandledErrorMessage,
    CancellationToken cancellationToken)
    {
        await _syncRunRepository.SaveRunAsync(
            new SyncRunRecord
            {
                SyncName = _options.SyncName,
                StartedAtUtc = startedAtUtc,
                CompletedAtUtc = completedAtUtc,
                RecordsRead = result?.RecordsRead ?? 0,
                RecordsUpserted = result?.RecordsUpserted ?? 0,
                RecordsSkipped = result?.RecordsSkipped ?? 0,
                RecordsFailed = result?.RecordsFailed ?? 0,
                Succeeded = result?.Succeeded == true && unhandledErrorMessage is null,
                ErrorMessage = unhandledErrorMessage
            },
            cancellationToken);
    }
}