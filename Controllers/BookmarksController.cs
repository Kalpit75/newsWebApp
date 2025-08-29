using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using newsWebApp.Data;
using newsWebApp.Models;

namespace newsWebApp.Controllers
{
    [Authorize]
    public class BookmarksController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<BookmarksController> _logger;

        public BookmarksController(ApplicationDbContext db, UserManager<IdentityUser> userManager, ILogger<BookmarksController> logger)
        {
            _db = db;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }

            var bookmarkedNews = await _db.UserBookmarks
                .Where(b => b.UserId == userId)
                .Include(b => b.NewsItem)
                .OrderByDescending(b => b.BookmarkedAt)
                .Select(b => b.NewsItem)
                .ToListAsync();

            var bookmarkedNewsIds = bookmarkedNews.Select(n => n.Id).ToHashSet();

            var viewModel = new NewsViewModel
            {
                News = bookmarkedNews,
                BookmarkedNewsIds = bookmarkedNewsIds
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> RemoveBookmark(int newsId)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "User not found" });
            }

            var bookmark = await _db.UserBookmarks
                .FirstOrDefaultAsync(b => b.UserId == userId && b.NewsItemId == newsId);

            if (bookmark != null)
            {
                _db.UserBookmarks.Remove(bookmark);
                await _db.SaveChangesAsync();
                return Json(new { success = true });
            }

            return Json(new { success = false, message = "Bookmark not found" });
        }
    }
}