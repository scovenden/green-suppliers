using GreenSuppliers.Api.Extensions;
using GreenSuppliers.Api.Models.DTOs;
using GreenSuppliers.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GreenSuppliers.Api.Controllers;

[ApiController]
[Route("api/v1/admin/documents")]
[Authorize(Policy = "Admin")]
public class AdminDocumentsController : ControllerBase
{
    private readonly DocumentService _documentService;

    public AdminDocumentsController(DocumentService documentService)
    {
        _documentService = documentService;
    }

    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Upload(
        [FromForm] Guid supplierProfileId,
        [FromForm] string documentType,
        IFormFile file,
        CancellationToken ct)
    {
        if (file is null || file.Length == 0)
            return BadRequest(ApiResponse<object>.Fail("INVALID_FILE", "A file is required."));

        if (file.Length > 10 * 1024 * 1024) // 10 MB limit
            return BadRequest(ApiResponse<object>.Fail("FILE_TOO_LARGE", "File must not exceed 10 MB."));

        if (supplierProfileId == Guid.Empty)
            return BadRequest(ApiResponse<object>.Fail("INVALID_SUPPLIER", "Supplier profile ID is required."));

        // In production, upload to Azure Blob Storage. For now, generate a placeholder URL.
        var blobUrl = $"https://greensuppliers.blob.core.windows.net/documents/{Guid.NewGuid()}/{file.FileName}";

        var adminUserId = User.GetUserId();

        var document = await _documentService.CreateAsync(
            supplierProfileId,
            file.FileName,
            blobUrl,
            file.ContentType,
            file.Length,
            documentType,
            adminUserId);

        var dto = new
        {
            document.Id,
            document.SupplierProfileId,
            document.FileName,
            document.BlobUrl,
            document.ContentType,
            document.FileSizeBytes,
            document.DocumentType,
            document.CreatedAt
        };

        return StatusCode(201, ApiResponse<object>.Ok(dto));
    }

    private Guid GetAdminUserId()
    {
        var sub = User.FindFirst("sub")?.Value;
        return Guid.TryParse(sub, out var userId) ? userId : Guid.Empty;
    }
}
