using BoutiqueEnLigne.Web.Models;
using BoutiqueEnLigne.Web.Services;
using BoutiqueEnLigne.Web.Services.ApiModels;
using Microsoft.AspNetCore.Mvc;

namespace BoutiqueEnLigne.Web.Controllers
{
    public class PanierController : Controller
    {
        private readonly ICartApiClient _cartApiClient;
        private readonly IProductApiClient _productApiClient;

        public PanierController(ICartApiClient cartApiClient, IProductApiClient productApiClient)
        {
            _cartApiClient = cartApiClient;
            _productApiClient = productApiClient;
        }

        public async Task<IActionResult> Index()
        {
            var userId = GetCurrentUserId();
            if (userId is null)
            {
                return RedirectToAction("Connexion", "Compte");
            }

            var panier = await GetPanierAsync(userId.Value);

            var total = panier.ArticlesPaniers.Sum(a => (a.Produit?.Prix ?? 0m) * a.Quantite);
            ViewBag.Total = total;

            return View(panier);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Ajouter(int idProduit, int quantite = 1)
        {
            var userId = GetCurrentUserId();
            if (userId is null)
            {
                return RedirectToAction("Connexion", "Compte");
            }

            var produit = await _productApiClient.GetProduitAsync(idProduit);
            if (produit == null)
            {
                return NotFound();
            }

            var cart = await _cartApiClient.AddItemAsync(userId.Value, new UpsertCartItemApiModel
            {
                ProduitId = produit.Id,
                Quantite = quantite,
                NomProduit = produit.Nom,
                PrixUnitaire = produit.Prix,
                UrlImage = produit.UrlImage
            });

            if (cart == null)
            {
                return StatusCode(StatusCodes.Status502BadGateway);
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Supprimer(int idProduit)
        {
            var userId = GetCurrentUserId();
            if (userId is null)
            {
                return RedirectToAction("Connexion", "Compte");
            }

            await _cartApiClient.RemoveItemAsync(userId.Value, idProduit);
            return RedirectToAction(nameof(Index));
        }

        private async Task<Panier> GetPanierAsync(int userId)
        {
            var cart = await _cartApiClient.GetCartAsync(userId);
            if (cart == null)
            {
                return new Panier
                {
                    IdUtilisateur = userId,
                    ArticlesPaniers = new List<ArticlePanier>()
                };
            }

            return new Panier
            {
                IdUtilisateur = cart.UtilisateurId,
                ArticlesPaniers = cart.Articles.Select(MapArticle).ToList()
            };
        }

        private int? GetCurrentUserId()
        {
            return HttpContext.Session.GetInt32("UtilisateurId");
        }

        private static ArticlePanier MapArticle(CartItemApiModel item)
        {
            return new ArticlePanier
            {
                ProduitId = item.ProduitId,
                Quantite = item.Quantite,
                Produit = new Produit
                {
                    Id = item.ProduitId,
                    Nom = item.NomProduit,
                    Prix = item.PrixUnitaire,
                    UrlImage = item.UrlImage
                }
            };
        }
    }
}
