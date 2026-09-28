using System.ComponentModel.DataAnnotations;

namespace BoutiqueEnLigne.Ordering.Api.Contracts.Requests;

public sealed class CreateOrderRequest
{
    public int UtilisateurId { get; init; }

    [Required, MinLength(1)]
    public List<CreateOrderItemRequest> ArticlesCommandes { get; init; } = [];
}

public sealed class CreateOrderItemRequest
{
    [Range(1, int.MaxValue)] public int ProduitId { get; init; }
    [Required, MaxLength(200)] public string NomProduit { get; init; } = string.Empty;
    [Range(1, int.MaxValue)] public int Quantite { get; init; }
    [Range(typeof(decimal), "0.01", "79228162514264337593543950335")] public decimal PrixUnitaire { get; init; }
}
