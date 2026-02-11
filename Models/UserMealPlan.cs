using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RecipeManager.Models
{
    public class UserMealPlan
    {
        [Key]
        public int MealPlanID { get; set; }

        [Required]
        public int UserID { get; set; }  // Changed from string to int

        [Required]
        public int RecipeID { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime PlannedDate { get; set; }

        [Required]
        public DateTime DateCreated { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey("RecipeID")]
        public virtual Recipe Recipe { get; set; }
    }
}