namespace BoutiqueEnLigne.Web.Services.ApiModels
{
    public class UpsertCartItemApiModel
    {
        public int ProduitId { get; set; }

        public int Quantite { get; set; } = 1;

        public string NomProduit { get; set; } = string.Empty;

        public decimal PrixUnitaire { get; set; }

        public string UrlImage { get; set; } = string.Empty;
    }
}
