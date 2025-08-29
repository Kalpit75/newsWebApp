using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using newsWebApp.Data;
using newsWebApp.Models;

namespace newsWebApp.Controllers
{
    [Authorize]
    public class NewsController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;

        public NewsController(ApplicationDbContext db, UserManager<IdentityUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(string? selectedCategory, int skip = 0, int pageSize = 10)
        {
            var user = await _userManager.GetUserAsync(User);
            
            // Get all available categories
            var categories = await _db.NewsItems
                .Where(n => !string.IsNullOrEmpty(n.Category))
                .Select(n => n.Category!)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();

            // Get user's bookmarked news IDs
            var bookmarkedIds = new HashSet<int>();
            if (user != null)
            {
                bookmarkedIds = await _db.UserBookmarks
                    .Where(b => b.UserId == user.Id)
                    .Select(b => b.NewsItemId)
                    .ToHashSetAsync();
            }

            // Build query for news items
            var query = _db.NewsItems.AsQueryable();

            // Apply category filter if selected
            if (!string.IsNullOrEmpty(selectedCategory))
            {
                query = query.Where(n => n.Category == selectedCategory);
            }

            // Get news items with pagination
            var news = await query
                .OrderByDescending(n => n.PublishDate)
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();

            // Check if there are more news items
            var totalCount = await query.CountAsync();
            var hasMoreNews = (skip + pageSize) < totalCount;

            var viewModel = new NewsViewModel
            {
                News = news,
                Categories = categories,
                SelectedCategory = selectedCategory,
                BookmarkedNewsIds = bookmarkedIds,
                HasMoreNews = hasMoreNews,
                PageSize = pageSize,
                Skip = skip
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> LoadMore(string? selectedCategory, int skip, int pageSize)
        {
            var user = await _userManager.GetUserAsync(User);
            
            // Get user's bookmarked news IDs
            var bookmarkedIds = new HashSet<int>();
            if (user != null)
            {
                bookmarkedIds = await _db.UserBookmarks
                    .Where(b => b.UserId == user.Id)
                    .Select(b => b.NewsItemId)
                    .ToHashSetAsync();
            }

            // Build query for news items
            var query = _db.NewsItems.AsQueryable();

            // Apply category filter if selected
            if (!string.IsNullOrEmpty(selectedCategory))
            {
                query = query.Where(n => n.Category == selectedCategory);
            }

            // Get news items with pagination
            var news = await query
                .OrderByDescending(n => n.PublishDate)
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();

            var viewModel = new NewsViewModel
            {
                News = news,
                BookmarkedNewsIds = bookmarkedIds,
                SelectedCategory = selectedCategory
            };

            return PartialView("_NewsItemsPartial", viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleBookmark(int newsId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Json(new { success = false, message = "User not found" });
            }

            var existingBookmark = await _db.UserBookmarks
                .FirstOrDefaultAsync(b => b.UserId == user.Id && b.NewsItemId == newsId);

            bool isBookmarked;

            if (existingBookmark != null)
            {
                // Remove bookmark
                _db.UserBookmarks.Remove(existingBookmark);
                isBookmarked = false;
            }
            else
            {
                // Add bookmark
                var bookmark = new UserBookmark
                {
                    UserId = user.Id,
                    NewsItemId = newsId,
                    BookmarkedAt = DateTime.UtcNow
                };
                _db.UserBookmarks.Add(bookmark);
                isBookmarked = true;
            }

            await _db.SaveChangesAsync();

            return Json(new { success = true, isBookmarked = isBookmarked });
        }
    }
}