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

    private static readonly HashSet<string> AllowedMimeTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "application/pdf",
        "image/jpeg",
        "image/png",
        "image/webp"
    };

    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf", ".jpg", ".jpeg", ".png", ".webp"
    };

    private static readonly HashSet<string> AllowedDocumentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "certificate", "policy", "report", "logo", "banner", "other"
    };

    public AdminDocumentsController(DocumentService documentService)
    {
        _documentService = documentService;
    }

    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(10 * 1024 * 1024)] // 10 MB hard limit at Kestrel level
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

        if (!AllowedMimeTypes.Contains(file.ContentType))
            return BadRequest(ApiResponse<object>.Fail("INVALID_FILE_TYPE", "Only PDF, JPG, PNG, and WebP files are allowed."));

        var extension = Path.GetExtension(file.FileName)?.ToLowerInvariant() ?? string.Empty;
        if (!AllowedExtensions.Contains(extension))
            return BadRequest(ApiResponse<object>.Fail("INVALID_FILE_EXTENSION",
                "File extension must be one of: .pdf, .jpg, .jpeg, .png, .webp"));

        if (string.IsNullOrWhiteSpace(documentType) || !AllowedDocumentTypes.Contains(documentType.Trim()))
            return BadRequest(ApiResponse<object>.Fail("INVALID_DOCUMENT_TYPE",
                $"Document type must be one of: {string.Join(", ", AllowedDocumentTypes)}"));

        if (supplierProfileId == Guid.Empty)
            return BadRequest(ApiResponse<object>.Fail("INVALID_SUPPLIER", "Supplier profile ID is required."));

        // Generate a safe filename to prevent path traversal -- never trust the original filename
        var safeFileName = $"{Guid.NewGuid()}{extension}";
        var blobUrl = $"https://greensuppliers.blob.core.windows.net/documents/{supplierProfileId}/{safeFileName}";

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
