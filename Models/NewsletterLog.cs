using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RecipeManager.Models
{
   
    public class NewsletterLog
    {
        [Key]
        public int NewsletterID { get; set; }

        [Required]
        public int RecipeID { get; set; }

        public DateTime SentDate { get; set; } = DateTime.UtcNow;

        [StringLength(200)]
        public string SubjectLine { get; set; }

        public int RecipientCount { get; set; }

        // Navigation properties
        [ForeignKey("RecipeID")]
        public virtual Recipe Recipe { get; set; }

        public virtual ICollection<UserNewsletterHistory> UserHistories { get; set; }
    }
}