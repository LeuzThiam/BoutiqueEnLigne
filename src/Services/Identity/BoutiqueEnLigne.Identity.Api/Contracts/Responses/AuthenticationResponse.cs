using BoutiqueEnLigne.Identity.Application.Abstractions;

namespace BoutiqueEnLigne.Identity.Api.Contracts.Responses;

public sealed record AuthenticationResponse(
    int Id,
    string Prenom,
    string Nom,
    string Email,
    string Role,
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiresAtUtc,
    DateTime RefreshTokenExpiresAtUtc)
{
    public static AuthenticationResponse FromResult(AuthenticationResult result) => new(
        result.User.Id,
        result.User.Prenom,
        result.User.Nom,
        result.User.Email,
        result.User.Role,
        result.AccessToken,
        result.RefreshToken,
        result.AccessTokenExpiresAtUtc,
        result.RefreshTokenExpiresAtUtc);
}
