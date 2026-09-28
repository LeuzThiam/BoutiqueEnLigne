using BoutiqueEnLigne.Payments.Application.Abstractions;
using BoutiqueEnLigne.Payments.Domain.Entities;
using BoutiqueEnLigne.Payments.Domain.Enums;
using BoutiqueEnLigne.Payments.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Stripe;
using BoutiqueEnLigne.Contracts.Payments;
using System.Text.Json;

namespace BoutiqueEnLigne.Payments.Infrastructure.Stripe;

public sealed class StripePaymentService : IPaymentService
{
    private readonly StripeOptions _options;
    private readonly PaymentsDbContext _dbContext;
    private readonly StripeClient _stripeClient;

    public StripePaymentService(StripeOptions options, PaymentsDbContext dbContext)
    {
        _options = options;
        _dbContext = dbContext;
        _stripeClient = new StripeClient(options.SecretKey);
    }

    public string GetPublishableKey() => _options.PublishableKey;
    public bool IsConfigured() => _options.IsConfigured();

    public async Task<PaymentIntentResult> CreatePaymentIntentAsync(
        int userId,
        int orderId,
        long amount,
        CancellationToken cancellationToken = default)
    {
        if (amount <= 0) throw new ArgumentException("Le montant doit etre superieur a 0.", nameof(amount));
        if (!IsConfigured()) throw new InvalidOperationException("Stripe n'est pas configure.");

        var service = new PaymentIntentService(_stripeClient);
        var intent = await service.CreateAsync(new PaymentIntentCreateOptions
        {
            Amount = amount,
            Currency = "cad",
            AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions { Enabled = true },
            Metadata = new Dictionary<string, string>
            {
                ["userId"] = userId.ToString(),
                ["orderId"] = orderId.ToString()
            }
        }, cancellationToken: cancellationToken);

        var payment = new Payment
        {
            UserId = userId,
            OrderId = orderId,
            StripePaymentIntentId = intent.Id,
            Amount = intent.Amount,
            Currency = intent.Currency ?? "cad"
        };
        _dbContext.Payments.Add(payment);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new PaymentIntentResult(payment.Id.ToString(), intent.ClientSecret ?? string.Empty, payment.Amount, payment.Currency);
    }

    public async Task HandleWebhookAsync(
        string payload,
        string signature,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.WebhookSecret) || _options.WebhookSecret.StartsWith("CHANGE_ME_"))
        {
            throw new InvalidOperationException("Stripe webhook secret is not configured.");
        }

        var stripeEvent = EventUtility.ConstructEvent(payload, signature, _options.WebhookSecret);
        if (await _dbContext.ProcessedWebhooks.AnyAsync(item => item.StripeEventId == stripeEvent.Id, cancellationToken))
        {
            return;
        }

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        if (stripeEvent.Data.Object is PaymentIntent intent)
        {
            var payment = await _dbContext.Payments.FirstOrDefaultAsync(
                item => item.StripePaymentIntentId == intent.Id,
                cancellationToken);
            if (payment is not null)
            {
                var newStatus = stripeEvent.Type switch
                {
                    "payment_intent.succeeded" => PaymentStatus.Succeeded,
                    "payment_intent.payment_failed" => PaymentStatus.Failed,
                    _ => payment.Status
                };
                if (newStatus != payment.Status)
                {
                    var occurredAt = DateTime.UtcNow;
                    if (newStatus == PaymentStatus.Succeeded) payment.MarkSucceeded(occurredAt);
                    else if (newStatus == PaymentStatus.Failed) payment.MarkFailed(occurredAt);
                    AddPaymentEvent(payment, intent.LastPaymentError?.Message);
                }
            }
        }

        _dbContext.ProcessedWebhooks.Add(new ProcessedWebhook { StripeEventId = stripeEvent.Id });
        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    private void AddPaymentEvent(Payment payment, string? failureReason)
    {
        var eventId = Guid.NewGuid();
        var occurredAt = DateTime.UtcNow;
        object integrationEvent;
        string eventName;

        if (payment.Status == PaymentStatus.Succeeded)
        {
            integrationEvent = new PaymentSucceededIntegrationEvent(
                eventId, occurredAt, payment.Id, payment.OrderId, payment.UserId, payment.Amount, payment.Currency);
            eventName = PaymentSucceededIntegrationEvent.EventName;
        }
        else if (payment.Status == PaymentStatus.Failed)
        {
            integrationEvent = new PaymentFailedIntegrationEvent(
                eventId, occurredAt, payment.Id, payment.OrderId, payment.UserId, failureReason);
            eventName = PaymentFailedIntegrationEvent.EventName;
        }
        else
        {
            return;
        }

        _dbContext.OutboxMessages.Add(new OutboxMessage
        {
            Id = eventId,
            Type = eventName,
            Payload = JsonSerializer.Serialize(integrationEvent, integrationEvent.GetType()),
            OccurredAtUtc = occurredAt
        });
    }
}
