using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace TennisClubRanking.Services
{
    public class PromotionRelegationBackgroundService : BackgroundService
    {
        private readonly ILogger<PromotionRelegationBackgroundService> _logger;
        private readonly IServiceProvider _serviceProvider;

        public PromotionRelegationBackgroundService(
            ILogger<PromotionRelegationBackgroundService> logger,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var promotionService = scope.ServiceProvider.GetRequiredService<PromotionRelegationService>();
                        
                        if (await promotionService.IsPromotionRelegationDue())
                        {
                            _logger.LogInformation("Processing promotions and relegations for the new season");
                            await promotionService.ProcessPromotionsAndRelegations();
                            _logger.LogInformation("Completed processing promotions and relegations");
                        }
                    }

                    // Check once per day
                    await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while processing promotions and relegations");
                    // Wait for an hour before retrying after an error
                    await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
                }
            }
        }
    }
}
