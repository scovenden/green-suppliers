using GreenSuppliers.Api.Models.DTOs;
using GreenSuppliers.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GreenSuppliers.Api.Controllers;

[ApiController]
[Route("api/v1/admin/content")]
[Authorize(Policy = "Admin")]
public class AdminContentController : ControllerBase
{
    private readonly ContentService _contentService;

    public AdminContentController(ContentService contentService)
    {
        _contentService = contentService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 50, CancellationToken ct = default)
    {
        var result = await _contentService.GetAllAsync(page, pageSize);
        return Ok(ApiResponse<PagedResult<ContentPageDto>>.Ok(result));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateContentRequest request, CancellationToken ct)
    {
        var page = await _contentService.CreateAsync(
            request.Title, request.Slug, request.Body, request.PageType,
            request.MetaTitle, request.MetaDesc);

        return StatusCode(201, ApiResponse<ContentPageDto>.Ok(page));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateContentRequest request, CancellationToken ct)
    {
        var page = await _contentService.UpdateAsync(
            id, request.Title, request.Slug, request.Body,
            request.MetaTitle, request.MetaDesc, request.IsPublished);

        if (page is null)
            return NotFound(ApiResponse<ContentPageDto>.Fail("NOT_FOUND", "Content page not found."));

        return Ok(ApiResponse<ContentPageDto>.Ok(page));
    }
}

public record CreateContentRequest(string Title, string Slug, string Body, string PageType, string? MetaTitle, string? MetaDesc);
public record UpdateContentRequest(string Title, string? Slug, string Body, string? MetaTitle, string? MetaDesc, bool IsPublished);
