using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ServiceModel.Syndication;
using System.Xml;

namespace newsWebApp.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public List<NewsItem> News { get; private set; } = new();

        public void OnGet()
        {
            var feeds = new[]
            {
                ("KrebsOnSecurity", "https://krebsonsecurity.com/feed/"),
                ("Threatpost", "https://threatpost.com/feed/"),
                ("The Hacker News", "https://feeds.feedburner.com/TheHackersNews")
            };

            var allNews = new List<NewsItem>();
            foreach (var (source, url) in feeds)
            {
                using var reader = XmlReader.Create(url);
                var feed = SyndicationFeed.Load(reader);
                if (feed == null) continue;
                allNews.AddRange(feed.Items.Select(item => new NewsItem
                {
                    Title = item.Title.Text,
                    Link = item.Links.FirstOrDefault()?.Uri.ToString(),
                    Source = source,
                    PublishDate = item.PublishDate,
                    Summary = item.Summary?.Text,
                    Content = item.Content is TextSyndicationContent textContent ? textContent.Text : null
                }));
            }

            var cutoff = DateTimeOffset.Now.AddDays(-30);
            News = allNews
                .Where(n => n.PublishDate >= cutoff)
                .OrderByDescending(n => n.PublishDate)
                .Take(15)
                .ToList();
        }
    }
}