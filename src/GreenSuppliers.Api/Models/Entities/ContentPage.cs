using System.ComponentModel.DataAnnotations;

namespace GreenSuppliers.Api.Models.Entities;

public class ContentPage
{
    public Guid Id { get; set; }

    [MaxLength(300)]
    public string Slug { get; set; } = string.Empty;

    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? MetaTitle { get; set; }

    [MaxLength(300)]
    public string? MetaDesc { get; set; }

    public string Body { get; set; } = string.Empty;

    [MaxLength(30)]
    public string PageType { get; set; } = string.Empty;

    public bool IsPublished { get; set; }
    public DateTime? PublishedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
