using GreenSuppliers.Api.Extensions;
using GreenSuppliers.Api.Models.DTOs;
using GreenSuppliers.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GreenSuppliers.Api.Controllers;

[ApiController]
[Route("api/v1/supplier/me")]
[Authorize(Policy = "Supplier")]
public class SupplierMeController : ControllerBase
{
    private readonly SupplierMeService _supplierMeService;
    private readonly DocumentService _documentService;
    private readonly AuditService _auditService;

    private static readonly HashSet<string> AllowedMimeTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "application/pdf",
        "image/jpeg",
        "image/png",
        "image/webp"
    };

    public SupplierMeController(
        SupplierMeService supplierMeService,
        DocumentService documentService,
        AuditService auditService)
    {
        _supplierMeService = supplierMeService;
        _documentService = documentService;
        _auditService = auditService;
    }

    /// <summary>
    /// Get the authenticated supplier's own profile.
    /// </summary>
    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile(CancellationToken ct)
    {
        var orgId = User.GetOrganizationId();
        var profile = await _supplierMeService.GetByOrganizationIdAsync(orgId, ct);

        if (profile is null)
            return NotFound(ApiResponse<SupplierProfileDto>.Fail("NOT_FOUND", "No supplier profile found for your organization."));

        return Ok(ApiResponse<SupplierProfileDto>.Ok(profile));
    }

    /// <summary>
    /// Update the authenticated supplier's own profile (editable fields only).
    /// </summary>
    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateMyProfileRequest request, CancellationToken ct)
    {
        var orgId = User.GetOrganizationId();
        var userId = User.GetUserId();
        var profile = await _supplierMeService.UpdateOwnProfileAsync(orgId, request, userId, ct);

        if (profile is null)
            return NotFound(ApiResponse<SupplierProfileDto>.Fail("NOT_FOUND", "No supplier profile found for your organization."));

        return Ok(ApiResponse<SupplierProfileDto>.Ok(profile));
    }

    /// <summary>
    /// List the authenticated supplier's certifications.
    /// </summary>
    [HttpGet("certifications")]
    public async Task<IActionResult> ListCertifications(CancellationToken ct)
    {
        var orgId = User.GetOrganizationId();
        var profile = await _supplierMeService.GetByOrganizationIdAsync(orgId, ct);

        if (profile is null)
            return NotFound(ApiResponse<object>.Fail("NOT_FOUND", "No supplier profile found for your organization."));

        return Ok(ApiResponse<List<SupplierCertificationDto>>.Ok(profile.Certifications));
    }

    /// <summary>
    /// Submit a new certification (status is always set to Pending).
    /// </summary>
    [HttpPost("certifications")]
    public async Task<IActionResult> AddCertification([FromBody] AddCertificationRequest request, CancellationToken ct)
    {
        var orgId = User.GetOrganizationId();
        var userId = User.GetUserId();
        var certification = await _supplierMeService.AddCertificationAsync(orgId, request, userId, ct);

        if (certification is null)
            return NotFound(ApiResponse<CertificationDto>.Fail("NOT_FOUND", "Supplier profile or certification type not found."));

        return StatusCode(201, ApiResponse<CertificationDto>.Ok(certification));
    }

    /// <summary>
    /// List the authenticated supplier's documents.
    /// </summary>
    [HttpGet("documents")]
    public async Task<IActionResult> ListDocuments(CancellationToken ct)
    {
        var orgId = User.GetOrganizationId();
        var profile = await _supplierMeService.GetByOrganizationIdAsync(orgId, ct);

        if (profile is null)
            return NotFound(ApiResponse<object>.Fail("NOT_FOUND", "No supplier profile found for your organization."));

        var documents = await _documentService.GetBySupplierAsync(profile.Id, ct);

        var dtos = documents.Select(d => new
        {
            d.Id,
            d.SupplierProfileId,
            d.FileName,
            d.BlobUrl,
            d.ContentType,
            d.FileSizeBytes,
            d.DocumentType,
            d.CreatedAt
        }).ToList();

        return Ok(ApiResponse<object>.Ok(dtos));
    }

    /// <summary>
    /// Upload a document (multipart form data). Validates MIME type and file size.
    /// </summary>
    [HttpPost("documents")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadDocument(
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

        var orgId = User.GetOrganizationId();
        var userId = User.GetUserId();
        var profile = await _supplierMeService.GetByOrganizationIdAsync(orgId, ct);

        if (profile is null)
            return NotFound(ApiResponse<object>.Fail("NOT_FOUND", "No supplier profile found for your organization."));

        // Generate a safe filename to prevent path traversal
        var safeFileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var blobUrl = $"https://greensuppliers.blob.core.windows.net/documents/{profile.Id}/{safeFileName}";

        var document = await _documentService.CreateAsync(
            profile.Id,
            file.FileName,
            blobUrl,
            file.ContentType,
            file.Length,
            documentType,
            userId,
            ct);

        await _auditService.LogAsync(userId, "DocumentUploaded", "Document", document.Id, ct: ct);

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

    /// <summary>
    /// Request publication of the supplier's profile.
    /// Sets IsPublished=true if the profile is sufficiently complete (>= 50%).
    /// </summary>
    [HttpPut("publish")]
    public async Task<IActionResult> RequestPublication(CancellationToken ct)
    {
        var orgId = User.GetOrganizationId();
        var success = await _supplierMeService.RequestPublicationAsync(orgId, ct);

        if (!success)
            return BadRequest(ApiResponse<object>.Fail("INCOMPLETE_PROFILE",
                "Profile must be at least 50% complete and not flagged to be published."));

        return Ok(ApiResponse<object>.Ok(new { published = true }));
    }

    /// <summary>
    /// Get the supplier dashboard stats (leads, certs, ESG score, profile completeness).
    /// </summary>
    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard(CancellationToken ct)
    {
        var orgId = User.GetOrganizationId();
        var stats = await _supplierMeService.GetDashboardStatsAsync(orgId, ct);

        return Ok(ApiResponse<SupplierDashboardDto>.Ok(stats));
    }
}
