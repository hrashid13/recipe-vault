using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using RecipeManager.Data;
using RecipeManager.Services;
using RecipesVault.Services;

// Fix PostgreSQL DateTime issue - MUST be at the very top
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

try
{
    var builder = WebApplication.CreateBuilder(args);


    builder.Services.Configure<ForwardedHeadersOptions>(options =>
    {
        options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
        options.KnownNetworks.Clear();
        options.KnownProxies.Clear();
    });

    // ========================================
    // Configuration & Logging
    // ========================================

    // Enable detailed logging for debugging
    builder.Logging.ClearProviders();
    builder.Logging.AddConsole();
    builder.Logging.SetMinimumLevel(LogLevel.Information);

    Console.WriteLine("=== Application Starting ===");
    Console.WriteLine($"Environment: {builder.Environment.EnvironmentName}");

    // ========================================
    // Database Configuration
    // ========================================

    // Get connection string from environment variable (Railway) OR appsettings.json (local)
    var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
        ?? builder.Configuration.GetConnectionString("DefaultConnection");

    Console.WriteLine($"Connection string found: {!string.IsNullOrEmpty(connectionString)}");

    if (string.IsNullOrEmpty(connectionString))
    {
        Console.WriteLine("ERROR: No database connection string found!");
        Console.WriteLine("Please set ConnectionStrings__DefaultConnection environment variable.");
        throw new InvalidOperationException("Database connection string not configured.");
    }

    // ========================================
    // Services Configuration
    // ========================================

    builder.Services.AddControllersWithViews();

    // Add DbContext with PostgreSQL
    builder.Services.AddDbContext<RecipeDbContext>(options =>
    {
        options.UseNpgsql(connectionString);

        // Enable detailed errors in development
        if (builder.Environment.IsDevelopment())
        {
            options.EnableSensitiveDataLogging();
            options.EnableDetailedErrors();
        }
    });

    // Add database warmup service to prevent cold starts
    builder.Services.AddHostedService<DatabaseWarmupService>();

    Console.WriteLine("Services configured successfully");




    // Add authentication services
    builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
    })
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.ExpireTimeSpan = TimeSpan.FromDays(30); // Stay logged in for 30 days
        options.SlidingExpiration = true;
    })
    .AddGoogle(options =>
    {
        options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
        options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
        options.SaveTokens = true;

        // Request specific scopes
        options.Scope.Add("profile");
        options.Scope.Add("email");
    });

    builder.Services.AddAuthorization();


    // Register Newsletter Service
    builder.Services.AddScoped<INewsletterService, BrevoNewsletterService>();

    // Register HttpClient for Brevo API calls
    builder.Services.AddHttpClient();


    // ========================================
    // Build Application
    // ========================================

    var app = builder.Build();

    app.UseForwardedHeaders();


    Console.WriteLine("Application built successfully");

    // ========================================
    // Test Database Connection at Startup
    // ========================================

    try
    {
        using (var scope = app.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<RecipeDbContext>();
            Console.WriteLine("Testing database connection...");

            // Simple query to test connection
            var canConnect = context.Database.CanConnect();
            Console.WriteLine($"Database connection: {(canConnect ? "SUCCESS" : "FAILED")}");

            if (canConnect)
            {
                var recipeCount = context.Recipes.Count();
                Console.WriteLine($"Database has {recipeCount} recipes");
            }
        }
    }
    catch (Exception dbEx)
    {
        Console.WriteLine("=== DATABASE CONNECTION ERROR ===");
        Console.WriteLine($"Type: {dbEx.GetType().Name}");
        Console.WriteLine($"Message: {dbEx.Message}");
        if (dbEx.InnerException != null)
        {
            Console.WriteLine($"Inner Exception: {dbEx.InnerException.Message}");
        }
        Console.WriteLine("=================================");
        // Don't crash - let the app start anyway and show errors in UI
    }

    // ========================================
    // Middleware Pipeline
    // ========================================

    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Home/Error");
        app.UseHsts();
    }
    else
    {
        app.UseDeveloperExceptionPage();
    }

    // Note: Railway handles HTTPS, 
    // app.UseHttpsRedirection();

    app.UseStaticFiles();
    app.UseRouting();

    // Authentication & Authorization 
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");

    // ========================================
    // Port Configuration for Railway
    // ========================================

    var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
    var url = $"http://0.0.0.0:{port}";

    Console.WriteLine($"=== Starting application on {url} ===");

    app.Run(url);
}
catch (Exception ex)
{
    // ========================================
    // Global Exception Handler
    // ========================================

    Console.WriteLine("=================================================");
    Console.WriteLine("=== FATAL ERROR - APPLICATION FAILED TO START ===");
    Console.WriteLine("=================================================");
    Console.WriteLine($"Exception Type: {ex.GetType().FullName}");
    Console.WriteLine($"Message: {ex.Message}");
    Console.WriteLine($"Stack Trace: {ex.StackTrace}");

    if (ex.InnerException != null)
    {
        Console.WriteLine("--- Inner Exception ---");
        Console.WriteLine($"Type: {ex.InnerException.GetType().FullName}");
        Console.WriteLine($"Message: {ex.InnerException.Message}");
        Console.WriteLine($"Stack: {ex.InnerException.StackTrace}");
    }

    Console.WriteLine("=================================================");

    // Exit with error code
    Environment.Exit(1);
}