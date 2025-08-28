using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace newsWebApp.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        
        // Add your news data here
        public DbSet<NewsItem> NewsItems { get; set; }
        
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            
            // Add indexes for better performance
            builder.Entity<NewsItem>()
                .HasIndex(n => n.Link)
                .IsUnique();
                
            builder.Entity<NewsItem>()
                .HasIndex(n => n.PublishDate);
        }
    }
}