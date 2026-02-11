using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RecipeManager.Models
{
    public class ShoppingListItem
    {
        [Key]
        public int ShoppingListItemID { get; set; }

        [Required]
        public int ShoppingListID { get; set; }

        [Required]
        public int IngredientID { get; set; }

        [Required]
        [Column(TypeName = "decimal(8,2)")]
        public decimal TotalQuantity { get; set; }

        [Required]
        public int UnitID { get; set; }

        [Required]
        public int CategoryID { get; set; }

        public bool IsChecked { get; set; } = false;

        // Navigation properties
        [ForeignKey("ShoppingListID")]
        public virtual ShoppingList ShoppingList { get; set; }

        [ForeignKey("IngredientID")]
        public virtual Ingredient Ingredient { get; set; }

        [ForeignKey("UnitID")]
        public virtual Unit Unit { get; set; }

        [ForeignKey("CategoryID")]
        public virtual Category Category { get; set; }
    }
}