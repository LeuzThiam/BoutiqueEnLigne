namespace BoutiqueEnLigne.Identity.Domain.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Prenom { get; set; } = string.Empty;
        public string Nom { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Role { get; set; } = "Client";
        public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
    }
}
