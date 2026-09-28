namespace BoutiqueEnLigne.Web.Services.ApiModels
{
    public class OrderItemApiModel
    {
        public int ProduitId { get; set; }
        public string NomProduit { get; set; } = string.Empty;
        public int Quantite { get; set; }
        public decimal PrixUnitaire { get; set; }
    }
}
