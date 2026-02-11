namespace RecipeManager.Models
{
    public class RecipeTag
    {
        public int RecipeTagID { get; set; }
        public int RecipeID { get; set; }
        public int TagID { get; set; }

        public Recipe? Recipe { get; set; }
        public Tag? Tag { get; set; }
    }
}
