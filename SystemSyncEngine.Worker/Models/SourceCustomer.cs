namespace SystemSyncEngine.Worker.Models;

public sealed class SourceCustomer
{
    public required string ExternalId { get; init; }
    public required string FullName { get; init; }
    public required string Email { get; init; }
    public string? PhoneNumber { get; init; }
    public DateTime UpdatedAtUtc { get; init; }
}
