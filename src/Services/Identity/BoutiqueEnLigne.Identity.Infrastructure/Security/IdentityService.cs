using BoutiqueEnLigne.Identity.Application.Abstractions;
using BoutiqueEnLigne.Identity.Domain.Entities;
using BoutiqueEnLigne.Identity.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BoutiqueEnLigne.Identity.Infrastructure.Security;

public sealed class IdentityService : IIdentityService
{
    private readonly IdentityDbContext _dbContext;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly ITokenService _tokenService;

    public IdentityService(
        IdentityDbContext dbContext,
        IPasswordHasher<User> passwordHasher,
        ITokenService tokenService)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
    }

    public Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        _dbContext.Users.AsNoTracking().FirstOrDefaultAsync(user => user.Id == id, cancellationToken);

    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = NormalizeEmail(email);
        return _dbContext.Users.AsNoTracking()
            .FirstOrDefaultAsync(user => user.Email == normalizedEmail, cancellationToken);
    }

    public Task<User?> GetSellerAsync(int id, CancellationToken cancellationToken = default) =>
        _dbContext.Users.AsNoTracking().FirstOrDefaultAsync(
            user => user.Id == id && user.Role == "Vendeur",
            cancellationToken);

    public async Task<RegistrationResult> RegisterAsync(
        string firstName,
        string lastName,
        string email,
        string password,
        string role,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = NormalizeEmail(email);
        if (await _dbContext.Users.AnyAsync(user => user.Email == normalizedEmail, cancellationToken))
        {
            return new RegistrationResult(null, true);
        }

        var user = new User
        {
            Prenom = firstName.Trim(),
            Nom = lastName.Trim(),
            Email = normalizedEmail,
            Role = string.Equals(role, "Vendeur", StringComparison.OrdinalIgnoreCase) ? "Vendeur" : "Client"
        };
        user.PasswordHash = _passwordHasher.HashPassword(user, password);

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return new RegistrationResult(user, false);
    }

    public async Task<AuthenticationResult?> AuthenticateAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(
            item => item.Email == NormalizeEmail(email),
            cancellationToken);
        if (user is null)
        {
            return null;
        }

        var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
        if (result == PasswordVerificationResult.Failed)
        {
            return null;
        }

        if (result == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.PasswordHash = _passwordHasher.HashPassword(user, password);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        return await IssueTokensAsync(user, cancellationToken);
    }

    public async Task<AuthenticationResult?> RefreshAsync(
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        var tokenHash = _tokenService.HashRefreshToken(refreshToken);
        var storedToken = await _dbContext.RefreshTokens
            .Include(token => token.User)
            .FirstOrDefaultAsync(token => token.TokenHash == tokenHash, cancellationToken);
        if (storedToken is null || !storedToken.IsActive(DateTime.UtcNow))
        {
            return null;
        }

        storedToken.RevokedAtUtc = DateTime.UtcNow;
        return await IssueTokensAsync(storedToken.User, cancellationToken);
    }

    public async Task RevokeAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var tokenHash = _tokenService.HashRefreshToken(refreshToken);
        var storedToken = await _dbContext.RefreshTokens
            .FirstOrDefaultAsync(token => token.TokenHash == tokenHash, cancellationToken);
        if (storedToken is null || storedToken.RevokedAtUtc is not null)
        {
            return;
        }

        storedToken.RevokedAtUtc = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<AuthenticationResult> IssueTokensAsync(User user, CancellationToken cancellationToken)
    {
        var tokens = _tokenService.Issue(user);
        _dbContext.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            TokenHash = tokens.RefreshTokenHash,
            CreatedAtUtc = DateTime.UtcNow,
            ExpiresAtUtc = tokens.RefreshTokenExpiresAtUtc
        });
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new AuthenticationResult(
            user,
            tokens.AccessToken,
            tokens.RefreshToken,
            tokens.AccessTokenExpiresAtUtc,
            tokens.RefreshTokenExpiresAtUtc);
    }

    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();
}
