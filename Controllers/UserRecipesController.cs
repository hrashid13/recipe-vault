using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecipeManager.Data;
using RecipeManager.Models;
using System.Security.Claims;

namespace RecipeManager.Controllers
{
    [Authorize]
    public class UserRecipesController : Controller
    {
        private readonly RecipeDbContext _context;

        public UserRecipesController(RecipeDbContext context)
        {
            _context = context;
        }

        // GET: UserRecipes (My Saved Recipes page)
        public async Task<IActionResult> Index()
        {
            var googleId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = await _context.Users.FirstOrDefaultAsync(u => u.GoogleID == googleId);

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var savedRecipes = await _context.UserRecipes
                .Where(ur => ur.UserID == user.UserID)
                .Include(ur => ur.Recipe)
                    .ThenInclude(r => r.Cuisine)
                .OrderByDescending(ur => ur.DateSaved)
                .ToListAsync();

            return View(savedRecipes);
        }

        // POST: UserRecipes/Save/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(int recipeId, string returnUrl = null)
        {
            var googleId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = await _context.Users.FirstOrDefaultAsync(u => u.GoogleID == googleId);

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Check if already saved
            var existing = await _context.UserRecipes
                .FirstOrDefaultAsync(ur => ur.UserID == user.UserID && ur.RecipeID == recipeId);

            if (existing == null)
            {
                var userRecipe = new UserRecipe
                {
                    UserID = user.UserID,
                    RecipeID = recipeId,
                    DateSaved = DateTime.UtcNow
                };

                _context.UserRecipes.Add(userRecipe);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Recipe saved successfully!";
            }
            else
            {
                TempData["InfoMessage"] = "You've already saved this recipe.";
            }

            // Redirect back to where they came from
            if (!string.IsNullOrEmpty(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Details", "Recipe", new { id = recipeId });
        }

        // POST: UserRecipes/Unsave/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Unsave(int recipeId, string returnUrl = null)
        {
            var googleId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = await _context.Users.FirstOrDefaultAsync(u => u.GoogleID == googleId);

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var userRecipe = await _context.UserRecipes
                .FirstOrDefaultAsync(ur => ur.UserID == user.UserID && ur.RecipeID == recipeId);

            if (userRecipe != null)
            {
                _context.UserRecipes.Remove(userRecipe);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Recipe removed from saved recipes.";
            }

            // Redirect back to where they came from
            if (!string.IsNullOrEmpty(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index");
        }

        // Helper method to check if a recipe is saved by current user
        // This is used by the view to show/hide save button
        [HttpGet]
        public async Task<JsonResult> IsSaved(int recipeId)
        {
            var googleId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = await _context.Users.FirstOrDefaultAsync(u => u.GoogleID == googleId);

            if (user == null)
            {
                return Json(new { isSaved = false });
            }

            var exists = await _context.UserRecipes
                .AnyAsync(ur => ur.UserID == user.UserID && ur.RecipeID == recipeId);

            return Json(new { isSaved = exists });
        }
    }
}
