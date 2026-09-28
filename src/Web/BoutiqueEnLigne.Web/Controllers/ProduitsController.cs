using BoutiqueEnLigne.Web.Models;
using BoutiqueEnLigne.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace BoutiqueEnLigne.Web.Controllers
{
    public class ProduitsController : Controller
    {
        private readonly IProductApiClient _productApiClient;

        public ProduitsController(IProductApiClient productApiClient)
        {
            _productApiClient = productApiClient;
        }

        public async Task<IActionResult> Index(string search)
        {
            ViewBag.Search = search;
            ViewBag.DataSource = "api";

            try
            {
                var produits = await _productApiClient.GetProduitsAsync(search);
                return View(produits);
            }
            catch
            {
                ViewBag.ErrorMessage = "Le service des produits est indisponible.";
                return View(new List<Produit>());
            }
        }

        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var produit = await _productApiClient.GetProduitAsync(id);
                if (produit == null)
                {
                    return NotFound();
                }

                ViewBag.DataSource = "api";
                return View(produit);
            }
            catch
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable);
            }
        }

        public IActionResult Creer()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Creer(Produit produit)
        {
            if (!ModelState.IsValid)
            {
                return View(produit);
            }

            try
            {
                var createdProduct = await _productApiClient.CreateProduitAsync(produit);
                if (createdProduct != null)
                {
                    return RedirectToAction(nameof(Index));
                }
            }
            catch
            {
                ModelState.AddModelError(string.Empty, "Le service des produits est indisponible.");
                return View(produit);
            }

            ModelState.AddModelError(string.Empty, "Impossible de créer le produit via le service de produits.");
            return View(produit);
        }

        public async Task<IActionResult> Modifier(int id)
        {
            var produit = await _productApiClient.GetProduitAsync(id);
            if (produit == null)
            {
                return NotFound();
            }

            return View(produit);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Modifier(Produit produit)
        {
            if (!ModelState.IsValid)
            {
                return View(produit);
            }

            try
            {
                if (await _productApiClient.UpdateProduitAsync(produit))
                {
                    return RedirectToAction(nameof(Index));
                }
            }
            catch
            {
                ModelState.AddModelError(string.Empty, "Le service des produits est indisponible.");
                return View(produit);
            }

            ModelState.AddModelError(string.Empty, "Impossible de mettre à jour le produit via le service de produits.");
            return View(produit);
        }

        public async Task<IActionResult> Supprimer(int id)
        {
            var produit = await _productApiClient.GetProduitAsync(id);
            if (produit == null)
            {
                return NotFound();
            }

            return View(produit);
        }

        [HttpPost, ActionName("Supprimer")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SupprimerConfirme(int id)
        {
            try
            {
                if (await _productApiClient.DeleteProduitAsync(id))
                {
                    return RedirectToAction(nameof(Index));
                }
            }
            catch
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable);
            }

            ModelState.AddModelError(string.Empty, "Impossible de supprimer le produit via le service de produits.");
            return RedirectToAction(nameof(Supprimer), new { id });
        }
    }
}
