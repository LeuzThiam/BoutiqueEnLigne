using Microsoft.AspNetCore.Mvc;

namespace BoutiqueEnLigne.Web.Controllers
{
    public class ErreurController : Controller
    {
        [Route("Erreur/Status")]
        public IActionResult Status(int code)
        {
            if (code == 404)
                return View("Error"); //  utilise la vue personnalisée

            ViewBag.ErrorCode = code;
            return View("Home"); // autre vue pour erreurs 500 etc.
        }
    }
}
