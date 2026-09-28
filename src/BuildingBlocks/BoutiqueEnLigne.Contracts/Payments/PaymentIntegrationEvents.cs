namespace BoutiqueEnLigne.Contracts.Payments;

public sealed record PaymentSucceededIntegrationEvent(
    Guid EventId,
    DateTime OccurredAtUtc,
    Guid PaymentId,
    int OrderId,
    int UserId,
    long Amount,
    string Currency)
{
    public const string EventName = "payments.payment-succeeded.v1";
}

public sealed record PaymentFailedIntegrationEvent(
    Guid EventId,
    DateTime OccurredAtUtc,
    Guid PaymentId,
    int OrderId,
    int UserId,
    string? Reason)
{
    public const string EventName = "payments.payment-failed.v1";
}
