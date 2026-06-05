using SystemSyncEngine.Worker;
using SystemSyncEngine.Worker.Options;
using SystemSyncEngine.Worker.Services;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<SyncEngineOptions>(
    builder.Configuration.GetSection("SyncEngine"));

builder.Services.Configure<RetryOptions>(
    builder.Configuration.GetSection("Retry"));

builder.Services.AddHostedService<Worker>();

builder.Services.AddSingleton<ISourceSystemClient, FakeSourceSystemClient>();
builder.Services.AddSingleton<IDestinationRepository, SqliteDestinationRepository>();
builder.Services.AddSingleton<ISyncStateRepository, SqliteSyncStateRepository>();
builder.Services.AddSingleton<ISyncRunRepository, SqliteSyncRunRepository>();
builder.Services.AddSingleton<IRetryPolicy, SimpleRetryPolicy>();
builder.Services.AddSingleton<CustomerSyncService>();

var host = builder.Build();
host.Run();
