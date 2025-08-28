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
        public DbSet<UserBookmark> UserBookmarks { get; set; } // Add this
        
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            
            // Add indexes for better performance
            builder.Entity<NewsItem>()
                .HasIndex(n => n.Link)
                .IsUnique();
                
            builder.Entity<NewsItem>()
                .HasIndex(n => n.PublishDate);

            // Configure UserBookmark relationships
            builder.Entity<UserBookmark>()
                .HasOne(b => b.User)
                .WithMany()
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<UserBookmark>()
                .HasOne(b => b.NewsItem)
                .WithMany()
                .HasForeignKey(b => b.NewsItemId)
                .OnDelete(DeleteBehavior.Cascade);

            // Ensure unique bookmark per user per news item
            builder.Entity<UserBookmark>()
                .HasIndex(b => new { b.UserId, b.NewsItemId })
                .IsUnique();
        }
    }
}