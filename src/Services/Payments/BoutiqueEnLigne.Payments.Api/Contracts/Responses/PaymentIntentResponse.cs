using BoutiqueEnLigne.Payments.Application.Abstractions;

namespace BoutiqueEnLigne.Payments.Api.Contracts.Responses;

public sealed record PaymentIntentResponse(string PaymentId, string ClientSecret, long Amount, string Currency)
{
    public static PaymentIntentResponse FromResult(PaymentIntentResult result) =>
        new(result.PaymentId, result.ClientSecret, result.Amount, result.Currency);
}
