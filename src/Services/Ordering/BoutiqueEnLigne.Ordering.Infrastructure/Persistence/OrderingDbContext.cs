using BoutiqueEnLigne.Ordering.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BoutiqueEnLigne.Ordering.Infrastructure.Persistence;

public sealed class OrderingDbContext : DbContext
{
    public OrderingDbContext(DbContextOptions<OrderingDbContext> options) : base(options) { }

    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(order => order.Id);
            entity.Property(order => order.Total).HasPrecision(18, 2);
            entity.Property(order => order.Statut).HasConversion<string>().HasMaxLength(30);
            entity.HasMany(order => order.ArticlesCommandes)
                .WithOne()
                .HasForeignKey(item => item.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(item => item.Id);
            entity.Property(item => item.NomProduit).HasMaxLength(200).IsRequired();
            entity.Property(item => item.PrixUnitaire).HasPrecision(18, 2);
        });
        modelBuilder.Entity<OutboxMessage>(entity =>
        {
            entity.ToTable("OutboxMessages");
            entity.HasKey(message => message.Id);
            entity.Property(message => message.Type).HasMaxLength(200).IsRequired();
            entity.Property(message => message.Payload).IsRequired();
            entity.Property(message => message.LastError).HasMaxLength(2000);
            entity.HasIndex(message => new { message.ProcessedAtUtc, message.OccurredAtUtc });
        });
        modelBuilder.Entity<InboxMessage>(entity =>
        {
            entity.ToTable("InboxMessages");
            entity.HasKey(message => message.Id);
            entity.Property(message => message.Type).HasMaxLength(200).IsRequired();
            entity.HasIndex(message => message.ProcessedAtUtc);
        });
    }
}
