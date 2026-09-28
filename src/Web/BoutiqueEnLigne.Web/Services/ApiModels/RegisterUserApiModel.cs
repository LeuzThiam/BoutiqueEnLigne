namespace BoutiqueEnLigne.Web.Services.ApiModels;

public sealed class RegisterUserApiModel
{
    public string Prenom { get; init; } = string.Empty;
    public string Nom { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string MotDePasse { get; init; } = string.Empty;
    public string Role { get; init; } = "Client";
}
