using BoutiqueEnLigne.Ordering.Domain.Entities;

namespace BoutiqueEnLigne.Ordering.Application.Abstractions;

public interface IOrderRepository
{
    Task<IReadOnlyList<Order>> GetByUserAsync(int userId, CancellationToken cancellationToken = default);
    Task<Order?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Order> AddAsync(Order order, CancellationToken cancellationToken = default);
}
