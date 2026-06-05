namespace SystemSyncEngine.Worker.Services;

public interface IRetryPolicy
{
    Task ExecuteAsync(
        Func<CancellationToken, Task> operation,
        string operationName,
        CancellationToken cancellationToken);

    Task<T> ExecuteAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        string operationName,
        CancellationToken cancellationToken);
}
