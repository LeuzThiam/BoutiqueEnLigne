namespace BoutiqueEnLigne.Payments.Domain.Entities;

public sealed class ProcessedWebhook
{
    public string StripeEventId { get; set; } = string.Empty;
    public DateTime ProcessedAtUtc { get; set; } = DateTime.UtcNow;
}
