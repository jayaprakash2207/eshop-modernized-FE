using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PlatformApp.Application.Loyalty;

namespace PlatformApp.Infrastructure;

public class LoyaltyExpiryService : BackgroundService
{
    private readonly ILogger<LoyaltyExpiryService> _logger;
    private readonly PlatformApp.Application.Loyalty.ILoyaltyRepository _repository;
    private readonly TimeSpan _interval = TimeSpan.FromMinutes(15);

    public LoyaltyExpiryService(ILogger<LoyaltyExpiryService> logger, PlatformApp.Application.Loyalty.ILoyaltyRepository repository)
    {
        _logger = logger;
        _repository = repository;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("LoyaltyExpiryService started.");
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await _repository.ExpirePointsAsync(DateTimeOffset.UtcNow, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while expiring loyalty points");
            }

            await Task.Delay(_interval, stoppingToken);
        }
    }
}
