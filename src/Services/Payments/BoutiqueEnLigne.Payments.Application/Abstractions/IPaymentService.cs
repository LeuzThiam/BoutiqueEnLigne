namespace BoutiqueEnLigne.Payments.Application.Abstractions;

public interface IPaymentService
{
    string GetPublishableKey();
    bool IsConfigured();
    Task<PaymentIntentResult> CreatePaymentIntentAsync(int userId, int orderId, long amount, CancellationToken cancellationToken = default);
    Task HandleWebhookAsync(string payload, string signature, CancellationToken cancellationToken = default);
}

public sealed record PaymentIntentResult(
    string PaymentId,
    string ClientSecret,
    long Amount,
    string Currency);
