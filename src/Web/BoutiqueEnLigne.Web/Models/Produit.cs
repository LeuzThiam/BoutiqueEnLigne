using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BoutiqueEnLigne.Web.Models
{
    public class Produit
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Le nom du produit est requis.")]
        public string Nom { get; set; } = string.Empty;

        [Required(ErrorMessage = "La description est requise.")]
        public string Description { get; set; } = string.Empty;

        [Range(0.01, 1000000, ErrorMessage = "Le prix doit etre superieur a zero.")]
        public decimal Prix { get; set; }

        [Range(0, 100, ErrorMessage = "Le pourcentage de remise doit etre entre 0 et 100.")]
        public double? PourcentageRemise { get; set; }

        [Range(0, 5, ErrorMessage = "La note doit etre entre 0 et 5.")]
        public double? Note { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "La quantite en stock doit etre positive.")]
        public int? QuantiteEnStock { get; set; }

        public string Marque { get; set; } = string.Empty;

        [Required(ErrorMessage = "L'URL de l'image est requise.")]
        [Url(ErrorMessage = "L'URL n'est pas valide.")]
        public string UrlImage { get; set; } = string.Empty;

        [ValidateNever]
        public List<string> ListeImages { get; set; } = new();

        [Required(ErrorMessage = "La categorie est requise.")]
        public string CategorieNom { get; set; } = string.Empty;

        public int? VendeurId { get; set; }

        [ValidateNever]
        public Utilisateur? Vendeur { get; set; }

        [ValidateNever]
        public ICollection<ArticleCommande> ArticlesCommandes { get; set; } = new List<ArticleCommande>();

        [ValidateNever]
        public ICollection<ArticlePanier> ArticlesPaniers { get; set; } = new List<ArticlePanier>();
    }
}
