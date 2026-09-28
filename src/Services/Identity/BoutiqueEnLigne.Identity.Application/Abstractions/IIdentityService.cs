using BoutiqueEnLigne.Identity.Domain.Entities;

namespace BoutiqueEnLigne.Identity.Application.Abstractions;

public interface IIdentityService
{
    Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<User?> GetSellerAsync(int id, CancellationToken cancellationToken = default);
    Task<RegistrationResult> RegisterAsync(
        string firstName,
        string lastName,
        string email,
        string password,
        string role,
        CancellationToken cancellationToken = default);
    Task<AuthenticationResult?> AuthenticateAsync(string email, string password, CancellationToken cancellationToken = default);
    Task<AuthenticationResult?> RefreshAsync(string refreshToken, CancellationToken cancellationToken = default);
    Task RevokeAsync(string refreshToken, CancellationToken cancellationToken = default);
}

public sealed record RegistrationResult(User? User, bool EmailAlreadyExists);
public sealed record AuthenticationResult(
    User User,
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiresAtUtc,
    DateTime RefreshTokenExpiresAtUtc);
