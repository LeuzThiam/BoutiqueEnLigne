using System.ComponentModel.DataAnnotations;

namespace BoutiqueEnLigne.Identity.Api.Contracts.Requests;

public sealed class RefreshTokenRequest
{
    [Required]
    public string RefreshToken { get; init; } = string.Empty;
}
