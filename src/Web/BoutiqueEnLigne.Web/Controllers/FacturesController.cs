using BoutiqueEnLigne.Web.Models;
using BoutiqueEnLigne.Web.Services;
using BoutiqueEnLigne.Web.Services.ApiModels;
using Microsoft.AspNetCore.Mvc;

namespace BoutiqueEnLigne.Web.Controllers
{
    public class FacturesController : Controller
    {
        private readonly IOrderApiClient _orderApiClient;
        private readonly IUserApiClient _userApiClient;

        public FacturesController(IOrderApiClient orderApiClient, IUserApiClient userApiClient)
        {
            _orderApiClient = orderApiClient;
            _userApiClient = userApiClient;
        }

        public async Task<IActionResult> Index()
        {
            var utilisateurId = GetCurrentUserId();
            var utilisateur = await _userApiClient.GetByIdAsync(utilisateurId);
            var commandes = await _orderApiClient.GetOrdersAsync(utilisateurId);

            var factures = commandes
                .Select(order => MapFacture(order, utilisateur))
                .ToList();

            return View(factures);
        }

        public async Task<IActionResult> Details(int id)
        {
            var utilisateurId = GetCurrentUserId();
            var utilisateur = await _userApiClient.GetByIdAsync(utilisateurId);
            var commande = await _orderApiClient.GetOrderAsync(id);

            if (commande == null)
            {
                return NotFound();
            }

            return View(MapFacture(commande, utilisateur));
        }

        public IActionResult TelechargerPdf(int id)
        {
            return RedirectToAction("Details", new { id });
        }

        private int GetCurrentUserId()
        {
            return HttpContext.Session.GetInt32("UtilisateurId") ?? 1;
        }

        private static Facture MapFacture(OrderApiModel commande, Utilisateur? utilisateur)
        {
            return new Facture
            {
                Id = commande.Id,
                CommandeId = commande.Id,
                NumeroFacture = $"FAC-{commande.Id:D6}",
                DateFacturation = commande.DateCommande,
                MontantTotal = commande.Total,
                UtilisateurId = commande.UtilisateurId,
                Utilisateur = utilisateur,
                ArticlesFactures = commande.ArticlesCommandes.Select(item => new ArticleCommande
                {
                    ProduitId = item.ProduitId,
                    Quantite = item.Quantite,
                    PrixUnitaire = item.PrixUnitaire,
                    Produit = new Produit
                    {
                        Id = item.ProduitId,
                        Nom = item.NomProduit,
                        Prix = item.PrixUnitaire
                    }
                }).ToList()
            };
        }
    }
}
