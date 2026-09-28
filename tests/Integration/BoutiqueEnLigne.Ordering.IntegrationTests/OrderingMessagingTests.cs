using System.Text.Json;
using BoutiqueEnLigne.Contracts.Ordering;
using BoutiqueEnLigne.Contracts.Payments;
using BoutiqueEnLigne.Ordering.Domain.Entities;
using BoutiqueEnLigne.Ordering.Domain.Enums;
using BoutiqueEnLigne.Ordering.Infrastructure.Messaging;
using BoutiqueEnLigne.Ordering.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BoutiqueEnLigne.Ordering.IntegrationTests;

public sealed class OrderingMessagingTests
{
    [Fact]
    public async Task Creating_order_persists_order_and_outbox_message_atomically()
    {
        await using var fixture = await OrderingDatabaseFixture.CreateAsync();
        var repository = new OrderRepository(fixture.DbContext);
        var order = CreateOrder();

        await repository.AddAsync(order);

        var storedOrder = await fixture.DbContext.Orders.SingleAsync();
        var outbox = await fixture.DbContext.OutboxMessages.SingleAsync();
        var integrationEvent = JsonSerializer.Deserialize<OrderCreatedIntegrationEvent>(outbox.Payload);

        Assert.Equal(storedOrder.Id, integrationEvent?.OrderId);
        Assert.Equal(OrderCreatedIntegrationEvent.EventName, outbox.Type);
        Assert.Null(outbox.ProcessedAtUtc);
    }

    [Fact]
    public async Task Payment_success_is_applied_once_when_event_is_delivered_twice()
    {
        await using var fixture = await OrderingDatabaseFixture.CreateAsync();
        var order = CreateOrder();
        fixture.DbContext.Orders.Add(order);
        await fixture.DbContext.SaveChangesAsync();
        var integrationEvent = new PaymentSucceededIntegrationEvent(
            Guid.NewGuid(), DateTime.UtcNow, Guid.NewGuid(), order.Id, order.UtilisateurId, 2500, "cad");
        var handler = new PaymentEventHandler(fixture.DbContext);

        await handler.HandleAsync(integrationEvent);
        await handler.HandleAsync(integrationEvent);

        Assert.True(order.EstPayee);
        Assert.Equal(OrderStatus.Confirmed, order.Statut);
        Assert.Equal(1, await fixture.DbContext.InboxMessages.CountAsync());
    }

    [Fact]
    public async Task Payment_failure_keeps_order_pending_and_records_inbox_message()
    {
        await using var fixture = await OrderingDatabaseFixture.CreateAsync();
        var order = CreateOrder();
        fixture.DbContext.Orders.Add(order);
        await fixture.DbContext.SaveChangesAsync();
        var integrationEvent = new PaymentFailedIntegrationEvent(
            Guid.NewGuid(), DateTime.UtcNow, Guid.NewGuid(), order.Id, order.UtilisateurId, "card_declined");

        await new PaymentEventHandler(fixture.DbContext).HandleAsync(integrationEvent);

        Assert.False(order.EstPayee);
        Assert.Equal(OrderStatus.Pending, order.Statut);
        Assert.Equal(integrationEvent.EventId, (await fixture.DbContext.InboxMessages.SingleAsync()).Id);
    }

    [Fact]
    public async Task Payment_event_for_another_user_is_rejected_without_inbox_receipt()
    {
        await using var fixture = await OrderingDatabaseFixture.CreateAsync();
        var order = CreateOrder();
        fixture.DbContext.Orders.Add(order);
        await fixture.DbContext.SaveChangesAsync();
        var integrationEvent = new PaymentSucceededIntegrationEvent(
            Guid.NewGuid(), DateTime.UtcNow, Guid.NewGuid(), order.Id, order.UtilisateurId + 1, 2500, "cad");

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            new PaymentEventHandler(fixture.DbContext).HandleAsync(integrationEvent));

        Assert.Empty(await fixture.DbContext.InboxMessages.ToListAsync());
    }

    private static Order CreateOrder() => new()
    {
        UtilisateurId = 42,
        Total = 25m,
        ArticlesCommandes =
        [
            new OrderItem
            {
                ProduitId = 7,
                NomProduit = "Test product",
                Quantite = 1,
                PrixUnitaire = 25m
            }
        ]
    };

    private sealed class OrderingDatabaseFixture : IAsyncDisposable
    {
        private readonly SqliteConnection _connection;
        public OrderingDbContext DbContext { get; }

        private OrderingDatabaseFixture(SqliteConnection connection, OrderingDbContext dbContext)
        {
            _connection = connection;
            DbContext = dbContext;
        }

        public static async Task<OrderingDatabaseFixture> CreateAsync()
        {
            var connection = new SqliteConnection("Data Source=:memory:");
            await connection.OpenAsync();
            var options = new DbContextOptionsBuilder<OrderingDbContext>()
                .UseSqlite(connection)
                .Options;
            var dbContext = new OrderingDbContext(options);
            await dbContext.Database.EnsureCreatedAsync();
            return new OrderingDatabaseFixture(connection, dbContext);
        }

        public async ValueTask DisposeAsync()
        {
            await DbContext.DisposeAsync();
            await _connection.DisposeAsync();
        }
    }
}
