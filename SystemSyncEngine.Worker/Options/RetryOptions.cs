namespace SystemSyncEngine.Worker.Options;

public sealed class RetryOptions
{
    public int MaxAttempts { get; init; } = 3;
    public int BaseDelaySeconds { get; init; } = 2;
}
