using BoutiqueEnLigne.Web.Models;
using BoutiqueEnLigne.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace BoutiqueEnLigne.Web.Controllers
{
    public class CompteController : Controller
    {
        private readonly IUserApiClient _userApiClient;

        public CompteController(IUserApiClient userApiClient)
        {
            _userApiClient = userApiClient;
        }

        [HttpGet]
        public IActionResult Inscription()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Inscription(Utilisateur utilisateur)
        {
            if (!ModelState.IsValid)
            {
                return View(utilisateur);
            }

            try
            {
                var createdUser = await _userApiClient.RegisterAsync(utilisateur);
                if (createdUser != null)
                {
                    return RedirectToAction("Connexion");
                }
            }
            catch
            {
            }

            ModelState.AddModelError(string.Empty, "Impossible de créer le compte. Le service utilisateur est indisponible ou l'email existe déjà.");
            return View(utilisateur);
        }

        [HttpGet]
        public IActionResult Connexion()
        {
            return View(new Utilisateur());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Connexion(string email, string motDePasse)
        {
            var authentication = await _userApiClient.LoginAsync(email, motDePasse);

            if (authentication != null)
            {
                var utilisateur = authentication.User;
                HttpContext.Session.SetInt32("UtilisateurId", utilisateur.Id);
                HttpContext.Session.SetString("UtilisateurNom", utilisateur.Nom);
                HttpContext.Session.SetString("UtilisateurRole", utilisateur.Role.ToString());
                HttpContext.Session.SetString("AccessToken", authentication.AccessToken);
                HttpContext.Session.SetString("RefreshToken", authentication.RefreshToken);

                return RedirectToAction("Profil");
            }

            ViewBag.MessageErreur = "Identifiants invalides ou service utilisateur indisponible.";
            return View(new Utilisateur { Email = email });
        }

        [HttpGet]
        public async Task<IActionResult> Profil()
        {
            var utilisateurId = HttpContext.Session.GetInt32("UtilisateurId");
            if (utilisateurId == null)
            {
                return RedirectToAction("Connexion");
            }

            var utilisateur = await _userApiClient.GetByIdAsync(utilisateurId.Value);
            if (utilisateur == null)
            {
                return RedirectToAction("Connexion");
            }

            return View(utilisateur);
        }

        public async Task<IActionResult> Deconnexion()
        {
            var refreshToken = HttpContext.Session.GetString("RefreshToken");
            if (!string.IsNullOrWhiteSpace(refreshToken))
            {
                await _userApiClient.RevokeAsync(refreshToken);
            }
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}
