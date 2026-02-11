using System.ComponentModel.DataAnnotations;

namespace RecipeManager.Models
{
    public class Ingredient
    {
        public int IngredientID { get; set; }

        [Required(ErrorMessage = "Ingredient name is required")]
        public string IngredientName { get; set; }

        public int CategoryID { get; set; }

        public Category? Category { get; set; }
    }
}
