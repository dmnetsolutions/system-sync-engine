using Microsoft.Extensions.Options;
using SystemSyncEngine.Worker.Options;

namespace SystemSyncEngine.Worker.Services;

public sealed class SimpleRetryPolicy : IRetryPolicy
{
    private readonly RetryOptions _options;
    private readonly ILogger<SimpleRetryPolicy> _logger;

    public SimpleRetryPolicy(
        IOptions<RetryOptions> options,
        ILogger<SimpleRetryPolicy> logger)
    {
        _options = options.Value;
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
        for (var attempt = 1; attempt <= _options.MaxAttempts; attempt++)
        {
            try
            {
                return await operation(cancellationToken);
            }
            catch (Exception ex) when (attempt < _options.MaxAttempts)
            {
                var delay = TimeSpan.FromSeconds(
                    _options.BaseDelaySeconds * Math.Pow(2, attempt - 1));

                _logger.LogWarning(
                    ex,
                    "Operation {OperationName} failed on attempt {Attempt}/{MaxAttempts}. Retrying in {DelaySeconds} seconds.",
                    operationName,
                    attempt,
                    _options.MaxAttempts,
                    delay.TotalSeconds);

                await Task.Delay(delay, cancellationToken);
            }
        }

        return await operation(cancellationToken);
    }
}