using BoutiqueEnLigne.Catalog.Application.Abstractions;
using BoutiqueEnLigne.Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BoutiqueEnLigne.Catalog.Infrastructure.Persistence;

public sealed class ProductRepository : IProductRepository
{
    private readonly CatalogDbContext _dbContext;

    public ProductRepository(CatalogDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<Product>> GetAllAsync(string? search, CancellationToken cancellationToken = default)
    {
        IQueryable<Product> query = _dbContext.Produits.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(product =>
                product.Nom.Contains(search) || product.CategorieNom.Contains(search));
        }

        return await query.OrderByDescending(product => product.DateAjout).ToListAsync(cancellationToken);
    }

    public Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return _dbContext.Produits.AsNoTracking()
            .FirstOrDefaultAsync(product => product.Id == id, cancellationToken);
    }

    public async Task<Product> AddAsync(Product product, CancellationToken cancellationToken = default)
    {
        _dbContext.Produits.Add(product);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return product;
    }

    public async Task<bool> UpdateAsync(int id, Product product, CancellationToken cancellationToken = default)
    {
        var existing = await _dbContext.Produits.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (existing is null)
        {
            return false;
        }

        existing.Nom = product.Nom;
        existing.Description = product.Description;
        existing.Prix = product.Prix;
        existing.Quantite = product.Quantite;
        existing.UrlImage = product.UrlImage;
        existing.CategorieNom = product.CategorieNom;
        existing.VendeurId = product.VendeurId;
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var product = await _dbContext.Produits.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (product is null)
        {
            return false;
        }

        _dbContext.Produits.Remove(product);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}
