using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecipeManager.Data;
using RecipesVault.Services;

namespace RecipesVault.Controllers
{
    [Authorize(Roles = "Admin")] 
    [Route("admin/newsletter")]
    public class AdminNewsletterController : Controller
    {
        private readonly INewsletterService _newsletterService;
        private readonly RecipeDbContext _context;

        public AdminNewsletterController(
            INewsletterService newsletterService,
            RecipeDbContext context)
        {
            _newsletterService = newsletterService;
            _context = context;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            ViewBag.SubscriberCount = await _newsletterService.GetSubscriberCountAsync();
            return View();
        }

        [HttpGet("compose")]
        public async Task<IActionResult> Compose()
        {
            // Get featured recipes for easy insertion
            var featuredRecipes = await _context.Recipes
                .Include(r => r.Cuisine)
                .OrderByDescending(r => r.DateAdded)
                .Take(10)
                .Select(r => new
                {
                    r.RecipeID,
                    r.RecipeName,
                    r.Description,
                    CuisineType = r.Cuisine.CuisineType,
                    r.PrepTime,
                    r.CookTime
                })
                .ToListAsync();

            ViewBag.FeaturedRecipes = featuredRecipes;
            ViewBag.SubscriberCount = await _newsletterService.GetSubscriberCountAsync();

            return View();
        }

        [HttpPost("send")]
        public async Task<IActionResult> Send(string subject, string htmlContent)
        {
            if (string.IsNullOrEmpty(subject) || string.IsNullOrEmpty(htmlContent))
            {
                TempData["Error"] = "Subject and content are required.";
                return RedirectToAction("Compose");
            }

            try
            {
                await _newsletterService.SendNewsletterAsync(subject, htmlContent);
                TempData["Success"] = $"Newsletter sent successfully to all subscribers!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error sending newsletter: {ex.Message}";
                return RedirectToAction("Compose");
            }
        }

        [HttpGet("preview")]
        public IActionResult Preview(string htmlContent)
        {
            ViewBag.HtmlContent = htmlContent;
            return View();
        }

        [HttpGet("subscribers")]
        public async Task<IActionResult> Subscribers()
        {
            var subscribers = await _context.NewsletterSubscribers
                .OrderByDescending(s => s.SubscribedDate)
                .ToListAsync();

            return View(subscribers);
        }

        [HttpGet("templates")]
        public IActionResult Templates()
        {
            return View();
        }

        [HttpPost("get-template")]
        public IActionResult GetTemplate([FromBody] TemplateRequest request)
        {
            var template = request.TemplateType switch
            {
                "weekly" => GetWeeklyRoundupTemplate(),
                "featured" => GetFeaturedRecipesTemplate(),
                "seasonal" => GetSeasonalTemplate(),
                _ => ""
            };

            return Json(new { html = template });
        }

        private string GetWeeklyRoundupTemplate()
        {
            return @"
<!DOCTYPE html>
<html>
<head>
    <style>
        body { font-family: Arial, sans-serif; line-height: 1.6; color: #333; background: #f4f4f4; }
        .container { max-width: 600px; margin: 20px auto; background: white; }
        .header { background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 40px 20px; text-align: center; }
        .content { padding: 30px; }
        .recipe-card { margin: 20px 0; padding: 20px; border: 1px solid #ddd; border-radius: 8px; }
        .recipe-card h3 { margin-top: 0; color: #667eea; }
        .recipe-meta { color: #666; font-size: 14px; }
        .button { display: inline-block; padding: 12px 30px; background: #667eea; color: white; text-decoration: none; border-radius: 5px; margin: 10px 0; }
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🍳 Weekly Recipe Roundup</h1>
            <p>Your favorite recipes this week</p>
        </div>
        <div class='content'>
            <h2>This Week's Featured Recipes</h2>
            <p>Here are some amazing recipes we think you'll love:</p>
            
            <!-- Add recipe cards here -->
            <div class='recipe-card'>
                <h3>Recipe Name Here</h3>
                <p>Brief description of the recipe goes here...</p>
                <p class='recipe-meta'>⏱️ 30 min | 🍽️ 4 servings | 🌍 Italian</p>
                <a href='#' class='button'>View Recipe</a>
            </div>
            
            <p style='margin-top: 40px;'>Happy cooking! 👨‍🍳</p>
        </div>
    </div>
</body>
</html>";
        }

        private string GetFeaturedRecipesTemplate()
        {
            return @"
<!DOCTYPE html>
<html>
<head>
    <style>
        body { font-family: Arial, sans-serif; line-height: 1.6; color: #333; background: #f4f4f4; }
        .container { max-width: 600px; margin: 20px auto; background: white; }
        .header { background: linear-gradient(135deg, #ff6b6b 0%, #ee5a6f 100%); color: white; padding: 40px 20px; text-align: center; }
        .content { padding: 30px; }
        .highlight { background: #fff9e6; padding: 20px; border-left: 4px solid #ffd93d; margin: 20px 0; }
        .button { display: inline-block; padding: 12px 30px; background: #ff6b6b; color: white; text-decoration: none; border-radius: 5px; }
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>⭐ Featured Recipes</h1>
            <p>Our top-rated dishes you need to try</p>
        </div>
        <div class='content'>
            <h2>Chef's Picks</h2>
            <p>These highly-rated recipes are favorites among our community:</p>
            
            <div class='highlight'>
                <h3>Recipe of the Month</h3>
                <p>Add your featured recipe details here...</p>
                <a href='#' class='button'>Try This Recipe</a>
            </div>
        </div>
    </div>
</body>
</html>";
        }

        private string GetSeasonalTemplate()
        {
            return @"
<!DOCTYPE html>
<html>
<head>
    <style>
        body { font-family: Arial, sans-serif; line-height: 1.6; color: #333; background: #f4f4f4; }
        .container { max-width: 600px; margin: 20px auto; background: white; }
        .header { background: linear-gradient(135deg, #56ab2f 0%, #a8e063 100%); color: white; padding: 40px 20px; text-align: center; }
        .content { padding: 30px; }
        .season-highlight { background: #f0fff4; padding: 20px; border-radius: 8px; margin: 20px 0; }
        .ingredient-list { columns: 2; }
        .button { display: inline-block; padding: 12px 30px; background: #56ab2f; color: white; text-decoration: none; border-radius: 5px; }
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🍂 Seasonal Favorites</h1>
            <p>Fresh ingredients, amazing flavors</p>
        </div>
        <div class='content'>
            <h2>This Season's Best</h2>
            <p>Make the most of seasonal ingredients with these delicious recipes:</p>
            
            <div class='season-highlight'>
                <h3>In Season Now:</h3>
                <ul class='ingredient-list'>
                    <li>Ingredient 1</li>
                    <li>Ingredient 2</li>
                    <li>Ingredient 3</li>
                    <li>Ingredient 4</li>
                </ul>
            </div>
            
            <p>Explore recipes featuring these fresh ingredients:</p>
            <a href='#' class='button'>Browse Seasonal Recipes</a>
        </div>
    </div>
</body>
</html>";
        }

        public class TemplateRequest
        {
            public string TemplateType { get; set; }
        }
    }
}
