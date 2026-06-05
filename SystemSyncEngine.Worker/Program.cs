using SystemSyncEngine.Worker;
using SystemSyncEngine.Worker.Services;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddHostedService<Worker>();

builder.Services.AddSingleton<ISourceSystemClient, FakeSourceSystemClient>();
builder.Services.AddSingleton<IDestinationRepository, InMemoryDestinationRepository>();
builder.Services.AddSingleton<CustomerSyncService>();

var host = builder.Build();
host.Run();
