using System.ComponentModel.DataAnnotations;

namespace BoutiqueEnLigne.Identity.Api.Contracts.Requests;

public sealed class RegisterRequest
{
    [Required, MaxLength(50)]
    public string Prenom { get; init; } = string.Empty;

    [Required, MaxLength(50)]
    public string Nom { get; init; } = string.Empty;

    [Required, EmailAddress, MaxLength(256)]
    public string Email { get; init; } = string.Empty;

    [Required, MinLength(8), MaxLength(100)]
    public string MotDePasse { get; init; } = string.Empty;

    [Required]
    public string Role { get; init; } = "Client";
}
