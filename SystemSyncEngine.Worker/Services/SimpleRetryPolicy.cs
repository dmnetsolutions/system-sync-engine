namespace SystemSyncEngine.Worker.Services;

public sealed class SimpleRetryPolicy : IRetryPolicy
{
    private readonly ILogger<SimpleRetryPolicy> _logger;

    private const int MaxAttempts = 3;

    public SimpleRetryPolicy(ILogger<SimpleRetryPolicy> logger)
    {
        _logger = logger;
    }

    public async Task ExecuteAsync(
        Func<CancellationToken, Task> operation,
        string operationName,
        CancellationToken cancellationToken)
    {
        await ExecuteAsync(
            async token =>
            {
                await operation(token);
                return true;
            },
            operationName,
            cancellationToken);
    }

    public async Task<T> ExecuteAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        string operationName,
        CancellationToken cancellationToken)
    {
        for (var attempt = 1; attempt <= MaxAttempts; attempt++)
        {
            try
            {
                return await operation(cancellationToken);
            }
            catch (Exception ex) when (attempt < MaxAttempts)
            {
                var delay = TimeSpan.FromSeconds(Math.Pow(2, attempt));

                _logger.LogWarning(
                    ex,
                    "Operation {OperationName} failed on attempt {Attempt}/{MaxAttempts}. Retrying in {DelaySeconds} seconds.",
                    operationName,
                    attempt,
                    MaxAttempts,
                    delay.TotalSeconds);

                await Task.Delay(delay, cancellationToken);
            }
        }

        return await operation(cancellationToken);
    }
}
