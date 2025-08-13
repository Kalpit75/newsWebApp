using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public class BackgroundRssFeedService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<BackgroundRssFeedService> _logger;

    public BackgroundRssFeedService(IServiceProvider serviceProvider, ILogger<BackgroundRssFeedService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Run immediately on startup
        await UpdateRssFeeds();
        
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken); // Run every 30 minutes
                await UpdateRssFeeds();
            }
            catch (OperationCanceledException)
            {
                // Expected when cancellation is requested
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in background RSS feed service");
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken); // Wait before retrying
            }
        }
    }

    private async Task UpdateRssFeeds()
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var rssFeedService = scope.ServiceProvider.GetRequiredService<RssFeedService>();
            await rssFeedService.LoadRssFeedsAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update RSS feeds");
        }
    }
}