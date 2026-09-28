using System.Text.Json;
using BoutiqueEnLigne.Contracts.Payments;
using BoutiqueEnLigne.Ordering.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BoutiqueEnLigne.Ordering.Infrastructure.Messaging;

public sealed class PaymentEventHandler
{
    private readonly OrderingDbContext _dbContext;

    public PaymentEventHandler(OrderingDbContext dbContext) => _dbContext = dbContext;

    public Task HandleAsync(
        string eventName,
        ReadOnlyMemory<byte> payload,
        CancellationToken cancellationToken = default) => eventName switch
    {
        PaymentSucceededIntegrationEvent.EventName => HandleAsync(
            Deserialize<PaymentSucceededIntegrationEvent>(payload.Span), cancellationToken),
        PaymentFailedIntegrationEvent.EventName => HandleAsync(
            Deserialize<PaymentFailedIntegrationEvent>(payload.Span), cancellationToken),
        _ => throw new JsonException($"Unsupported integration event: {eventName}")
    };

    public Task HandleAsync(
        PaymentSucceededIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default) =>
        ApplyAsync(
            integrationEvent.EventId,
            PaymentSucceededIntegrationEvent.EventName,
            integrationEvent.OrderId,
            integrationEvent.UserId,
            succeeded: true,
            cancellationToken);

    public Task HandleAsync(
        PaymentFailedIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default) =>
        ApplyAsync(
            integrationEvent.EventId,
            PaymentFailedIntegrationEvent.EventName,
            integrationEvent.OrderId,
            integrationEvent.UserId,
            succeeded: false,
            cancellationToken);

    private async Task ApplyAsync(
        Guid eventId,
        string eventName,
        int orderId,
        int userId,
        bool succeeded,
        CancellationToken cancellationToken)
    {
        if (await _dbContext.InboxMessages.AnyAsync(message => message.Id == eventId, cancellationToken))
        {
            return;
        }

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        var order = await _dbContext.Orders.FirstOrDefaultAsync(
            item => item.Id == orderId && item.UtilisateurId == userId,
            cancellationToken) ?? throw new InvalidOperationException($"Order {orderId} was not found.");

        if (succeeded) order.ConfirmPayment();
        else order.RegisterPaymentFailure();

        var now = DateTime.UtcNow;
        _dbContext.InboxMessages.Add(new InboxMessage
        {
            Id = eventId,
            Type = eventName,
            ReceivedAtUtc = now,
            ProcessedAtUtc = now
        });
        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    private static T Deserialize<T>(ReadOnlySpan<byte> payload) =>
        JsonSerializer.Deserialize<T>(payload) ?? throw new JsonException($"Invalid {typeof(T).Name} payload.");
}
