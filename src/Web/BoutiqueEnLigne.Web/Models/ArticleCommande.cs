namespace BoutiqueEnLigne.Web.Models
{
    public class ArticleCommande
    {
        public int Id { get; set; }

        // Lien vers la commande
        public int CommandeId { get; set; }
        public Commande? Commande { get; set; }

        // Lien vers le produit
        public int ProduitId { get; set; }
        public Produit? Produit { get; set; }

        // Quantité achetée
        public int Quantite { get; set; }

        // Prix unitaire figé au moment de l'achat
        public decimal PrixUnitaire { get; set; }
    }
}
