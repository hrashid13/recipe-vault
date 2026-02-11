namespace RecipeManager.Models
{
    public class RecipeIngredient
    {
        public int RecipeIngredientID { get; set; }
        public decimal Quantity { get; set; }
        public string? Notes { get; set; }
        public int UnitID {  get; set; }
        public int RecipeID { get; set; }
        public int IngredientID { get; set; }
        public Recipe? Recipe { get; set; }
        public Ingredient? Ingredient { get; set; }
        public Unit? Unit { get; set; }
    }
}
