using BoutiqueEnLigne.Identity.Domain.Entities;

namespace BoutiqueEnLigne.Identity.Application.Abstractions;

public interface ITokenService
{
    IssuedTokens Issue(User user);
    string HashRefreshToken(string refreshToken);
}

public sealed record IssuedTokens(
    string AccessToken,
    string RefreshToken,
    string RefreshTokenHash,
    DateTime AccessTokenExpiresAtUtc,
    DateTime RefreshTokenExpiresAtUtc);
