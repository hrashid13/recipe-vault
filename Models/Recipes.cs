namespace RecipeManager.Models
{
    public class Recipe
    {
        public int RecipeID { get; set; }
        public string? RecipeName { get; set; }
        public string? Description { get; set; }
        public int PrepTime { get; set; }
        public int CookTime { get; set; }
        public int Servings { get; set; }
        public string? DifficultyLevel { get; set; }
        public int CuisineID { get; set; }
        public DateTime DateAdded { get; set; }

        public Cuisine? Cuisine { get; set; }
        public ICollection<Instruction>? Instructions { get; set; }
        public ICollection<RecipeIngredient>? RecipeIngredients { get; set; }
        public ICollection<RecipeTag>? RecipeTags { get; set; }
    }
}