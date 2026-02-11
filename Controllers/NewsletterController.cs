using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RecipesVault.Services;

namespace RecipesVault.Controllers
{
    public class NewsletterController : Controller
    {
        private readonly INewsletterService _newsletterService;

        public NewsletterController(INewsletterService newsletterService)
        {
            _newsletterService = newsletterService;
        }

        [HttpPost]
        public async Task<IActionResult> Subscribe(string email, string returnUrl = null)
        {
            if (string.IsNullOrEmpty(email))
            {
                TempData["Error"] = "Please provide a valid email address.";
                return Redirect(returnUrl ?? "/");
            }

            var userId = User?.FindFirstValue(ClaimTypes.NameIdentifier);
            var success = await _newsletterService.SubscribeAsync(email, userId);

            if (success)
            {
                TempData["Success"] = "Thanks for subscribing! Check your email for a welcome message.";
            }
            else
            {
                TempData["Info"] = "This email is already subscribed to our newsletter.";
            }

            return Redirect(returnUrl ?? "/");
        }

        [HttpGet]
        public async Task<IActionResult> Unsubscribe(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return View("UnsubscribeError");
            }

            var success = await _newsletterService.UnsubscribeAsync(token);
            
            ViewBag.Success = success;
            return View();
        }
    }
}
