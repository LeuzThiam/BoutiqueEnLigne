using BoutiqueEnLigne.Web.Services.ApiModels;

namespace BoutiqueEnLigne.Web.Services;

public interface IPaymentApiClient
{
    Task<PaymentPublicKeyApiModel?> GetPublishableKeyAsync(CancellationToken cancellationToken = default);
    Task<PaymentIntentApiModel?> CreatePaymentIntentAsync(int orderId, long amount, CancellationToken cancellationToken = default);
}
