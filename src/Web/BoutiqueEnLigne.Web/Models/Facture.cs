using System;
using System.Collections.Generic;

namespace BoutiqueEnLigne.Web.Models
{
    public class Facture
    {
        public int Id { get; set; }

        // Numéro de facture
        public string NumeroFacture { get; set; } = string.Empty;

        // Montant total de la facture
        public decimal MontantTotal { get; set; }

        // Date d'émission de la facture
        public DateTime DateFacturation { get; set; }

        // Propriété pour simplifier l'accès depuis la vue (ex: facture.Client)
        public Utilisateur? Client => Utilisateur;

        public DateTime DateFacture => DateFacturation;

        // Commande associée
        public int CommandeId { get; set; }
        public Commande? Commande { get; set; }

        // Client lié
        public int? UtilisateurId { get; set; }
        public Utilisateur? Utilisateur { get; set; }

        // Liste des articles facturés
        public ICollection<ArticleCommande> ArticlesFactures { get; set; } = new List<ArticleCommande>();
    }
}
