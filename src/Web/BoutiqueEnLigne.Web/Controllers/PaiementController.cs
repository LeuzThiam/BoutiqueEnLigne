using BoutiqueEnLigne.Web.Models;
using BoutiqueEnLigne.Web.Services;
using BoutiqueEnLigne.Web.Services.ApiModels;
using Microsoft.AspNetCore.Mvc;

namespace BoutiqueEnLigne.Web.Controllers
{
    public class PaiementController : Controller
    {
        private readonly ICartApiClient _cartApiClient;
        private readonly IOrderApiClient _orderApiClient;
        private readonly IPaymentApiClient _paymentApiClient;

        public PaiementController(
            ICartApiClient cartApiClient,
            IOrderApiClient orderApiClient,
            IPaymentApiClient paymentApiClient)
        {
            _cartApiClient = cartApiClient;
            _orderApiClient = orderApiClient;
            _paymentApiClient = paymentApiClient;
        }

        [HttpGet]
        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            if (GetCurrentUserId() is null)
            {
                return RedirectToAction("Connexion", "Compte");
            }

            var panier = await GetPanierAsync(cancellationToken);

            if (panier == null || !panier.ArticlesPaniers.Any())
            {
                return RedirectToAction("Index", "Panier");
            }

            await PreparePaymentViewAsync(panier, cancellationToken);
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(string cardName, string cardNumber, string expDate, string cvc, string postalCode)
        {
            var userId = GetCurrentUserId();
            if (userId is null)
            {
                return RedirectToAction("Connexion", "Compte");
            }

            if (!EffectuerPaiement(cardName, cardNumber, expDate, cvc, postalCode))
            {
                ViewBag.MessageErreur = "Le paiement a echoue. Veuillez reessayer.";
                await PreparePaymentViewAsync();
                return View();
            }

            var result = await FinaliserCommandeInterne(userId.Value);
            if (result is RedirectToActionResult redirectResult)
            {
                return redirectResult;
            }

            ViewBag.MessageErreur = "Votre panier est vide.";
            await PreparePaymentViewAsync();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreatePaymentIntent([FromBody] CreatePaymentIntentRequest request, CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            if (userId is null) return Unauthorized();

            if (request.Amount <= 0)
            {
                return BadRequest(new { error = "Le montant doit etre superieur a 0." });
            }

            try
            {
                var order = await CreatePendingOrderAsync(userId.Value, cancellationToken);
                if (order is null)
                {
                    return BadRequest(new { error = "Impossible de creer la commande." });
                }

                HttpContext.Session.SetInt32("PendingOrderId", order.Id);
                var paymentIntent = await _paymentApiClient.CreatePaymentIntentAsync(order.Id, request.Amount, cancellationToken);
                if (paymentIntent == null || string.IsNullOrWhiteSpace(paymentIntent.ClientSecret))
                {
                    return StatusCode(StatusCodes.Status502BadGateway, new { error = "Impossible de creer le paiement Stripe." });
                }

                return Json(new { clientSecret = paymentIntent.ClientSecret });
            }
            catch
            {
                return StatusCode(StatusCodes.Status502BadGateway, new { error = "Le service de paiement est indisponible." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> FinaliserCommande(CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            if (userId is null)
            {
                return Unauthorized();
            }

            var pendingOrderId = HttpContext.Session.GetInt32("PendingOrderId");
            if (pendingOrderId is not null)
            {
                TempData["CommandeId"] = pendingOrderId.Value;
                HttpContext.Session.Remove("PendingOrderId");
                await _cartApiClient.ClearCartAsync(userId.Value);
                return Json(new { redirectUrl = Url.Action("Confirmation", "Paiement") });
            }

            var result = await FinaliserCommandeInterne(userId.Value, cancellationToken);
            if (result is RedirectToActionResult)
            {
                return Json(new { redirectUrl = Url.Action("Confirmation", "Paiement") });
            }

            return result;
        }

        public IActionResult Confirmation()
        {
            ViewBag.CommandeId = TempData["CommandeId"];
            return View();
        }

        private async Task PreparePaymentViewAsync(Panier? panier = null, CancellationToken cancellationToken = default)
        {
            panier ??= await GetPanierAsync(cancellationToken);

            var total = panier?.ArticlesPaniers.Sum(article => (article.Produit?.Prix ?? 0m) * article.Quantite) ?? 0M;
            ViewBag.Total = total;
            ViewBag.TotalEnCents = (long)Math.Round(total * 100M, MidpointRounding.AwayFromZero);

            try
            {
                var paymentKey = await _paymentApiClient.GetPublishableKeyAsync(cancellationToken);
                ViewBag.StripePublishableKey = paymentKey?.Key ?? string.Empty;
                ViewBag.StripeConfigured = paymentKey?.Configured ?? false;
            }
            catch
            {
                ViewBag.StripePublishableKey = string.Empty;
                ViewBag.StripeConfigured = false;
            }
        }

        private async Task<IActionResult> FinaliserCommandeInterne(int utilisateurId, CancellationToken cancellationToken = default)
        {
            var createdOrder = await CreatePendingOrderAsync(utilisateurId, cancellationToken);
            if (createdOrder == null)
            {
                return BadRequest(new { error = "Impossible de creer la commande via le service des commandes." });
            }

            TempData["CommandeId"] = createdOrder.Id;
            await _cartApiClient.ClearCartAsync(utilisateurId);
            return RedirectToAction("Confirmation");
        }

        private async Task<OrderApiModel?> CreatePendingOrderAsync(int utilisateurId, CancellationToken cancellationToken)
        {
            var panier = await GetPanierAsync(cancellationToken);

            if (panier == null || !panier.ArticlesPaniers.Any())
            {
                return null;
            }

            var orderPayload = new OrderApiModel
            {
                UtilisateurId = utilisateurId,
                EstPayee = true,
                ArticlesCommandes = panier.ArticlesPaniers.Select(article => new OrderItemApiModel
                {
                    ProduitId = article.ProduitId,
                    NomProduit = article.Produit?.Nom ?? string.Empty,
                    Quantite = article.Quantite,
                    PrixUnitaire = article.Produit?.Prix ?? 0m
                }).ToList()
            };

            orderPayload.Total = orderPayload.ArticlesCommandes.Sum(a => a.PrixUnitaire * a.Quantite);

            try
            {
                return await _orderApiClient.CreateOrderAsync(orderPayload);
            }
            catch
            {
                return null;
            }
        }

        private int? GetCurrentUserId()
        {
            return HttpContext.Session.GetInt32("UtilisateurId");
        }

        private async Task<Panier?> GetPanierAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var userId = GetCurrentUserId();
            if (userId is null)
            {
                return null;
            }

            var cart = await _cartApiClient.GetCartAsync(userId.Value);
            if (cart == null)
            {
                return null;
            }

            return new Panier
            {
                IdUtilisateur = cart.UtilisateurId,
                ArticlesPaniers = cart.Articles.Select(article => new ArticlePanier
                {
                    ProduitId = article.ProduitId,
                    Quantite = article.Quantite,
                    Produit = new Produit
                    {
                        Id = article.ProduitId,
                        Nom = article.NomProduit,
                        Prix = article.PrixUnitaire,
                        UrlImage = article.UrlImage
                    }
                }).ToList()
            };
        }

        private static bool EffectuerPaiement(string cardName, string cardNumber, string expDate, string cvc, string postalCode)
        {
            return !string.IsNullOrWhiteSpace(cardName)
                && !string.IsNullOrWhiteSpace(cardNumber)
                && !string.IsNullOrWhiteSpace(expDate)
                && !string.IsNullOrWhiteSpace(cvc)
                && !string.IsNullOrWhiteSpace(postalCode);
        }

        public class CreatePaymentIntentRequest
        {
            public long Amount { get; set; }
        }
    }
}
