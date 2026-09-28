using System.Security.Claims;
using BoutiqueEnLigne.Payments.Api.Contracts.Requests;
using BoutiqueEnLigne.Payments.Api.Contracts.Responses;
using BoutiqueEnLigne.Payments.Application.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;

namespace BoutiqueEnLigne.Payments.Api.Controllers;

[ApiController]
[Route("api/payments")]
public sealed class PaymentsController : ControllerBase
{
    private readonly IPaymentService _payments;
    public PaymentsController(IPaymentService payments) => _payments = payments;

    [AllowAnonymous]
    [HttpGet("public-key")]
    public IActionResult GetPublishableKey() => Ok(new
    {
        key = _payments.GetPublishableKey(),
        configured = _payments.IsConfigured()
    });

    [Authorize]
    [HttpPost("payment-intent")]
    public async Task<ActionResult<PaymentIntentResponse>> CreatePaymentIntent(
        [FromBody] CreatePaymentIntentRequest request,
        CancellationToken cancellationToken)
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId)) return Unauthorized();

        try
        {
            var result = await _payments.CreatePaymentIntentAsync(userId, request.OrderId, request.Amount, cancellationToken);
            return Ok(PaymentIntentResponse.FromResult(result));
        }
        catch (InvalidOperationException exception)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { error = exception.Message });
        }
    }

    [AllowAnonymous]
    [HttpPost("webhook")]
    public async Task<IActionResult> Webhook(CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(Request.Body);
        var payload = await reader.ReadToEndAsync(cancellationToken);
        var signature = Request.Headers["Stripe-Signature"].ToString();
        try
        {
            await _payments.HandleWebhookAsync(payload, signature, cancellationToken);
            return Ok();
        }
        catch (StripeException)
        {
            return BadRequest();
        }
    }
}
