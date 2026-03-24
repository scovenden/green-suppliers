using GreenSuppliers.Api.Models.DTOs;
using GreenSuppliers.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace GreenSuppliers.Api.Controllers;

[ApiController]
[Route("api/v1/industries")]
public class IndustriesController : ControllerBase
{
    private readonly TaxonomyService _taxonomyService;
    private readonly ISupplierSearchService _searchService;

    public IndustriesController(TaxonomyService taxonomyService, ISupplierSearchService searchService)
    {
        _taxonomyService = taxonomyService;
        _searchService = searchService;
    }

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        var industries = await _taxonomyService.GetIndustriesAsync(activeOnly: true);
        return Ok(ApiResponse<List<IndustryDto>>.Ok(industries));
    }

    [HttpGet("{slug}")]
    public async Task<IActionResult> GetBySlug(string slug, CancellationToken ct)
    {
        var industry = await _taxonomyService.GetIndustryBySlugAsync(slug);

        if (industry is null)
            return NotFound(ApiResponse<object>.Fail("NOT_FOUND", "Industry not found."));

        var supplierQuery = new SupplierSearchQuery { IndustrySlug = slug, Page = 1, PageSize = 20 };
        var suppliers = await _searchService.SearchAsync(supplierQuery, ct);

        var result = new
        {
            Industry = industry,
            Suppliers = suppliers.Items,
            Meta = new PaginationMeta(suppliers.Page, suppliers.PageSize, suppliers.Total, suppliers.TotalPages)
        };

        return Ok(ApiResponse<object>.Ok(result));
    }
}
