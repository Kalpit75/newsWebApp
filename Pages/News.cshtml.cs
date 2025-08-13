using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace newsWebApp.Pages
{
    public class NewsModel : PageModel
    {
        private readonly NewsDbContext _db;
        private readonly ILogger<NewsModel> _logger;

        public NewsModel(NewsDbContext db, ILogger<NewsModel> logger)
        {
            _db = db;
            _logger = logger;
        }

        public List<NewsItem> News { get; private set; } = new();
        public bool HasMoreNews { get; private set; }

        [BindProperty(SupportsGet = true)]
        public int PageSize { get; set; } = 10;

        [BindProperty(SupportsGet = true)]
        public int Skip { get; set; } = 0;

        public async Task OnGetAsync()
        {
            // Only load news from database - no RSS fetching during page load
            var cutoff = DateTimeOffset.Now.AddDays(-30);

            var totalCount = await _db.NewsItems
                .Where(n => n.PublishDate >= cutoff)
                .CountAsync();

            News = await _db.NewsItems
                .Where(n => n.PublishDate >= cutoff)
                .OrderByDescending(n => n.PublishDate)
                .Skip(Skip)
                .Take(PageSize)
                .ToListAsync();

            HasMoreNews = (Skip + PageSize) < totalCount;

            _logger.LogInformation($"Loaded {News.Count} news items, Skip: {Skip}, HasMore: {HasMoreNews}");
        }

        public async Task<IActionResult> OnPostAsync()
        {
            _logger.LogInformation($"OnPostAsync called with Skip: {Skip}, PageSize: {PageSize}");

            await OnGetAsync();

            _logger.LogInformation($"Returning {News.Count} news items from OnPostAsync");

            return Partial("_NewsItemsPartial", News);
        }
    }
}
