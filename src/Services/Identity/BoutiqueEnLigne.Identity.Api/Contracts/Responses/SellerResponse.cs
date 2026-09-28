using BoutiqueEnLigne.Identity.Domain.Entities;

namespace BoutiqueEnLigne.Identity.Api.Contracts.Responses;

public sealed record SellerResponse(int Id, string Nom, string Email, bool Actif)
{
    public static SellerResponse FromUser(User user) =>
        new(user.Id, $"{user.Prenom} {user.Nom}".Trim(), user.Email, true);
}
