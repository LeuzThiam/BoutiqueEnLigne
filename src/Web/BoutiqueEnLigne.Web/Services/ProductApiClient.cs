using System.Net.Http.Json;
using BoutiqueEnLigne.Web.Models;

namespace BoutiqueEnLigne.Web.Services
{
    public class ProductApiClient : IProductApiClient
    {
        private readonly HttpClient _httpClient;

        public ProductApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IReadOnlyList<Produit>> GetProduitsAsync(string? search = null)
        {
            var url = "api/products";
            if (!string.IsNullOrWhiteSpace(search))
            {
                url += $"?search={Uri.EscapeDataString(search)}";
            }

            var produits = await _httpClient.GetFromJsonAsync<List<Produit>>(url);
            return produits ?? new List<Produit>();
        }

        public Task<Produit?> GetProduitAsync(int id)
        {
            return _httpClient.GetFromJsonAsync<Produit>($"api/products/{id}");
        }

        public async Task<Produit?> CreateProduitAsync(Produit produit)
        {
            var response = await _httpClient.PostAsJsonAsync("api/products", produit);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadFromJsonAsync<Produit>();
        }

        public async Task<bool> UpdateProduitAsync(Produit produit)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/products/{produit.Id}", produit);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteProduitAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"api/products/{id}");
            return response.IsSuccessStatusCode;
        }
    }
}
