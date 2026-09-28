using System.ComponentModel.DataAnnotations;

namespace BoutiqueEnLigne.Identity.Api.Contracts.Requests;

public sealed class LoginRequest
{
    [Required, EmailAddress]
    public string Email { get; init; } = string.Empty;

    [Required]
    public string MotDePasse { get; init; } = string.Empty;
}
