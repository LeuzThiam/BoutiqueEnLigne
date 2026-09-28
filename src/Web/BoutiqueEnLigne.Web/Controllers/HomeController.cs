using Microsoft.AspNetCore.Mvc;
using BoutiqueEnLigne.Web.Models;
using BoutiqueEnLigne.Web.Services;

namespace BoutiqueEnLigne.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IProductApiClient _productApiClient;

        public HomeController(IProductApiClient productApiClient)
        {
            _productApiClient = productApiClient;
        }

        // PAGE D'ACCUEIL
        public async Task<IActionResult> Index()
        {
            var produits = new List<Produit>();

            try
            {
                produits = (await _productApiClient.GetProduitsAsync())
                    .OrderByDescending(p => p.Id)
                    .Take(6)
                    .ToList();
            }
            catch
            {
                ViewBag.ErrorMessage = "Le service des produits est indisponible.";
            }

            return View(produits);
        }

        // PAGE D'ERREUR (Optionnelle)
        public IActionResult Error()
        {
            return View();
        }

        // PAGE CONTACT (exemple de page statique)
        public IActionResult Contact()
        {
            return View();
        }
    }
}
