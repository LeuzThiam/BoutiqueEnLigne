using System.Text.Json;
using BoutiqueEnLigne.Cart.Application.Abstractions;
using BoutiqueEnLigne.Cart.Domain.Entities;
using Microsoft.Extensions.Caching.Distributed;

namespace BoutiqueEnLigne.Cart.Infrastructure.Persistence;

public sealed class RedisCartRepository : ICartRepository
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private static readonly DistributedCacheEntryOptions CacheOptions = new()
    {
        SlidingExpiration = TimeSpan.FromDays(7)
    };

    private readonly IDistributedCache _cache;

    public RedisCartRepository(IDistributedCache cache)
    {
        _cache = cache;
    }

    public async Task<ShoppingCart> GetOrCreateAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await GetAsync(userId, cancellationToken) ?? new ShoppingCart { UtilisateurId = userId };
    }

    public async Task<ShoppingCart> AddItemAsync(
        int userId,
        CartItemInput input,
        CancellationToken cancellationToken = default)
    {
        var cart = await GetOrCreateAsync(userId, cancellationToken);
        var item = cart.Articles.FirstOrDefault(existing => existing.ProduitId == input.ProductId);
        if (item is null)
        {
            cart.Articles.Add(new CartItem
            {
                ProduitId = input.ProductId,
                Quantite = input.Quantity,
                NomProduit = input.ProductName,
                PrixUnitaire = input.UnitPrice,
                UrlImage = input.ImageUrl
            });
        }
        else
        {
            item.Quantite += input.Quantity;
            item.NomProduit = string.IsNullOrWhiteSpace(input.ProductName) ? item.NomProduit : input.ProductName;
            item.PrixUnitaire = input.UnitPrice > 0 ? input.UnitPrice : item.PrixUnitaire;
            item.UrlImage = string.IsNullOrWhiteSpace(input.ImageUrl) ? item.UrlImage : input.ImageUrl;
        }

        await SaveAsync(cart, cancellationToken);
        return cart;
    }

    public async Task<ShoppingCart?> UpdateItemQuantityAsync(
        int userId,
        int productId,
        int quantity,
        CancellationToken cancellationToken = default)
    {
        var cart = await GetAsync(userId, cancellationToken);
        var item = cart?.Articles.FirstOrDefault(existing => existing.ProduitId == productId);
        if (cart is null || item is null)
        {
            return null;
        }

        item.Quantite = quantity;
        await SaveAsync(cart, cancellationToken);
        return cart;
    }

    public async Task<bool> RemoveItemAsync(int userId, int productId, CancellationToken cancellationToken = default)
    {
        var cart = await GetAsync(userId, cancellationToken);
        var item = cart?.Articles.FirstOrDefault(existing => existing.ProduitId == productId);
        if (cart is null || item is null)
        {
            return false;
        }

        cart.Articles.Remove(item);
        await SaveAsync(cart, cancellationToken);
        return true;
    }

    public async Task<bool> ClearAsync(int userId, CancellationToken cancellationToken = default)
    {
        var cart = await GetAsync(userId, cancellationToken);
        if (cart is null)
        {
            return false;
        }

        await _cache.RemoveAsync(GetKey(userId), cancellationToken);
        return true;
    }

    private async Task<ShoppingCart?> GetAsync(int userId, CancellationToken cancellationToken)
    {
        var json = await _cache.GetStringAsync(GetKey(userId), cancellationToken);
        return json is null ? null : JsonSerializer.Deserialize<ShoppingCart>(json, JsonOptions);
    }

    private async Task SaveAsync(ShoppingCart cart, CancellationToken cancellationToken)
    {
        cart.DerniereMiseAJourUtc = DateTime.UtcNow;
        var json = JsonSerializer.Serialize(cart, JsonOptions);
        await _cache.SetStringAsync(GetKey(cart.UtilisateurId), json, CacheOptions, cancellationToken);
    }

    private static string GetKey(int userId) => $"cart:{userId}";
}
