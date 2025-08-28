using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using newsWebApp.Data;

namespace newsWebApp.Pages
{
    [Authorize]
    public class NewsModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<NewsModel> _logger;

        public NewsModel(ApplicationDbContext db, ILogger<NewsModel> logger)
        {
            _db = db;
            _logger = logger;
        }

        public List<NewsItem> News { get; private set; } = new();
        public bool HasMoreNews { get; private set; }
        public List<string> Categories { get; private set; } = new(); // Add this

        [BindProperty(SupportsGet = true)]
        public int PageSize { get; set; } = 10;

        [BindProperty(SupportsGet = true)]
        public int Skip { get; set; } = 0;

        [BindProperty(SupportsGet = true)]
        public string? SelectedCategory { get; set; } // Add this

        public async Task OnGetAsync()
        {
            // Load available categories
            var cutoff = DateTimeOffset.Now.AddDays(-30);
            Categories = await _db.NewsItems
                .Where(n => n.PublishDate >= cutoff && !string.IsNullOrEmpty(n.Category))
                .Select(n => n.Category!)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();

            // Build query with category filter
            var query = _db.NewsItems.Where(n => n.PublishDate >= cutoff);
            
            if (!string.IsNullOrEmpty(SelectedCategory))
            {
                query = query.Where(n => n.Category == SelectedCategory);
            }

            var totalCount = await query.CountAsync();

            News = await query
                .OrderByDescending(n => n.PublishDate)
                .Skip(Skip)
                .Take(PageSize)
                .ToListAsync();

            HasMoreNews = (Skip + PageSize) < totalCount;

            _logger.LogInformation($"Loaded {News.Count} news items, Skip: {Skip}, HasMore: {HasMoreNews}, Category: {SelectedCategory ?? "All"}");
        }

        public async Task<IActionResult> OnPostAsync()
        {
            _logger.LogInformation($"OnPostAsync called with Skip: {Skip}, PageSize: {PageSize}, Category: {SelectedCategory ?? "All"}");

            await OnGetAsync();

            _logger.LogInformation($"Returning {News.Count} news items from OnPostAsync");

            return Partial("_NewsItemsPartial", News);
        }
    }
}
