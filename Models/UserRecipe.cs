using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RecipeManager.Models
{
   
    public class UserRecipe
    {
        [Key]
        public int UserRecipeID { get; set; }

        [Required]
        public int UserID { get; set; }

        [Required]
        public int RecipeID { get; set; }

        public DateTime DateSaved { get; set; } = DateTime.UtcNow;

        [StringLength(500)]
        public string? Notes { get; set; }

        // Navigation properties
        [ForeignKey("UserID")]
        public virtual User User { get; set; }

        [ForeignKey("RecipeID")]
        public virtual Recipe Recipe { get; set; }
    }
}