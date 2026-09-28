using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations.Schema;

namespace BoutiqueEnLigne.Web.Models
{
    public class Panier
    {
        public int Id { get; set; }

        [ForeignKey(nameof(Utilisateur))]
        public int IdUtilisateur { get; set; }
        public Utilisateur? Utilisateur { get; set; }

        public ICollection<ArticlePanier> ArticlesPaniers { get; set; }
            = new List<ArticlePanier>();

        [NotMapped]
        public decimal Total { get; private set; }

    }
}
