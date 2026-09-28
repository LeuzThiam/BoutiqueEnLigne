namespace BoutiqueEnLigne.Cart.Domain.Entities;

public class ShoppingCart
{
    public int UtilisateurId { get; set; }

    public List<CartItem> Articles { get; set; } = [];

    public DateTime DerniereMiseAJourUtc { get; set; } = DateTime.UtcNow;

    public decimal Total => Articles.Sum(article => article.PrixUnitaire * article.Quantite);
}
