using BoutiqueEnLigne.Catalog.Application.Abstractions;
using BoutiqueEnLigne.Catalog.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace BoutiqueEnLigne.Catalog.Api.Controllers;

[ApiController]
[Route("api/products")]
public sealed class ProductsController : ControllerBase
{
    private readonly IProductRepository _products;

    public ProductsController(IProductRepository products)
    {
        _products = products;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<Product>>> GetProducts(
        [FromQuery] string? search,
        CancellationToken cancellationToken)
    {
        return Ok(await _products.GetAllAsync(search, cancellationToken));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Product>> GetProductById(int id, CancellationToken cancellationToken)
    {
        var product = await _products.GetByIdAsync(id, cancellationToken);
        return product is null ? NotFound() : Ok(product);
    }

    [HttpPost]
    public async Task<ActionResult<Product>> AddProduct(
        [FromBody] Product product,
        CancellationToken cancellationToken)
    {
        product.DateAjout = product.DateAjout == default ? DateTime.UtcNow : product.DateAjout;
        await _products.AddAsync(product, cancellationToken);
        return CreatedAtAction(nameof(GetProductById), new { id = product.Id }, product);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateProduct(
        int id,
        [FromBody] Product product,
        CancellationToken cancellationToken)
    {
        return await _products.UpdateAsync(id, product, cancellationToken) ? NoContent() : NotFound();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteProduct(int id, CancellationToken cancellationToken)
    {
        return await _products.DeleteAsync(id, cancellationToken) ? NoContent() : NotFound();
    }
}
