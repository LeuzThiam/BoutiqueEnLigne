namespace BoutiqueEnLigne.Ordering.Infrastructure.Persistence;

public sealed class InboxMessage
{
    public Guid Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public DateTime ReceivedAtUtc { get; set; }
    public DateTime ProcessedAtUtc { get; set; }
}
