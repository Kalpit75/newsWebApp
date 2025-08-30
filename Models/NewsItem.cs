using System;
using System.ComponentModel.DataAnnotations;

public class NewsItem
{
    [Key]
    public int Id { get; set; }
    public string Title { get; set; }
    public string Link { get; set; }
    public string Source { get; set; }
    public DateTimeOffset PublishDate { get; set; }
    public string Summary { get; set; } // RSS summary
    public string Content { get; set; }
    public string? Category { get; set; } // news category
}