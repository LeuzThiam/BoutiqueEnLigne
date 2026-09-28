namespace BoutiqueEnLigne.Web.Services.ApiModels
{
    public class CartApiModel
    {
        public int UtilisateurId { get; set; }

        public List<CartItemApiModel> Articles { get; set; } = new();
    }
}
