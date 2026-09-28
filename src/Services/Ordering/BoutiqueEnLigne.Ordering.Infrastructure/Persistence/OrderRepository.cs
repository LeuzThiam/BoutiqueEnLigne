using BoutiqueEnLigne.Ordering.Application.Abstractions;
using BoutiqueEnLigne.Ordering.Domain.Entities;
using BoutiqueEnLigne.Contracts.Ordering;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace BoutiqueEnLigne.Ordering.Infrastructure.Persistence;

public sealed class OrderRepository : IOrderRepository
{
    private readonly OrderingDbContext _dbContext;
    public OrderRepository(OrderingDbContext dbContext) => _dbContext = dbContext;

    public async Task<IReadOnlyList<Order>> GetByUserAsync(int userId, CancellationToken cancellationToken = default) =>
        await _dbContext.Orders.AsNoTracking().Include(order => order.ArticlesCommandes)
            .Where(order => order.UtilisateurId == userId)
            .OrderByDescending(order => order.DateCommande)
            .ToListAsync(cancellationToken);

    public Task<Order?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        _dbContext.Orders.AsNoTracking().Include(order => order.ArticlesCommandes)
            .FirstOrDefaultAsync(order => order.Id == id, cancellationToken);

    public async Task<Order> AddAsync(Order order, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            _dbContext.Orders.Add(order);
            await _dbContext.SaveChangesAsync(cancellationToken);

            var integrationEvent = new OrderCreatedIntegrationEvent(
                Guid.NewGuid(),
                DateTime.UtcNow,
                order.Id,
                order.UtilisateurId,
                order.Total,
                order.ArticlesCommandes.Select(item => new OrderCreatedItem(
                    item.ProduitId,
                    item.NomProduit,
                    item.Quantite,
                    item.PrixUnitaire)).ToArray());

            _dbContext.OutboxMessages.Add(new OutboxMessage
            {
                Id = integrationEvent.EventId,
                Type = OrderCreatedIntegrationEvent.EventName,
                Payload = JsonSerializer.Serialize(integrationEvent),
                OccurredAtUtc = integrationEvent.OccurredAtUtc
            });
            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return order;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
