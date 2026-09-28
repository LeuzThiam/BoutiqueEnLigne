using System.Net.Http.Json;
using BoutiqueEnLigne.Web.Models;
using BoutiqueEnLigne.Web.Services.ApiModels;

namespace BoutiqueEnLigne.Web.Services
{
    public class UserApiClient : IUserApiClient
    {
        private readonly HttpClient _httpClient;

        public UserApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<Utilisateur?> RegisterAsync(Utilisateur utilisateur)
        {
            var payload = ToApiModel(utilisateur);
            var response = await _httpClient.PostAsJsonAsync("api/users/register", payload);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var user = await response.Content.ReadFromJsonAsync<UserApiModel>();
            return user is null ? null : ToDomainModel(user);
        }

        public async Task<UserAuthenticationResult?> LoginAsync(string email, string motDePasse)
        {
            var response = await _httpClient.PostAsJsonAsync("api/users/login", new LoginRequestApiModel
            {
                Email = email,
                MotDePasse = motDePasse
            });

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var authentication = await response.Content.ReadFromJsonAsync<AuthenticationApiModel>();
            return authentication is null
                ? null
                : new UserAuthenticationResult(
                    ToDomainModel(authentication),
                    authentication.AccessToken,
                    authentication.RefreshToken,
                    authentication.AccessTokenExpiresAtUtc,
                    authentication.RefreshTokenExpiresAtUtc);
        }

        public async Task RevokeAsync(string refreshToken)
        {
            using var response = await _httpClient.PostAsJsonAsync("api/users/revoke", new { refreshToken });
        }

        public async Task<Utilisateur?> GetByIdAsync(int id)
        {
            var user = await _httpClient.GetFromJsonAsync<UserApiModel>($"api/users/{id}");
            return user is null ? null : ToDomainModel(user);
        }

        private static RegisterUserApiModel ToApiModel(Utilisateur utilisateur)
        {
            return new RegisterUserApiModel
            {
                Prenom = utilisateur.Prenom,
                Nom = utilisateur.Nom,
                Email = utilisateur.Email,
                MotDePasse = utilisateur.MotDePasse,
                Role = utilisateur.Role.ToString()
            };
        }

        private static Utilisateur ToDomainModel(UserApiModel user)
        {
            Enum.TryParse<RoleUtilisateur>(user.Role, true, out var role);

            return new Utilisateur
            {
                Id = user.Id,
                Prenom = user.Prenom,
                Nom = user.Nom,
                Email = user.Email,
                Role = role
            };
        }
    }
}
