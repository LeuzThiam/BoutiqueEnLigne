using BoutiqueEnLigne.Identity.Api.Contracts.Requests;
using BoutiqueEnLigne.Identity.Api.Contracts.Responses;
using BoutiqueEnLigne.Identity.Application.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace BoutiqueEnLigne.Identity.Api.Controllers;

[ApiController]
[Route("api/users")]
public sealed class UsersController : ControllerBase
{
    private readonly IIdentityService _identity;

    public UsersController(IIdentityService identity)
    {
        _identity = identity;
    }

    [HttpGet("{id:int}")]
    [Authorize]
    public async Task<ActionResult<UserResponse>> GetUserById(int id, CancellationToken cancellationToken)
    {
        var user = await _identity.GetByIdAsync(id, cancellationToken);
        return user is null ? NotFound() : Ok(UserResponse.FromUser(user));
    }

    [HttpGet("email/{email}")]
    [Authorize(Roles = "Administrator")]
    public async Task<ActionResult<UserResponse>> GetUserByEmail(string email, CancellationToken cancellationToken)
    {
        var user = await _identity.GetByEmailAsync(email, cancellationToken);
        return user is null ? NotFound() : Ok(UserResponse.FromUser(user));
    }

    [HttpPost("register")]
    public async Task<ActionResult<UserResponse>> Register(
        [FromBody] RegisterRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _identity.RegisterAsync(
            request.Prenom,
            request.Nom,
            request.Email,
            request.MotDePasse,
            request.Role,
            cancellationToken);

        if (result.EmailAlreadyExists)
        {
            return Conflict(new { message = "Cet email est deja utilise." });
        }

        var response = UserResponse.FromUser(result.User!);
        return CreatedAtAction(nameof(GetUserById), new { id = response.Id }, response);
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthenticationResponse>> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _identity.AuthenticateAsync(request.Email, request.MotDePasse, cancellationToken);
        return result is null ? Unauthorized() : Ok(AuthenticationResponse.FromResult(result));
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<AuthenticationResponse>> Refresh(
        [FromBody] RefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _identity.RefreshAsync(request.RefreshToken, cancellationToken);
        return result is null ? Unauthorized() : Ok(AuthenticationResponse.FromResult(result));
    }

    [HttpPost("revoke")]
    public async Task<IActionResult> Revoke(
        [FromBody] RefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        await _identity.RevokeAsync(request.RefreshToken, cancellationToken);
        return NoContent();
    }

    [HttpGet("sellers/{id:int}")]
    [Authorize]
    public async Task<ActionResult<SellerResponse>> GetSellerById(int id, CancellationToken cancellationToken)
    {
        var seller = await _identity.GetSellerAsync(id, cancellationToken);
        return seller is null ? NotFound() : Ok(SellerResponse.FromUser(seller));
    }
}
