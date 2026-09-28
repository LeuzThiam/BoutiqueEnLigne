using System.Security.Claims;
using BoutiqueEnLigne.Ordering.Api.Contracts.Requests;
using BoutiqueEnLigne.Ordering.Application.Abstractions;
using BoutiqueEnLigne.Ordering.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BoutiqueEnLigne.Ordering.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/orders")]
public sealed class OrdersController : ControllerBase
{
    private readonly IOrderRepository _orders;
    public OrdersController(IOrderRepository orders) => _orders = orders;

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<Order>>> GetOrders(
        [FromQuery] int userId,
        CancellationToken cancellationToken)
    {
        if (!CanAccess(userId)) return Forbid();
        return Ok(await _orders.GetByUserAsync(userId, cancellationToken));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Order>> GetOrderById(int id, CancellationToken cancellationToken)
    {
        var order = await _orders.GetByIdAsync(id, cancellationToken);
        if (order is null) return NotFound();
        return CanAccess(order.UtilisateurId) ? Ok(order) : Forbid();
    }

    [HttpPost]
    public async Task<ActionResult<Order>> CreateOrder(
        [FromBody] CreateOrderRequest request,
        CancellationToken cancellationToken)
    {
        var authenticatedUserId = GetAuthenticatedUserId();
        if (authenticatedUserId is null) return Unauthorized();

        var order = new Order
        {
            UtilisateurId = authenticatedUserId.Value,
            DateCommande = DateTime.UtcNow,
            ArticlesCommandes = request.ArticlesCommandes.Select(item => new OrderItem
            {
                ProduitId = item.ProduitId,
                NomProduit = item.NomProduit,
                Quantite = item.Quantite,
                PrixUnitaire = item.PrixUnitaire
            }).ToList()
        };
        order.Total = order.ArticlesCommandes.Sum(item => item.PrixUnitaire * item.Quantite);
        await _orders.AddAsync(order, cancellationToken);
        return CreatedAtAction(nameof(GetOrderById), new { id = order.Id }, order);
    }

    private bool CanAccess(int userId) =>
        User.IsInRole("Administrator") || GetAuthenticatedUserId() == userId;

    private int? GetAuthenticatedUserId() =>
        int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;
}
