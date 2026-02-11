using System.Threading.Tasks;

namespace RecipesVault.Services
{
    public interface INewsletterService
    {
        Task<bool> SubscribeAsync(string email, string? userId = null);
        Task<bool> UnsubscribeAsync(string token);
        Task<bool> IsSubscribedAsync(string email);
        Task SendNewsletterAsync(string subject, string htmlContent);
        Task SendWelcomeEmailAsync(string email);
        Task<int> GetSubscriberCountAsync();

        // New method to sync user account with newsletter subscription
        Task SyncUserNewsletterPreferenceAsync(string userId, string email, bool isSubscribed);

    }
}