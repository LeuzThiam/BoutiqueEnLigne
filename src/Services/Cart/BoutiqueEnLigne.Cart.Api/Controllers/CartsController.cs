using System.Security.Claims;
using BoutiqueEnLigne.Cart.Api.Contracts.Requests;
using BoutiqueEnLigne.Cart.Application.Abstractions;
using BoutiqueEnLigne.Cart.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BoutiqueEnLigne.Cart.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/carts")]
public sealed class CartsController : ControllerBase
{
    private readonly ICartRepository _carts;

    public CartsController(ICartRepository carts)
    {
        _carts = carts;
    }

    [HttpGet("{userId:int}")]
    public async Task<ActionResult<ShoppingCart>> GetCart(int userId, CancellationToken cancellationToken)
    {
        if (!CanAccess(userId)) return Forbid();
        return Ok(await _carts.GetOrCreateAsync(userId, cancellationToken));
    }

    [HttpPost("{userId:int}/items")]
    public async Task<ActionResult<ShoppingCart>> AddItem(
        int userId,
        [FromBody] UpsertCartItemRequest request,
        CancellationToken cancellationToken)
    {
        if (!CanAccess(userId)) return Forbid();
        if (request.ProduitId <= 0 || request.Quantite <= 0)
        {
            return BadRequest(new { error = "Le produit et la quantite doivent etre valides." });
        }

        var input = new CartItemInput(
            request.ProduitId,
            request.Quantite,
            request.NomProduit,
            request.PrixUnitaire,
            request.UrlImage);
        return Ok(await _carts.AddItemAsync(userId, input, cancellationToken));
    }

    [HttpPut("{userId:int}/items/{productId:int}")]
    public async Task<ActionResult<ShoppingCart>> UpdateItemQuantity(
        int userId,
        int productId,
        [FromBody] UpsertCartItemRequest request,
        CancellationToken cancellationToken)
    {
        if (!CanAccess(userId)) return Forbid();
        if (request.Quantite <= 0)
        {
            return BadRequest(new { error = "La quantite doit etre superieure a 0." });
        }

        var cart = await _carts.UpdateItemQuantityAsync(userId, productId, request.Quantite, cancellationToken);
        return cart is null ? NotFound() : Ok(cart);
    }

    [HttpDelete("{userId:int}/items/{productId:int}")]
    public async Task<IActionResult> RemoveItem(int userId, int productId, CancellationToken cancellationToken)
    {
        if (!CanAccess(userId)) return Forbid();
        return await _carts.RemoveItemAsync(userId, productId, cancellationToken) ? NoContent() : NotFound();
    }

    [HttpDelete("{userId:int}")]
    public async Task<IActionResult> ClearCart(int userId, CancellationToken cancellationToken)
    {
        if (!CanAccess(userId)) return Forbid();
        return await _carts.ClearAsync(userId, cancellationToken) ? NoContent() : NotFound();
    }

    private bool CanAccess(int userId)
    {
        return User.IsInRole("Administrator") ||
               User.FindFirstValue(ClaimTypes.NameIdentifier) == userId.ToString();
    }
}
