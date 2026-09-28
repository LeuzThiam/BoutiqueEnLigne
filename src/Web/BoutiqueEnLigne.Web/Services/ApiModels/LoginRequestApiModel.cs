namespace BoutiqueEnLigne.Web.Services.ApiModels
{
    public class LoginRequestApiModel
    {
        public string Email { get; set; } = string.Empty;
        public string MotDePasse { get; set; } = string.Empty;
    }
}
