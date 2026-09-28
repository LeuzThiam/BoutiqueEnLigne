using BoutiqueEnLigne.Identity.Domain.Entities;

namespace BoutiqueEnLigne.Identity.Api.Contracts.Responses;

public sealed record UserResponse(int Id, string Prenom, string Nom, string Email, string Role)
{
    public static UserResponse FromUser(User user) =>
        new(user.Id, user.Prenom, user.Nom, user.Email, user.Role);
}
