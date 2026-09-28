using BoutiqueEnLigne.Payments.Domain.Enums;

namespace BoutiqueEnLigne.Payments.Domain.Entities;

public sealed class Payment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public int UserId { get; set; }
    public int OrderId { get; set; }
    public string StripePaymentIntentId { get; set; } = string.Empty;
    public long Amount { get; set; }
    public string Currency { get; set; } = "cad";
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    public void MarkSucceeded(DateTime occurredAtUtc)
    {
        Status = PaymentStatus.Succeeded;
        UpdatedAtUtc = occurredAtUtc;
    }

    public void MarkFailed(DateTime occurredAtUtc)
    {
        Status = PaymentStatus.Failed;
        UpdatedAtUtc = occurredAtUtc;
    }
}
