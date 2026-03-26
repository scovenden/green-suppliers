using GreenSuppliers.Api.Data;
using GreenSuppliers.Api.Models.DTOs;
using GreenSuppliers.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace GreenSuppliers.Api.Services;

public class ContentService
{
    private readonly GreenSuppliersDbContext _context;

    public ContentService(GreenSuppliersDbContext context)
    {
        _context = context;
    }

    public async Task<ContentPageDto?> GetBySlugAsync(string slug, CancellationToken ct = default)
    {
        var page = await _context.ContentPages
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Slug == slug && p.IsPublished, ct);

        return page is null ? null : MapToDto(page);
    }

    public async Task<PagedResult<ContentPageDto>> GetAllAsync(int page, int pageSize, CancellationToken ct = default)
    {
        var query = _context.ContentPages.AsNoTracking();

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(p => p.UpdatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<ContentPageDto>
        {
            Items = items.Select(MapToDto).ToList(),
            Page = page,
            PageSize = pageSize,
            Total = total
        };
    }

    public async Task<ContentPageDto> CreateAsync(string title, string slug, string body, string pageType,
        string? metaTitle, string? metaDesc, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;

        var contentPage = new ContentPage
        {
            Id = Guid.NewGuid(),
            Title = title,
            Slug = slug,
            Body = body,
            PageType = pageType,
            MetaTitle = metaTitle,
            MetaDesc = metaDesc,
            IsPublished = false,
            CreatedAt = now,
            UpdatedAt = now
        };

        _context.ContentPages.Add(contentPage);
        await _context.SaveChangesAsync(ct);

        return MapToDto(contentPage);
    }

    public async Task<ContentPageDto?> UpdateAsync(Guid id, string title, string? slug, string body,
        string? metaTitle, string? metaDesc, bool isPublished, CancellationToken ct = default)
    {
        var contentPage = await _context.ContentPages.FindAsync(new object[] { id }, ct);
        if (contentPage is null) return null;

        var now = DateTime.UtcNow;

        contentPage.Title = title;
        if (!string.IsNullOrWhiteSpace(slug))
            contentPage.Slug = slug;
        contentPage.Body = body;
        contentPage.MetaTitle = metaTitle;
        contentPage.MetaDesc = metaDesc;
        contentPage.UpdatedAt = now;

        if (isPublished && !contentPage.IsPublished)
        {
            contentPage.IsPublished = true;
            contentPage.PublishedAt = now;
        }
        else if (!isPublished)
        {
            contentPage.IsPublished = false;
        }

        await _context.SaveChangesAsync(ct);

        return MapToDto(contentPage);
    }

    private static ContentPageDto MapToDto(ContentPage page)
    {
        return new ContentPageDto
        {
            Id = page.Id,
            Slug = page.Slug,
            Title = page.Title,
            MetaTitle = page.MetaTitle,
            MetaDesc = page.MetaDesc,
            Body = page.Body,
            PageType = page.PageType,
            IsPublished = page.IsPublished,
            PublishedAt = page.PublishedAt,
            CreatedAt = page.CreatedAt,
            UpdatedAt = page.UpdatedAt
        };
    }
}
