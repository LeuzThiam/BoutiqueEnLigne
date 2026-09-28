using System.Net.Http.Json;
using BoutiqueEnLigne.Web.Services.ApiModels;

namespace BoutiqueEnLigne.Web.Services
{
    public class OrderApiClient : IOrderApiClient
    {
        private readonly HttpClient _httpClient;

        public OrderApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IReadOnlyList<OrderApiModel>> GetOrdersAsync(int userId)
        {
            var orders = await _httpClient.GetFromJsonAsync<List<OrderApiModel>>($"api/orders?userId={userId}");
            return orders ?? new List<OrderApiModel>();
        }

        public Task<OrderApiModel?> GetOrderAsync(int id)
        {
            return _httpClient.GetFromJsonAsync<OrderApiModel>($"api/orders/{id}");
        }

        public async Task<OrderApiModel?> CreateOrderAsync(OrderApiModel order)
        {
            var response = await _httpClient.PostAsJsonAsync("api/orders", order);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadFromJsonAsync<OrderApiModel>();
        }
    }
}
