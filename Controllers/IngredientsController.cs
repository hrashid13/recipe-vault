using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecipeManager.Data;
using RecipeManager.Models;

namespace RecipeManager.Controllers
{
    public class IngredientsController : Controller
    {
        private readonly RecipeDbContext _context;

        public IngredientsController(RecipeDbContext context)
        {
            _context = context;
        }

        // GET: Ingredients
        public async Task<IActionResult> Index(int? categoryFilter, string searchString)
        {
            // Store current filter value in ViewBag for the view
            ViewBag.CurrentCategory = categoryFilter;
            ViewBag.CurrentSearch = searchString;

            // Get all categories for the filter dropdown
            ViewBag.Categories = await _context.Categories
                .OrderBy(c => c.CategoryName)
                .ToListAsync();

            // Start with all ingredients, including category data
            var ingredients = _context.Ingredients
                .Include(i => i.Category)
                .AsQueryable();

            // Apply search filter (case-insensitive)
            if (!string.IsNullOrEmpty(searchString))
            {
                ingredients = ingredients.Where(i =>
                    i.IngredientName.ToLower().Contains(searchString.ToLower()));
            }

            // Apply category filter if selected
            if (categoryFilter.HasValue)
            {
                ingredients = ingredients.Where(i => i.CategoryID == categoryFilter.Value);
            }

            // Always sort alphabetically A-Z
            ingredients = ingredients.OrderBy(i => i.IngredientName);

            return View(await ingredients.ToListAsync());
        }

        // GET: Ingredients/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ingredient = await _context.Ingredients
                .Include(i => i.Category)
                .FirstOrDefaultAsync(m => m.IngredientID == id);

            if (ingredient == null)
            {
                return NotFound();
            }

            return View(ingredient);
        }

        // GET: Ingredients/Create
        [Authorize]
        public IActionResult Create()
        {
            // Get all categories for the dropdown
            ViewBag.Categories = _context.Categories
                .OrderBy(c => c.CategoryName)
                .ToList();

            return View();
        }

        // POST: Ingredients/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create([Bind("IngredientName,CategoryID")] Ingredient ingredient)
        {
            if (ModelState.IsValid)
            {
                _context.Add(ingredient);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // If validation fails, reload categories for dropdown
            ViewBag.Categories = _context.Categories
                .OrderBy(c => c.CategoryName)
                .ToList();

            return View(ingredient);
        }

        // GET: API endpoint to get all ingredients as JSON
        [HttpGet]
        public JsonResult GetIngredientsJson()
        {
            var ingredientsList = _context.Ingredients
                .OrderBy(i => i.IngredientName)
                .Select(i => new {
                    ingredientID = i.IngredientID,
                    ingredientName = i.IngredientName,
                    categoryID = i.CategoryID
                })
                .ToList();

            return Json(ingredientsList);
        }

        // GET: Ingredients/Edit/5
        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ingredient = await _context.Ingredients.FindAsync(id);
            if (ingredient == null)
            {
                return NotFound();
            }

            ViewBag.Categories = _context.Categories
                .OrderBy(c => c.CategoryName)
                .ToList();

            return View(ingredient);
        }

    }
}