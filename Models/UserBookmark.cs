using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

public class UserBookmark
{
    [Key]
    public int Id { get; set; }
    
    public string UserId { get; set; } = string.Empty;
    public int NewsItemId { get; set; }
    public DateTime BookmarkedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public IdentityUser User { get; set; } = null!;
    public NewsItem NewsItem { get; set; } = null!;
}