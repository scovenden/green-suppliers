namespace GreenSuppliers.Api.Models.DTOs;

public class ContentPageDto
{
    public Guid Id { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? MetaTitle { get; set; }
    public string? MetaDesc { get; set; }
    public string Body { get; set; } = string.Empty;
    public string PageType { get; set; } = string.Empty;
    public bool IsPublished { get; set; }
    public DateTime? PublishedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
