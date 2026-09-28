namespace BoutiqueEnLigne.Web.Services.ApiModels;

public sealed class AuthenticationApiModel : UserApiModel
{
    public string AccessToken { get; init; } = string.Empty;
    public string RefreshToken { get; init; } = string.Empty;
    public DateTime AccessTokenExpiresAtUtc { get; init; }
    public DateTime RefreshTokenExpiresAtUtc { get; init; }
}
