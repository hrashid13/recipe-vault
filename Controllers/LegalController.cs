using Microsoft.AspNetCore.Mvc;

namespace RecipeDatabase.Controllers
{
    public class LegalController : Controller
    {
        // GET: /Legal/Privacy or /Privacy
        [Route("Privacy")]
        [Route("Legal/Privacy")]
        public IActionResult Privacy()
        {
            return View();
        }

        // GET: /Legal/Terms or /Terms
        [Route("Terms")]
        [Route("Legal/Terms")]
        public IActionResult Terms()
        {
            return View();
        }
    }
}
