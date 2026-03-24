using GreenSuppliers.Api.Models.DTOs;
using GreenSuppliers.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace GreenSuppliers.Api.Controllers;

[ApiController]
[Route("api/v1/service-tags")]
public class ServiceTagsController : ControllerBase
{
    private readonly TaxonomyService _taxonomyService;

    public ServiceTagsController(TaxonomyService taxonomyService)
    {
        _taxonomyService = taxonomyService;
    }

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        var tags = await _taxonomyService.GetServiceTagsAsync(activeOnly: true);
        return Ok(ApiResponse<List<ServiceTagDto>>.Ok(tags));
    }
}
