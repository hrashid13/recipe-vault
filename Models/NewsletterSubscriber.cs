using System;
using System.ComponentModel.DataAnnotations;

namespace RecipesVault.Models
{
    public class NewsletterSubscriber
    {
        [Key]
        public int SubscriberID { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(255)]
        public string Email { get; set; }

        [StringLength(450)]
        public string? UserID { get; set; }

        public DateTime SubscribedDate { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;

        [StringLength(100)]
        public string UnsubscribeToken { get; set; }

        public DateTime? LastEmailSent { get; set; }
    }
}
