using Microsoft.EntityFrameworkCore;

public class NewsDbContext : DbContext
{
    public NewsDbContext(DbContextOptions<NewsDbContext> options) : base(options) { }
    public DbSet<NewsItem> NewsItems { get; set; }
}