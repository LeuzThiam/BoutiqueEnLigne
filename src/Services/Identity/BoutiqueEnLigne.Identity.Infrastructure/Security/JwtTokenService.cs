using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using BoutiqueEnLigne.Identity.Application.Abstractions;
using BoutiqueEnLigne.Identity.Domain.Entities;
using Microsoft.IdentityModel.Tokens;

namespace BoutiqueEnLigne.Identity.Infrastructure.Security;

public sealed class JwtTokenService : ITokenService
{
    private readonly JwtOptions _options;

    public JwtTokenService(JwtOptions options)
    {
        _options = options;
    }

    public IssuedTokens Issue(User user)
    {
        var now = DateTime.UtcNow;
        var accessExpiresAt = now.AddMinutes(_options.AccessTokenMinutes);
        var refreshExpiresAt = now.AddDays(_options.RefreshTokenDays);
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SecretKey));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, $"{user.Prenom} {user.Nom}".Trim()),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N"))
        };
        var token = new JwtSecurityToken(
            _options.Issuer,
            _options.Audience,
            claims,
            now,
            accessExpiresAt,
            credentials);
        var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

        return new IssuedTokens(
            new JwtSecurityTokenHandler().WriteToken(token),
            refreshToken,
            HashRefreshToken(refreshToken),
            accessExpiresAt,
            refreshExpiresAt);
    }

    public string HashRefreshToken(string refreshToken)
    {
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken)));
    }
}
