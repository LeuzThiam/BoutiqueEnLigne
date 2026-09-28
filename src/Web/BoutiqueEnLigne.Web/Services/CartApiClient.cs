using System.Net;
using System.Net.Http.Json;
using BoutiqueEnLigne.Web.Services.ApiModels;

namespace BoutiqueEnLigne.Web.Services
{
    public class CartApiClient : ICartApiClient
    {
        private readonly HttpClient _httpClient;

        public CartApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public Task<CartApiModel?> GetCartAsync(int userId)
        {
            return _httpClient.GetFromJsonAsync<CartApiModel>($"api/carts/{userId}");
        }

        public async Task<CartApiModel?> AddItemAsync(int userId, UpsertCartItemApiModel item)
        {
            var response = await _httpClient.PostAsJsonAsync($"api/carts/{userId}/items", item);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadFromJsonAsync<CartApiModel>();
        }

        public async Task<bool> RemoveItemAsync(int userId, int productId)
        {
            var response = await _httpClient.DeleteAsync($"api/carts/{userId}/items/{productId}");
            return response.StatusCode == HttpStatusCode.NoContent;
        }

        public async Task<bool> ClearCartAsync(int userId)
        {
            var response = await _httpClient.DeleteAsync($"api/carts/{userId}");
            return response.StatusCode == HttpStatusCode.NoContent;
        }
    }
}
