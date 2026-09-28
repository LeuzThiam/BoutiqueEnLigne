using BoutiqueEnLigne.Web.Models;

namespace BoutiqueEnLigne.Web.Services;

public sealed record UserAuthenticationResult(
    Utilisateur User,
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiresAtUtc,
    DateTime RefreshTokenExpiresAtUtc);
