namespace RecipeManager.DTOs
{
    public class RecipeIngredientDto
    {
        public int IngredientID { get; set; }
        public decimal Quantity { get; set; }
        public int UnitID { get; set; }
        public string? Notes { get; set; }
    }
}