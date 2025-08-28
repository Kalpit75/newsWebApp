using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using newsWebApp.Data;


namespace newsWebApp.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ApplicationDbContext db, ILogger<IndexModel> logger)
        {
            _logger = logger;
        }
 
    }
}