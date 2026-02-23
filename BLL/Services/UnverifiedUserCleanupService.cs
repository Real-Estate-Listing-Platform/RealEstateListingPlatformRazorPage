using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BLL.Services
{
    public class UnverifiedUserCleanupService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<UnverifiedUserCleanupService> _logger;

        public UnverifiedUserCleanupService(IServiceProvider serviceProvider, ILogger<UnverifiedUserCleanupService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Unverified User Cleanup Service running.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
                        var threshold = DateTime.UtcNow.AddMinutes(-30);
                        var deletedCount = await userService.CleanupUnverifiedUsersOlderThanAsync(threshold, stoppingToken);
                        if (deletedCount > 0)
                            _logger.LogInformation("Cleaned up {Count} unverified users.", deletedCount);
                    }
                }
                catch (OperationCanceledException)
                {
                    // Expected when the application is shutting down
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred executing cleanup.");
                }

                try
                {
                    await Task.Delay(TimeSpan.FromMinutes(29), stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    // Expected when the application is shutting down
                    break;
                }
            }

            _logger.LogInformation("Unverified User Cleanup Service stopped.");
        }
    }
}
