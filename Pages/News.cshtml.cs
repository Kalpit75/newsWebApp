using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using newsWebApp.Data;
using System.Text.Json;

namespace newsWebApp.Pages
{
    [Authorize]
    public class NewsModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<NewsModel> _logger;
        private readonly UserManager<IdentityUser> _userManager;

        public NewsModel(ApplicationDbContext db, ILogger<NewsModel> logger, UserManager<IdentityUser> userManager)
        {
            _db = db;
            _logger = logger;
            _userManager = userManager;
        }

        public List<NewsItem> News { get; private set; } = new();
        public bool HasMoreNews { get; private set; }
        public List<string> Categories { get; private set; } = new();
        public HashSet<int> BookmarkedNewsIds { get; private set; } = new();

        [BindProperty(SupportsGet = true)]
        public int PageSize { get; set; } = 10;

        [BindProperty(SupportsGet = true)]
        public int Skip { get; set; } = 0;

        [BindProperty(SupportsGet = true)]
        public string? SelectedCategory { get; set; }

        public async Task OnGetAsync()
        {
            var userId = _userManager.GetUserId(User);
            
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

            // Load user's bookmarked news IDs for current page
            if (!string.IsNullOrEmpty(userId))
            {
                var newsIds = News.Select(n => n.Id).ToList();
                BookmarkedNewsIds = (await _db.UserBookmarks
                    .Where(b => b.UserId == userId && newsIds.Contains(b.NewsItemId))
                    .Select(b => b.NewsItemId)
                    .ToListAsync()).ToHashSet();
            }

            _logger.LogInformation($"Loaded {News.Count} news items, Skip: {Skip}, HasMore: {HasMoreNews}, Category: {SelectedCategory ?? "All"}");
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await OnGetAsync();
            return Partial("_NewsItemsPartial", News);
        }

        // Add bookmark toggle endpoint - FIXED VERSION
        public async Task<IActionResult> OnPostToggleBookmarkAsync(int newsId)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("User not found");
            }

            var existingBookmark = await _db.UserBookmarks
                .FirstOrDefaultAsync(b => b.UserId == userId && b.NewsItemId == newsId);

            if (existingBookmark != null)
            {
                // Remove bookmark
                _db.UserBookmarks.Remove(existingBookmark);
                await _db.SaveChangesAsync();
                return new JsonResult(new { isBookmarked = false }); // FIXED: Use JsonResult
            }
            else
            {
                // Add bookmark
                var bookmark = new UserBookmark
                {
                    UserId = userId,
                    NewsItemId = newsId,
                    BookmarkedAt = DateTime.UtcNow
                };
                
                _db.UserBookmarks.Add(bookmark);
                await _db.SaveChangesAsync();
                return new JsonResult(new { isBookmarked = true }); // FIXED: Use JsonResult
            }
        }
    }
}
