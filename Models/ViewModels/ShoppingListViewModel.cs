using System;
using System.Collections.Generic;

namespace RecipeManager.Models.ViewModels
{
    public class ShoppingListViewModel
    {
        public int ShoppingListID { get; set; }
        public string ListName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime DateCreated { get; set; }
        public bool IsCompleted { get; set; }

        // Dictionary: Key = Category Name, Value = List of items in that category
        public Dictionary<string, List<ShoppingListItemViewModel>> ItemsByCategory { get; set; }

        public ShoppingListViewModel()
        {
            ItemsByCategory = new Dictionary<string, List<ShoppingListItemViewModel>>();
        }
    }

    public class ShoppingListItemViewModel
    {
        public int ShoppingListItemID { get; set; }
        public string IngredientName { get; set; }
        public decimal TotalQuantity { get; set; }
        public string UnitName { get; set; }
        public bool IsChecked { get; set; }
        public int CategoryID { get; set; }
    }
}