namespace SystemSyncEngine.Worker.Models;

public sealed class CustomerRecord
{
    public required string ExternalId { get; init; }
    public required string FullName { get; init; }
    public required string Email { get; init; }
    public string? PhoneNumber { get; init; }
    public DateTime SourceUpdatedAtUtc { get; init; }
    public DateTime SyncedAtUtc { get; init; }
}
