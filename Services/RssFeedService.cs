using Microsoft.EntityFrameworkCore;
using System.ServiceModel.Syndication;
using System.Xml;

public class RssFeedService
{
    private readonly NewsDbContext _db;
    private readonly ILogger<RssFeedService> _logger;

    public RssFeedService(NewsDbContext db, ILogger<RssFeedService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task LoadRssFeedsAsync()
    {
        var feeds = new[]
        {
            ("KrebsOnSecurity", "https://krebsonsecurity.com/feed/"),
            ("TheHackerNews", "https://feeds.feedburner.com/TheHackersNews?format=xml"),
            ("SecureList", "https://securelist.com/feed/"),
            ("0dayfans", "https://0dayfans.com/feed.rss")
        };

        foreach (var (source, url) in feeds)
        {
            try
            {
                _logger.LogInformation($"Loading RSS feed from {source}");
                using var reader = XmlReader.Create(url);
                var feed = SyndicationFeed.Load(reader);
                if (feed == null) continue;

                var newItemsCount = 0;
                foreach (var item in feed.Items)
                {
                    var link = item.Links.FirstOrDefault()?.Uri.ToString();
                    
                    // Check if already exists
                    if (!await _db.NewsItems.AnyAsync(n => n.Link == link))
                    {
                        _db.NewsItems.Add(new NewsItem
                        {
                            Title = item.Title?.Text ?? "No Title",
                            Link = link,
                            Source = source,
                            PublishDate = item.PublishDate,
                            Summary = item.Summary?.Text,
                            Content = item.Content is TextSyndicationContent textContent ? textContent.Text : string.Empty
                        });
                        newItemsCount++;
                    }
                }
                
                _logger.LogInformation($"Added {newItemsCount} new items from {source}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading feed {source}: {ex.Message}");
            }
        }

        await _db.SaveChangesAsync();
        _logger.LogInformation("RSS feed loading completed");
    }
}