using BoutiqueEnLigne.Web.Services.ApiModels;

namespace BoutiqueEnLigne.Web.Services
{
    public interface IOrderApiClient
    {
        Task<IReadOnlyList<OrderApiModel>> GetOrdersAsync(int userId);
        Task<OrderApiModel?> GetOrderAsync(int id);
        Task<OrderApiModel?> CreateOrderAsync(OrderApiModel order);
    }
}
