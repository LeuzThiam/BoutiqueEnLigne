using BoutiqueEnLigne.Cart.Domain.Entities;

namespace BoutiqueEnLigne.Cart.Application.Abstractions;

public interface ICartRepository
{
    Task<ShoppingCart> GetOrCreateAsync(int userId, CancellationToken cancellationToken = default);
    Task<ShoppingCart> AddItemAsync(int userId, CartItemInput item, CancellationToken cancellationToken = default);
    Task<ShoppingCart?> UpdateItemQuantityAsync(int userId, int productId, int quantity, CancellationToken cancellationToken = default);
    Task<bool> RemoveItemAsync(int userId, int productId, CancellationToken cancellationToken = default);
    Task<bool> ClearAsync(int userId, CancellationToken cancellationToken = default);
}

public sealed record CartItemInput(
    int ProductId,
    int Quantity,
    string ProductName,
    decimal UnitPrice,
    string ImageUrl);
