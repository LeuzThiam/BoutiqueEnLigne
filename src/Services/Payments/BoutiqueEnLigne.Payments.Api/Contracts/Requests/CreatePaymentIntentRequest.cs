using System.ComponentModel.DataAnnotations;

namespace BoutiqueEnLigne.Payments.Api.Contracts.Requests;

public sealed class CreatePaymentIntentRequest
{
    [Range(1, int.MaxValue)]
    public int OrderId { get; init; }

    [Range(1, long.MaxValue)]
    public long Amount { get; init; }
}
