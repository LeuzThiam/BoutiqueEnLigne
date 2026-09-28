namespace BoutiqueEnLigne.Contracts.Ordering;

public sealed record OrderCreatedIntegrationEvent(
    Guid EventId,
    DateTime OccurredAtUtc,
    int OrderId,
    int UserId,
    decimal Total,
    IReadOnlyCollection<OrderCreatedItem> Items)
{
    public const string EventName = "ordering.order-created.v1";
}

public sealed record OrderCreatedItem(
    int ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice);
