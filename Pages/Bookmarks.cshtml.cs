using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using newsWebApp.Data;

namespace newsWebApp.Pages
{
    [Authorize]
    public class BookmarksModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;

        public BookmarksModel(ApplicationDbContext db, UserManager<IdentityUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public List<NewsItem> BookmarkedNews { get; private set; } = new();

        public async Task OnGetAsync()
        {
            var userId = _userManager.GetUserId(User);
            if (!string.IsNullOrEmpty(userId))
            {
                BookmarkedNews = await _db.UserBookmarks
                    .Where(b => b.UserId == userId)
                    .Include(b => b.NewsItem)
                    .OrderByDescending(b => b.BookmarkedAt)
                    .Select(b => b.NewsItem)
                    .ToListAsync();
            }
        }
    }
}