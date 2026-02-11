using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RecipeManager.Models
{
    public class ShoppingList
    {
        [Key]
        public int ShoppingListID { get; set; }

        [Required]
        public int UserID { get; set; }  // Changed from string to int

        [Required]
        [StringLength(200)]
        public string ListName { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        [Required]
        public DateTime DateCreated { get; set; } = DateTime.Now;

        public bool IsCompleted { get; set; } = false;

        // Navigation property
        public virtual ICollection<ShoppingListItem> ShoppingListItems { get; set; }
    }
}