using GreenSuppliers.Api.Models.DTOs;
using GreenSuppliers.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace GreenSuppliers.Api.Controllers;

[ApiController]
[Route("api/v1/content")]
public class ContentController : ControllerBase
{
    private readonly ContentService _contentService;

    public ContentController(ContentService contentService)
    {
        _contentService = contentService;
    }

    [HttpGet("{slug}")]
    public async Task<IActionResult> GetBySlug(string slug, CancellationToken ct)
    {
        var page = await _contentService.GetBySlugAsync(slug);

        if (page is null)
            return NotFound(ApiResponse<ContentPageDto>.Fail("NOT_FOUND", "Content page not found."));

        return Ok(ApiResponse<ContentPageDto>.Ok(page));
    }
}
