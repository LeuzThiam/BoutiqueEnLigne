namespace BoutiqueEnLigne.Web.Models
{
    public class ArticlePanier
    {
        public int Id { get; set; }

        // Lien vers le panier
        public int PanierId { get; set; }
        public Panier? Panier { get; set; }

        // Lien vers le produit
        public int ProduitId { get; set; }
        public Produit? Produit { get; set; }

        // Quantité souhaitée
        public int Quantite { get; set; }
    }
}
