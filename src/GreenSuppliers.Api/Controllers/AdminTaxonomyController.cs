using GreenSuppliers.Api.Models.DTOs;
using GreenSuppliers.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GreenSuppliers.Api.Controllers;

[ApiController]
[Route("api/v1/admin/taxonomy")]
[Authorize(Policy = "Admin")]
public class AdminTaxonomyController : ControllerBase
{
    private readonly TaxonomyService _taxonomyService;

    public AdminTaxonomyController(TaxonomyService taxonomyService)
    {
        _taxonomyService = taxonomyService;
    }

    // --- Industries ---

    [HttpGet("industries")]
    [HttpGet("/api/v1/admin/industries")]
    public async Task<IActionResult> GetIndustries(CancellationToken ct)
    {
        var industries = await _taxonomyService.GetIndustriesAsync(activeOnly: false);
        return Ok(ApiResponse<List<IndustryDto>>.Ok(industries));
    }

    [HttpPost("industries")]
    public async Task<IActionResult> CreateIndustry([FromBody] CreateIndustryRequest request, CancellationToken ct)
    {
        var industry = await _taxonomyService.CreateIndustryAsync(request.Name, request.Description, request.ParentId);
        return StatusCode(201, ApiResponse<IndustryDto>.Ok(industry));
    }

    [HttpPut("industries/{id:guid}")]
    public async Task<IActionResult> UpdateIndustry(Guid id, [FromBody] UpdateIndustryRequest request, CancellationToken ct)
    {
        var industry = await _taxonomyService.UpdateIndustryAsync(id, request.Name, request.Description);

        if (industry is null)
            return NotFound(ApiResponse<IndustryDto>.Fail("NOT_FOUND", "Industry not found."));

        return Ok(ApiResponse<IndustryDto>.Ok(industry));
    }

    // --- Certification Types ---

    [HttpGet("certification-types")]
    [HttpGet("/api/v1/admin/certification-types")]
    public async Task<IActionResult> GetCertTypes(CancellationToken ct)
    {
        var certTypes = await _taxonomyService.GetCertTypesAsync(activeOnly: false);
        return Ok(ApiResponse<List<CertTypeDto>>.Ok(certTypes));
    }

    [HttpPost("certification-types")]
    public async Task<IActionResult> CreateCertType([FromBody] CreateCertTypeRequest request, CancellationToken ct)
    {
        var certType = await _taxonomyService.CreateCertTypeAsync(request.Name, request.Description);
        return StatusCode(201, ApiResponse<CertTypeDto>.Ok(certType));
    }

    [HttpPut("certification-types/{id:guid}")]
    public async Task<IActionResult> UpdateCertType(Guid id, [FromBody] UpdateCertTypeRequest request, CancellationToken ct)
    {
        var certType = await _taxonomyService.UpdateCertTypeAsync(id, request.Name, request.Description);

        if (certType is null)
            return NotFound(ApiResponse<CertTypeDto>.Fail("NOT_FOUND", "Certification type not found."));

        return Ok(ApiResponse<CertTypeDto>.Ok(certType));
    }

    // --- Service Tags ---

    [HttpGet("service-tags")]
    [HttpGet("/api/v1/admin/service-tags")]
    public async Task<IActionResult> GetServiceTags(CancellationToken ct)
    {
        var tags = await _taxonomyService.GetServiceTagsAsync(activeOnly: false);
        return Ok(ApiResponse<List<ServiceTagDto>>.Ok(tags));
    }

    [HttpPost("service-tags")]
    public async Task<IActionResult> CreateServiceTag([FromBody] CreateServiceTagRequest request, CancellationToken ct)
    {
        var tag = await _taxonomyService.CreateServiceTagAsync(request.Name);
        return StatusCode(201, ApiResponse<ServiceTagDto>.Ok(tag));
    }
}

public record CreateIndustryRequest(string Name, string? Description, Guid? ParentId);
public record UpdateIndustryRequest(string Name, string? Description);
public record CreateCertTypeRequest(string Name, string? Description);
public record UpdateCertTypeRequest(string Name, string? Description);
public record CreateServiceTagRequest(string Name);
