using BoutiqueEnLigne.Payments.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BoutiqueEnLigne.Payments.Infrastructure.Persistence;

public sealed class PaymentsDbContext : DbContext
{
    public PaymentsDbContext(DbContextOptions<PaymentsDbContext> options) : base(options) { }

    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<ProcessedWebhook> ProcessedWebhooks => Set<ProcessedWebhook>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(payment => payment.Id);
            entity.HasIndex(payment => payment.StripePaymentIntentId).IsUnique();
            entity.Property(payment => payment.StripePaymentIntentId).HasMaxLength(100).IsRequired();
            entity.Property(payment => payment.Currency).HasMaxLength(3).IsRequired();
            entity.Property(payment => payment.Status).HasConversion<string>().HasMaxLength(30);
        });
        modelBuilder.Entity<ProcessedWebhook>(entity =>
        {
            entity.HasKey(webhook => webhook.StripeEventId);
            entity.Property(webhook => webhook.StripeEventId).HasMaxLength(100);
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
    }
}
