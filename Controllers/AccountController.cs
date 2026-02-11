using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecipeManager.Data;
using RecipeManager.Models;
using RecipesVault.Services;
using System.Security.Claims;

namespace RecipeManager.Controllers
{
    public class AccountController : Controller
    {
        private readonly RecipeDbContext _context;
        private readonly INewsletterService _newsletterService;

        public AccountController(RecipeDbContext context, INewsletterService newsletterService)
        {
            _context = context;
            _newsletterService = newsletterService;
        }

        [HttpGet]
        public IActionResult TestDatabase()
        {
            try
            {
                // Test 1: Can we access the database at all?
                var recipeCount = _context.Recipes.Count();
                ViewBag.RecipeCount = recipeCount;

                // Test 2: Can we see the Users DbSet?
                var userDbSet = _context.Users;
                ViewBag.UsersDbSetExists = userDbSet != null;

                // Test 3: Try raw SQL
                var userCountRaw = _context.Database.ExecuteSqlRaw("SELECT COUNT(*) FROM users");
                ViewBag.UserCountRaw = userCountRaw;

                // Test 4: Try to query users
                var userCount = _context.Users.Count();
                ViewBag.UserCount = userCount;

                return Content($"Recipe count: {recipeCount}, Users DbSet: {userDbSet != null}, User count: {userCount}");
            }
            catch (Exception ex)
            {
                return Content($"Error: {ex.Message}\n\nInner: {ex.InnerException?.Message}\n\nStack: {ex.StackTrace}");
            }
        }

        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpGet]
        public IActionResult GoogleLogin(string returnUrl = null, bool newsletter = false)
        {
            var properties = new AuthenticationProperties
            {
                RedirectUri = Url.Action("GoogleResponse", new { returnUrl, newsletter }),
                Items = { { "newsletter", newsletter.ToString() } }
            };
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        [HttpGet]
        public async Task<IActionResult> GoogleResponse(string returnUrl = null, bool newsletter = false)
        {
            var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            if (!result.Succeeded)
            {
                return RedirectToAction("Login");
            }

            var claims = result.Principal.Identities.FirstOrDefault()?.Claims;
            var googleId = claims?.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            var email = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            var name = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
            var picture = claims?.FirstOrDefault(c => c.Type == "picture")?.Value;

            if (string.IsNullOrEmpty(googleId) || string.IsNullOrEmpty(email))
            {
                return RedirectToAction("Login");
            }

            // Check if user exists - use FromSqlRaw to bypass EF mapping issues
            var user = await _context.Users
                .FromSqlRaw("SELECT * FROM users WHERE googleid = {0}", googleId)
                .FirstOrDefaultAsync();

            if (user == null)
            {
                // Create new user
                user = new User
                {
                    GoogleID = googleId,
                    Email = email,
                    DisplayName = name,
                    ProfilePictureUrl = picture,
                    DateJoined = DateTime.UtcNow,
                    LastLogin = DateTime.UtcNow,
                    IsNewsletterSubscribed = newsletter,
                    IsActive = true
                };
                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Sync newsletter preference for new user
                await _newsletterService.SyncUserNewsletterPreferenceAsync(
                    user.UserID.ToString(),
                    user.Email,
                    user.IsNewsletterSubscribed
                );
            }
            else
            {
                // Update existing user
                user.LastLogin = DateTime.UtcNow;
                user.DisplayName = name;
                user.ProfilePictureUrl = picture;

                // Update newsletter preference if they opted in during this login
                if (newsletter && !user.IsNewsletterSubscribed)
                {
                    user.IsNewsletterSubscribed = true;
                }

                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                // Sync newsletter preference for existing user
                await _newsletterService.SyncUserNewsletterPreferenceAsync(
                    user.UserID.ToString(),
                    user.Email,
                    user.IsNewsletterSubscribed
                );
            }

            // ===== NEW CODE STARTS HERE =====
            // Create claims for the authenticated user
            var userClaims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.GoogleID),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.DisplayName ?? user.Email)
            };

            // Add admin role if user is admin
            if (user.IsAdmin)
            {
                userClaims.Add(new Claim(ClaimTypes.Role, "Admin"));
            }

            var claimsIdentity = new ClaimsIdentity(userClaims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(30)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);
            // ===== NEW CODE ENDS HERE =====

            return Redirect(returnUrl ?? "/");
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        public IActionResult Profile()
        {
            var googleId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = _context.Users.FirstOrDefault(u => u.GoogleID == googleId);

            if (user == null)
            {
                return RedirectToAction("Login");
            }

            return View(user);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> UpdateNewsletter(bool subscribe)
        {
            var googleId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = _context.Users.FirstOrDefault(u => u.GoogleID == googleId);

            if (user != null)
            {
                user.IsNewsletterSubscribed = subscribe;
                await _context.SaveChangesAsync();

                // Sync newsletter preference
                await _newsletterService.SyncUserNewsletterPreferenceAsync(
                    user.UserID.ToString(),
                    user.Email,
                    user.IsNewsletterSubscribed
                );

                TempData["SuccessMessage"] = subscribe
                    ? "You've been subscribed to the newsletter!"
                    : "You've been unsubscribed from the newsletter.";
            }

            return RedirectToAction("Profile");
        }
    }
}