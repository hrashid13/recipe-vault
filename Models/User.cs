using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RecipeManager.Models
{
    
    public class User
    {
        [Key]
        public int UserID { get; set; }

        [Required]
        [StringLength(255)]
        public string GoogleID { get; set; }

        [Required]
        [StringLength(255)]
        [EmailAddress]
        public string Email { get; set; }

        [StringLength(200)]
        public string? DisplayName { get; set; }

        [StringLength(500)]
        public string? ProfilePictureUrl { get; set; }

        public DateTime DateJoined { get; set; } = DateTime.UtcNow;

        public DateTime LastLogin { get; set; } = DateTime.UtcNow;

        public bool IsNewsletterSubscribed { get; set; } = false;

        public bool IsActive { get; set; } = true;
        public bool IsAdmin { get; set; } = false;
        // Navigation properties
        public virtual ICollection<UserRecipe> UserRecipes { get; set; }
        public virtual ICollection<UserNewsletterHistory> NewsletterHistory { get; set; }
    }
}
