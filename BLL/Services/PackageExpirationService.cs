using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BLL.Services
{
    public class PackageExpirationService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<PackageExpirationService> _logger;
        private readonly TimeSpan _interval = TimeSpan.FromHours(1); // Run every hour

        public PackageExpirationService(
            IServiceProvider serviceProvider, 
            ILogger<PackageExpirationService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("PackageExpirationService is starting.");

            // Wait a bit before starting the first iteration
            try
            {
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // Application is shutting down before first run
                _logger.LogInformation("PackageExpirationService stopped before first execution.");
                return;
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessExpirations(stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    // Expected when the application is shutting down
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred in PackageExpirationService");
                }

                // Wait for the next interval
                try
                {
                    await Task.Delay(_interval, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    // Expected when the application is shutting down
                    break;
                }
            }

            _logger.LogInformation("PackageExpirationService is stopping.");
        }

        private async Task ProcessExpirations(CancellationToken stoppingToken)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var packageService = scope.ServiceProvider.GetRequiredService<IPackageService>();

                try
                {
                    // Expire old boosts
                    await packageService.ExpireOldBoostsAsync();

                    _logger.LogInformation(
                        "PackageExpirationService: Expiration check completed at {time}", 
                        DateTimeOffset.UtcNow);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing package expirations");
                }
            }
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("PackageExpirationService is stopping.");
            await base.StopAsync(stoppingToken);
        }
    }
}
