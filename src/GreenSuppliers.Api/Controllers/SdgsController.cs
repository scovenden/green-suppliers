using GreenSuppliers.Api.Models.DTOs;
using GreenSuppliers.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace GreenSuppliers.Api.Controllers;

[ApiController]
[Route("api/v1/sdgs")]
public class SdgsController : ControllerBase
{
    private readonly SdgService _sdgService;

    public SdgsController(SdgService sdgService)
    {
        _sdgService = sdgService;
    }

    /// <summary>
    /// Returns all 17 UN Sustainable Development Goals. Public endpoint.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var sdgs = await _sdgService.GetAllAsync(ct);
        return Ok(ApiResponse<List<SdgDto>>.Ok(sdgs));
    }
}
