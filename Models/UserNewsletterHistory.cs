using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RecipeManager.Models
{
   
    public class UserNewsletterHistory
    {
        [Key]
        public int HistoryID { get; set; }

        [Required]
        public int UserID { get; set; }

        [Required]
        public int NewsletterID { get; set; }

        public DateTime SentDate { get; set; } = DateTime.UtcNow;

        public bool WasOpened { get; set; } = false;

        public DateTime? OpenedDate { get; set; }

        // Navigation properties
        [ForeignKey("UserID")]
        public virtual User User { get; set; }

        [ForeignKey("NewsletterID")]
        public virtual NewsletterLog Newsletter { get; set; }
    }
}