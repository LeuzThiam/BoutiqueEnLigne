using Microsoft.EntityFrameworkCore;
using BoutiqueEnLigne.Catalog.Domain.Entities;

namespace BoutiqueEnLigne.Catalog.Infrastructure.Persistence
{
    public class CatalogDbContext : DbContext
    {
        public CatalogDbContext(DbContextOptions<CatalogDbContext> options)
            : base(options)
        {
        }

        public DbSet<Product> Produits => Set<Product>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Product>()
                .Property(p => p.Nom)
                .HasMaxLength(200);

            modelBuilder.Entity<Product>()
                .Property(p => p.Prix)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Product>()
                .Property(p => p.CategorieNom)
                .HasMaxLength(100);
        }
    }
}
