using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using RecipeManager.Data;
using RecipesVault.Models;

namespace RecipesVault.Services
{
    public class BrevoNewsletterService : INewsletterService
    {
        private readonly RecipeDbContext _context;
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _senderEmail;
        private readonly string _senderName;
        private readonly string _baseUrl;

        public BrevoNewsletterService(RecipeDbContext context, IConfiguration configuration)
        {
            _context = context;
            _httpClient = new HttpClient();

            _apiKey = configuration["Brevo:ApiKey"];
            _senderEmail = configuration["Brevo:SenderEmail"];
            _senderName = configuration["Brevo:SenderName"] ?? "RecipesVault";
            _baseUrl = configuration["AppSettings:BaseUrl"] ?? "https://recipesvault.org";

            _httpClient.DefaultRequestHeaders.Add("api-key", _apiKey);
        }

        public async Task<bool> SubscribeAsync(string email, string? userId = null)
        {
            try
            {
                // Check if already subscribed
                var existing = await _context.NewsletterSubscribers
                    .FirstOrDefaultAsync(s => s.Email == email);

                if (existing != null)
                {
                    // If previously unsubscribed, reactivate
                    if (!existing.IsActive)
                    {
                        existing.IsActive = true;
                        existing.SubscribedDate = DateTime.UtcNow;
                        await _context.SaveChangesAsync();
                        await SendWelcomeEmailAsync(email);
                        return true;
                    }
                    return false; // Already subscribed
                }

                // Create new subscriber
                var subscriber = new NewsletterSubscriber
                {
                    Email = email,
                    UserID = userId,
                    SubscribedDate = DateTime.UtcNow,
                    IsActive = true,
                    UnsubscribeToken = GenerateUnsubscribeToken()
                };

                _context.NewsletterSubscribers.Add(subscriber);
                await _context.SaveChangesAsync();

                // Send welcome email
                await SendWelcomeEmailAsync(email);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UnsubscribeAsync(string token)
        {
            try
            {
                var subscriber = await _context.NewsletterSubscribers
                    .FirstOrDefaultAsync(s => s.UnsubscribeToken == token);

                if (subscriber == null)
                    return false;

                subscriber.IsActive = false;
                await _context.SaveChangesAsync();

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> IsSubscribedAsync(string email)
        {
            return await _context.NewsletterSubscribers
                .AnyAsync(s => s.Email == email && s.IsActive);
        }

        public async Task<int> GetSubscriberCountAsync()
        {
            return await _context.NewsletterSubscribers
                .CountAsync(s => s.IsActive);
        }

        public async Task SyncUserNewsletterPreferenceAsync(string userId, string email, bool isSubscribed)
        {
            var existingSubscriber = await _context.NewsletterSubscribers
                .FirstOrDefaultAsync(s => s.Email == email);

            if (isSubscribed)
            {
                // User wants to be subscribed
                if (existingSubscriber == null)
                {
                    // Create new subscriber
                    var subscriber = new NewsletterSubscriber
                    {
                        Email = email,
                        UserID = userId,
                        SubscribedDate = DateTime.UtcNow,
                        IsActive = true,
                        UnsubscribeToken = GenerateUnsubscribeToken()
                    };

                    _context.NewsletterSubscribers.Add(subscriber);
                    await _context.SaveChangesAsync();

                    // Send welcome email
                    await SendWelcomeEmailAsync(email);
                }
                else if (!existingSubscriber.IsActive)
                {
                    // Reactivate existing subscriber
                    existingSubscriber.IsActive = true;
                    existingSubscriber.UserID = userId;
                    existingSubscriber.SubscribedDate = DateTime.UtcNow;
                    await _context.SaveChangesAsync();

                    // Send welcome email
                    await SendWelcomeEmailAsync(email);
                }
                else if (string.IsNullOrEmpty(existingSubscriber.UserID))
                {
                    // Link existing subscriber to user account
                    existingSubscriber.UserID = userId;
                    await _context.SaveChangesAsync();
                }
            }
            else
            {
                // User doesn't want to be subscribed
                if (existingSubscriber != null && existingSubscriber.IsActive)
                {
                    existingSubscriber.IsActive = false;
                    await _context.SaveChangesAsync();
                }
            }
        }

        public async Task SendNewsletterAsync(string subject, string htmlContent)
        {
            var subscribers = await _context.NewsletterSubscribers
                .Where(s => s.IsActive)
                .ToListAsync();

            foreach (var subscriber in subscribers)
            {
                await SendEmailAsync(
                    subscriber.Email,
                    subject,
                    AddUnsubscribeLink(htmlContent, subscriber.UnsubscribeToken)
                );

                // Update last email sent
                subscriber.LastEmailSent = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }

        public async Task SendWelcomeEmailAsync(string email)
        {
            var subscriber = await _context.NewsletterSubscribers
                .FirstOrDefaultAsync(s => s.Email == email);

            if (subscriber == null)
                return;

            var htmlContent = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0; }}
        .content {{ background: white; padding: 30px; border: 1px solid #ddd; border-top: none; }}
        .footer {{ background: #f8f9fa; padding: 20px; text-align: center; font-size: 12px; color: #666; border-radius: 0 0 10px 10px; }}
        .button {{ display: inline-block; padding: 12px 30px; background: #667eea; color: white; text-decoration: none; border-radius: 5px; margin: 20px 0; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Welcome to RecipesVault Newsletter!</h1>
        </div>
        <div class='content'>
            <h2>Thanks for subscribing!</h2>
            <p>We're excited to have you join our community of food enthusiasts. You'll now receive:</p>
            <ul>
                <li>Weekly featured recipes from around the world</li>
                <li>Seasonal cooking tips and ingredient spotlights</li>
                <li>Quick meal ideas for busy weeknights</li>
                <li>New recipe additions to our collection</li>
            </ul>
            <p>Get started by exploring our recipe collection:</p>
            <a href='{_baseUrl}' class='button'>Browse Recipes</a>
        </div>
        <div class='footer'>
            <p>You're receiving this because you subscribed to RecipesVault newsletter.</p>
            <p><a href='{_baseUrl}/newsletter/unsubscribe?token={subscriber.UnsubscribeToken}'>Unsubscribe</a></p>
        </div>
    </div>
</body>
</html>";

            await SendEmailAsync(email, "Welcome to RecipesVault!", htmlContent);
        }

        private async Task SendEmailAsync(string recipientEmail, string subject, string htmlContent)
        {
            try
            {
                var emailData = new
                {
                    sender = new { name = _senderName, email = _senderEmail },
                    to = new[] { new { email = recipientEmail } },
                    subject = subject,
                    htmlContent = htmlContent
                };

                var response = await _httpClient.PostAsJsonAsync(
                    "https://api.brevo.com/v3/smtp/email",
                    emailData
                );

                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                // Log error (you might want to add logging here)
                Console.WriteLine($"Error sending email to {recipientEmail}: {ex.Message}");
            }
        }

        private string AddUnsubscribeLink(string htmlContent, string token)
        {
            var unsubscribeLink = $@"
<div style='margin-top: 40px; padding-top: 20px; border-top: 1px solid #ddd; text-align: center; font-size: 12px; color: #666;'>
    <p>You're receiving this because you subscribed to RecipesVault newsletter.</p>
    <p><a href='{_baseUrl}/newsletter/unsubscribe?token={token}' style='color: #667eea;'>Unsubscribe</a></p>
</div>";

            return htmlContent + unsubscribeLink;
        }

        private string GenerateUnsubscribeToken()
        {
            return Guid.NewGuid().ToString("N"); // Returns 32-character hex string
        }
    }
}