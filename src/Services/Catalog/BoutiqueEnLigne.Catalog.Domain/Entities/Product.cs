namespace BoutiqueEnLigne.Catalog.Domain.Entities
{
    public class Product
    {
        public int Id { get; set; }
        public string Nom { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Prix { get; set; }
        public int Quantite { get; set; }
        public DateTime DateAjout { get; set; }
        public string UrlImage { get; set; } = string.Empty;
        public string CategorieNom { get; set; } = string.Empty;
        public int? VendeurId { get; set; }
    }
}
