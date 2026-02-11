using System;
using System.Collections.Generic;

namespace RecipeManager.Models.ViewModels
{
    public class WeeklyMealPlanViewModel
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        // Dictionary: Key = Date, Value = List of meal plan details for that day
        public Dictionary<DateTime, List<MealPlanDetail>> MealsByDay { get; set; }

        // List of all available recipes for selection
        public List<Recipe> AvailableRecipes { get; set; }

        public WeeklyMealPlanViewModel()
        {
            MealsByDay = new Dictionary<DateTime, List<MealPlanDetail>>();
            AvailableRecipes = new List<Recipe>();
        }
    }

    public class MealPlanDetail
    {
        public int MealPlanID { get; set; }
        public Recipe Recipe { get; set; }
    }
}