using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using RecipeManager.Data;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RecipeManager.Services
{
    public class DatabaseWarmupService : IHostedService, IDisposable
    {
        private Timer _timer;
        private readonly IServiceProvider _serviceProvider;

        public DatabaseWarmupService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            // Warm up immediately on startup
            DoWork(null);

            // Then keep warm every 4 minutes
            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromMinutes(4));
            return Task.CompletedTask;
        }

        private void DoWork(object state)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<RecipeDbContext>();

                // Simple query to keep connection warm
                _ = context.Recipes.Take(1).ToList();
            }
            catch (Exception ex)
            {
                // Log but don't crash
                Console.WriteLine($"Warmup query failed: {ex.Message}");
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}