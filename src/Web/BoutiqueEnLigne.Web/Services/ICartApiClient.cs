using BoutiqueEnLigne.Web.Services.ApiModels;

namespace BoutiqueEnLigne.Web.Services
{
    public interface ICartApiClient
    {
        Task<CartApiModel?> GetCartAsync(int userId);
        Task<CartApiModel?> AddItemAsync(int userId, UpsertCartItemApiModel item);
        Task<bool> RemoveItemAsync(int userId, int productId);
        Task<bool> ClearCartAsync(int userId);
    }
}
