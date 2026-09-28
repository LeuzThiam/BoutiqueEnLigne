namespace BoutiqueEnLigne.Web.Services.ApiModels
{
    public class UserApiModel
    {
        public int Id { get; set; }
        public string Prenom { get; set; } = string.Empty;
        public string Nom { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = "Client";
    }
}
