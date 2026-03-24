using GreenSuppliers.Api.Models.DTOs;
using GreenSuppliers.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace GreenSuppliers.Api.Controllers;

[ApiController]
[Route("api/v1/countries")]
public class CountriesController : ControllerBase
{
    private readonly TaxonomyService _taxonomyService;
    private readonly ISupplierSearchService _searchService;

    public CountriesController(TaxonomyService taxonomyService, ISupplierSearchService searchService)
    {
        _taxonomyService = taxonomyService;
        _searchService = searchService;
    }

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        var countries = await _taxonomyService.GetCountriesAsync(activeOnly: true);
        return Ok(ApiResponse<List<CountryDto>>.Ok(countries));
    }

    [HttpGet("{code}")]
    public async Task<IActionResult> GetByCode(string code, CancellationToken ct)
    {
        var country = await _taxonomyService.GetCountryByCodeAsync(code);

        if (country is null)
            return NotFound(ApiResponse<object>.Fail("NOT_FOUND", "Country not found."));

        var supplierQuery = new SupplierSearchQuery { CountryCode = code, Page = 1, PageSize = 20 };
        var suppliers = await _searchService.SearchAsync(supplierQuery, ct);

        var result = new
        {
            Country = country,
            Suppliers = suppliers.Items,
            Meta = new PaginationMeta(suppliers.Page, suppliers.PageSize, suppliers.Total, suppliers.TotalPages)
        };

        return Ok(ApiResponse<object>.Ok(result));
    }
}
