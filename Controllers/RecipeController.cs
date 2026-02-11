using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecipeManager.Data;
using RecipeManager.Models;

namespace RecipeManager.Controllers
{
    public class RecipeController : Controller
    {
        private readonly RecipeDbContext _context;

        public RecipeController(RecipeDbContext context)
        {
            _context = context;
        }

        // GET: Recipes
        public async Task<IActionResult> Index(string sortOrder, int? cuisineFilter, string searchString)
        {
            // Store current filter values in ViewBag for the view
            ViewBag.CurrentSort = sortOrder;
            ViewBag.CurrentCuisine = cuisineFilter;
            ViewBag.CurrentSearch = searchString;

            // Get all cuisines for the filter dropdown
            ViewBag.Cuisines = await _context.Cuisines
                .OrderBy(c => c.CuisineType)
                .ToListAsync();

            // Start with all recipes, including cuisine data
            var recipes = _context.Recipes
                .Include(r => r.Cuisine)
                .AsQueryable();

            // Apply search filter (case-insensitive)
            if (!string.IsNullOrEmpty(searchString))
            {
                string searchLower = searchString.ToLower();
                recipes = recipes.Where(r =>
                    r.RecipeName.ToLower().Contains(searchLower) ||
                    r.Description.ToLower().Contains(searchLower));
            }

            // Apply cuisine filter if selected
            if (cuisineFilter.HasValue)
            {
                recipes = recipes.Where(r => r.CuisineID == cuisineFilter.Value);
            }

            // Apply sorting
            recipes = sortOrder switch
            {
                "name_desc" => recipes.OrderByDescending(r => r.RecipeName),
                "date_asc" => recipes.OrderBy(r => r.DateAdded),
                "date_desc" => recipes.OrderByDescending(r => r.DateAdded),
                _ => recipes.OrderBy(r => r.RecipeName), // Default: A-Z
            };

            return View(await recipes.ToListAsync());
        }

        // GET: Recipes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var recipe = await _context.Recipes
                .Include(r => r.Cuisine)
                .Include(r => r.RecipeIngredients)
                    .ThenInclude(ri => ri.Ingredient)
                .Include(r => r.RecipeIngredients)
                    .ThenInclude(ri => ri.Unit)
                .Include(r => r.Instructions.OrderBy(i => i.StepNumber))
                .Include(r => r.RecipeTags)
                    .ThenInclude(rt => rt.Tag)
                .FirstOrDefaultAsync(m => m.RecipeID == id);

            if (recipe == null)
            {
                return NotFound();
            }

            return View(recipe);
        }

        // GET: Recipes/Create
        [Authorize]
        public IActionResult Create()
        {
            ViewBag.Cuisines = _context.Cuisines.OrderBy(c => c.CuisineType).ToList();
            ViewBag.Ingredients = _context.Ingredients.OrderBy(i => i.IngredientName).ToList();
            ViewBag.Units = _context.Units.OrderBy(u => u.UnitName).ToList();
            ViewBag.Tags = _context.Tags.OrderBy(t => t.TagName).ToList();
            ViewBag.Categories = _context.Categories.OrderBy(c => c.CategoryName).ToList();
            return View();
        }

        // POST: Recipes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create(Recipe recipe, List<RecipeIngredient> Ingredients, List<Instruction> Instructions)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Set the date added
                    recipe.DateAdded = DateTime.Now;

                    // Initialize collections to prevent duplicates
                    recipe.RecipeIngredients = new List<RecipeIngredient>();
                    recipe.Instructions = new List<Instruction>();

                    // Add the recipe first
                    _context.Recipes.Add(recipe);
                    await _context.SaveChangesAsync();

                    // Now add ingredients with the recipe ID
                    if (Ingredients != null && Ingredients.Any())
                    {
                        foreach (var ingredient in Ingredients)
                        {
                            if (ingredient.IngredientID > 0) // Only add if ingredient is selected
                            {
                                ingredient.RecipeID = recipe.RecipeID;
                                _context.RecipeIngredients.Add(ingredient);
                            }
                        }
                    }

                    // Add instructions with the recipe ID
                    if (Instructions != null && Instructions.Any())
                    {
                        int stepNumber = 1;
                        foreach (var instruction in Instructions)
                        {
                            if (!string.IsNullOrWhiteSpace(instruction.InstructionText))
                            {
                                instruction.RecipeID = recipe.RecipeID;
                                instruction.StepNumber = stepNumber++;
                                _context.Instructions.Add(instruction);
                            }
                        }
                    }

                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Recipe created successfully!";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating recipe: {ex.Message}");
                ModelState.AddModelError("", "An error occurred while creating the recipe. Please try again.");
            }

            // If we got here, something failed, redisplay form
            ViewBag.Cuisines = _context.Cuisines.OrderBy(c => c.CuisineType).ToList();
            ViewBag.Ingredients = _context.Ingredients.OrderBy(i => i.IngredientName).ToList();
            ViewBag.Units = _context.Units.OrderBy(u => u.UnitName).ToList();
            ViewBag.Tags = _context.Tags.OrderBy(t => t.TagName).ToList();
            ViewBag.Categories = _context.Categories.OrderBy(c => c.CategoryName).ToList();
            return View(recipe);
        }

    }
}