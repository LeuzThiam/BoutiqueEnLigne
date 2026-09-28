namespace BoutiqueEnLigne.Web.Services.ApiModels
{
    public class OrderApiModel
    {
        public int Id { get; set; }
        public DateTime DateCommande { get; set; }
        public decimal Total { get; set; }
        public bool EstPayee { get; set; }
        public int UtilisateurId { get; set; }
        public List<OrderItemApiModel> ArticlesCommandes { get; set; } = [];
    }
}
