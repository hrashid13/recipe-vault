using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecipeManager.Data;
using RecipeManager.Models;
using RecipeManager.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace RecipeManager.Controllers
{
    [Authorize]
    public class MealPlannerController : Controller
    {
        private readonly RecipeDbContext _context;

        public MealPlannerController(RecipeDbContext context)
        {
            _context = context;
        }

        // Helper method to get integer UserID from GoogleID claim
        private async Task<int?> GetUserIdAsync()
        {
            var googleId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(googleId))
                return null;

            var user = await _context.Users
                .Where(u => u.GoogleID == googleId)
                .Select(u => u.UserID)
                .FirstOrDefaultAsync();

            return user;
        }

        // GET: MealPlanner/WeeklyPlan
        public async Task<IActionResult> WeeklyPlan(DateTime? startDate)
        {
            var userId = await GetUserIdAsync();
            if (userId == null)
                return Unauthorized();

            // Default to current week (Monday to Sunday)
            var weekStart = startDate ?? GetMondayOfCurrentWeek();
            var weekEnd = weekStart.AddDays(6);

            var model = new WeeklyMealPlanViewModel
            {
                StartDate = weekStart,
                EndDate = weekEnd
            };

            // Get user's meal plans for this week
            var mealPlans = await _context.UserMealPlans
                .Include(mp => mp.Recipe)
                    .ThenInclude(r => r.Cuisine)
                .Where(mp => mp.UserID == userId
                    && mp.PlannedDate >= weekStart
                    && mp.PlannedDate <= weekEnd)
                .ToListAsync();

            // Organize meals by day
            for (int i = 0; i < 7; i++)
            {
                var currentDay = weekStart.AddDays(i);
                var mealsForDay = mealPlans
                    .Where(mp => mp.PlannedDate.Date == currentDay.Date)
                    .Select(mp => new MealPlanDetail
                    {
                        MealPlanID = mp.MealPlanID,
                        Recipe = mp.Recipe
                    })
                    .ToList();

                model.MealsByDay[currentDay] = mealsForDay;
            }

            // Get all available recipes for selection
            model.AvailableRecipes = await _context.Recipes
                .Include(r => r.Cuisine)
                .OrderBy(r => r.RecipeName)
                .ToListAsync();

            return View(model);
        }

        // POST: MealPlanner/AddMealToDay
        [HttpPost]
        public async Task<IActionResult> AddMealToDay(int recipeId, DateTime plannedDate)
        {
            var userId = await GetUserIdAsync();
            if (userId == null)
                return Unauthorized();

            var mealPlan = new UserMealPlan
            {
                UserID = userId.Value,
                RecipeID = recipeId,
                PlannedDate = plannedDate.Date,
                DateCreated = DateTime.Now
            };

            _context.UserMealPlans.Add(mealPlan);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(WeeklyPlan), new { startDate = GetMondayOfWeek(plannedDate) });
        }

        // POST: MealPlanner/RemoveMealFromDay
        [HttpPost]
        public async Task<IActionResult> RemoveMealFromDay(int mealPlanId, DateTime startDate)
        {
            var userId = await GetUserIdAsync();
            if (userId == null)
                return Unauthorized();

            var mealPlan = await _context.UserMealPlans
                .FirstOrDefaultAsync(mp => mp.MealPlanID == mealPlanId && mp.UserID == userId.Value);

            if (mealPlan != null)
            {
                _context.UserMealPlans.Remove(mealPlan);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(WeeklyPlan), new { startDate });
        }

        // POST: MealPlanner/GenerateShoppingList
        [HttpPost]
        public async Task<IActionResult> GenerateShoppingList(DateTime startDate, DateTime endDate)
        {
            var userId = await GetUserIdAsync();
            if (userId == null)
                return Unauthorized();

            // Get all meal plans for the week
            var mealPlans = await _context.UserMealPlans
                .Where(mp => mp.UserID == userId.Value
                    && mp.PlannedDate >= startDate
                    && mp.PlannedDate <= endDate)
                .Select(mp => mp.RecipeID)
                .ToListAsync();

            if (!mealPlans.Any())
            {
                TempData["ErrorMessage"] = "No meals planned for this week. Please add some recipes first.";
                return RedirectToAction(nameof(WeeklyPlan), new { startDate });
            }

            // Aggregate ingredients across all recipes
            var aggregatedIngredients = await _context.RecipeIngredients
                .Where(ri => mealPlans.Contains(ri.RecipeID))
                .GroupBy(ri => new { ri.IngredientID, ri.UnitID })
                .Select(g => new
                {
                    IngredientID = g.Key.IngredientID,
                    UnitID = g.Key.UnitID,
                    TotalQuantity = g.Sum(ri => ri.Quantity),
                    CategoryID = g.First().Ingredient.CategoryID
                })
                .ToListAsync();

            // Create shopping list
            var shoppingList = new ShoppingList
            {
                UserID = userId.Value,
                ListName = $"Week of {startDate:MMM dd} - {endDate:MMM dd}",
                StartDate = startDate,
                EndDate = endDate,
                DateCreated = DateTime.Now,
                IsCompleted = false
            };

            _context.ShoppingLists.Add(shoppingList);
            await _context.SaveChangesAsync();

            // Add items to shopping list
            foreach (var item in aggregatedIngredients)
            {
                var shoppingListItem = new ShoppingListItem
                {
                    ShoppingListID = shoppingList.ShoppingListID,
                    IngredientID = item.IngredientID,
                    TotalQuantity = item.TotalQuantity,
                    UnitID = item.UnitID,
                    CategoryID = item.CategoryID,
                    IsChecked = false
                };

                _context.ShoppingListItems.Add(shoppingListItem);
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(ViewShoppingList), new { id = shoppingList.ShoppingListID });
        }

        // GET: MealPlanner/ViewShoppingList/5
        public async Task<IActionResult> ViewShoppingList(int id)
        {
            var userId = await GetUserIdAsync();
            if (userId == null)
                return Unauthorized();

            var shoppingList = await _context.ShoppingLists
                .FirstOrDefaultAsync(sl => sl.ShoppingListID == id && sl.UserID == userId.Value);

            if (shoppingList == null)
            {
                return NotFound();
            }

            var items = await _context.ShoppingListItems
                .Include(sli => sli.Ingredient)
                .Include(sli => sli.Unit)
                .Include(sli => sli.Category)
                .Where(sli => sli.ShoppingListID == id)
                .OrderBy(sli => sli.Category.CategoryName)
                .ThenBy(sli => sli.Ingredient.IngredientName)
                .ToListAsync();

            var model = new ShoppingListViewModel
            {
                ShoppingListID = shoppingList.ShoppingListID,
                ListName = shoppingList.ListName,
                StartDate = shoppingList.StartDate,
                EndDate = shoppingList.EndDate,
                DateCreated = shoppingList.DateCreated,
                IsCompleted = shoppingList.IsCompleted
            };

            // Group items by category
            var groupedItems = items.GroupBy(i => i.Category.CategoryName);

            foreach (var group in groupedItems)
            {
                var itemList = group.Select(item => new ShoppingListItemViewModel
                {
                    ShoppingListItemID = item.ShoppingListItemID,
                    IngredientName = item.Ingredient.IngredientName,
                    TotalQuantity = item.TotalQuantity,
                    UnitName = item.Unit.UnitName,
                    IsChecked = item.IsChecked,
                    CategoryID = item.CategoryID
                }).ToList();

                model.ItemsByCategory[group.Key] = itemList;
            }

            return View(model);
        }

        // POST: MealPlanner/ToggleItemCheck
        [HttpPost]
        public async Task<IActionResult> ToggleItemCheck(int itemId)
        {
            var userId = await GetUserIdAsync();
            if (userId == null)
                return Unauthorized();

            var item = await _context.ShoppingListItems
                .Include(sli => sli.ShoppingList)
                .FirstOrDefaultAsync(sli => sli.ShoppingListItemID == itemId
                    && sli.ShoppingList.UserID == userId.Value);

            if (item != null)
            {
                item.IsChecked = !item.IsChecked;
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(ViewShoppingList), new { id = item.ShoppingListID });
        }

        // GET: MealPlanner/MyShoppingLists
        public async Task<IActionResult> MyShoppingLists()
        {
            var userId = await GetUserIdAsync();
            if (userId == null)
                return Unauthorized();

            var shoppingLists = await _context.ShoppingLists
                .Where(sl => sl.UserID == userId.Value)
                .OrderByDescending(sl => sl.DateCreated)
                .ToListAsync();

            return View(shoppingLists);
        }

        // POST: MealPlanner/DeleteShoppingList
        [HttpPost]
        public async Task<IActionResult> DeleteShoppingList(int id)
        {
            var userId = await GetUserIdAsync();
            if (userId == null)
                return Unauthorized();

            var shoppingList = await _context.ShoppingLists
                .FirstOrDefaultAsync(sl => sl.ShoppingListID == id && sl.UserID == userId.Value);

            if (shoppingList != null)
            {
                _context.ShoppingLists.Remove(shoppingList);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(MyShoppingLists));
        }

        // Helper methods
        private DateTime GetMondayOfCurrentWeek()
        {
            var today = DateTime.Today;
            var dayOfWeek = (int)today.DayOfWeek;
            var daysUntilMonday = dayOfWeek == 0 ? -6 : 1 - dayOfWeek;
            return today.AddDays(daysUntilMonday);
        }

        private DateTime GetMondayOfWeek(DateTime date)
        {
            var dayOfWeek = (int)date.DayOfWeek;
            var daysUntilMonday = dayOfWeek == 0 ? -6 : 1 - dayOfWeek;
            return date.AddDays(daysUntilMonday);
        }
    }
}