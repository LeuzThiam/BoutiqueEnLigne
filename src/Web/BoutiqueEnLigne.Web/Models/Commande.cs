using System;
using System.Collections.Generic;

namespace BoutiqueEnLigne.Web.Models
{
    public class Commande
    {
        public int Id { get; set; }
        public DateTime DateCommande { get; set; } = DateTime.Now;

        // Total de la commande
        public decimal Total { get; set; }

        // Statut de paiement (true si payée)
        public bool EstPayee { get; set; }

        // Lien vers le client
        public int UtilisateurId { get; set; }
        public Utilisateur? Utilisateur { get; set; }

        // Liste des articles commandés
        public ICollection<ArticleCommande> ArticlesCommandes { get; set; } = new List<ArticleCommande>();

        // Relation avec Facture (1-1)
        public Facture? Facture { get; set; }
    }
}
