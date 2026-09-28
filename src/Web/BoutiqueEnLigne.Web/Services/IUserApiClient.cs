using BoutiqueEnLigne.Web.Models;

namespace BoutiqueEnLigne.Web.Services
{
    public interface IUserApiClient
    {
        Task<Utilisateur?> RegisterAsync(Utilisateur utilisateur);
        Task<UserAuthenticationResult?> LoginAsync(string email, string motDePasse);
        Task<Utilisateur?> GetByIdAsync(int id);
        Task RevokeAsync(string refreshToken);
    }
}
