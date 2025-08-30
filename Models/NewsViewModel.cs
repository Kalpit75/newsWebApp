using newsWebApp.Data;

namespace newsWebApp.Models
{
    public class NewsViewModel
    {
        public IEnumerable<NewsItem> News { get; set; }
        public HashSet<int> BookmarkedIds { get; set; } // Add this property
        public HashSet<int> BookmarkedNewsIds { get; internal set; }
        public string? SelectedCategory { get; internal set; }
        public List<string> Categories { get; internal set; }
        public bool HasMoreNews { get; internal set; }
        public int PageSize { get; internal set; }
        public int Skip { get; internal set; }

        // Add other properties as needed
    }
}