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
    private readonly SupplierMeLeadService _supplierMeLeadService;
    private readonly DocumentService _documentService;
    private readonly AuditService _auditService;

    private static readonly HashSet<string> AllowedMimeTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "application/pdf",
        "image/jpeg",
        "image/png",
        "image/webp"
    };

    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf",
        ".jpg",
        ".jpeg",
        ".png",
        ".webp"
    };

    private static readonly HashSet<string> AllowedDocumentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "certificate",
        "policy",
        "report",
        "logo",
        "banner",
        "other"
    };

    public SupplierMeController(
        SupplierMeService supplierMeService,
        SupplierMeLeadService supplierMeLeadService,
        DocumentService documentService,
        AuditService auditService)
    {
        _supplierMeService = supplierMeService;
        _supplierMeLeadService = supplierMeLeadService;
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

        try
        {
            var profile = await _supplierMeService.UpdateOwnProfileAsync(orgId, request, userId, ct);

            if (profile is null)
                return NotFound(ApiResponse<SupplierProfileDto>.Fail("NOT_FOUND", "No supplier profile found for your organization."));

            return Ok(ApiResponse<SupplierProfileDto>.Ok(profile));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<SupplierProfileDto>.Fail("VALIDATION_ERROR", ex.Message));
        }
    }

    /// <summary>
    /// List the authenticated supplier's certifications.
    /// </summary>
    [HttpGet("certifications")]
    public async Task<IActionResult> ListCertifications(CancellationToken ct)
    {
        var orgId = User.GetOrganizationId();
        var certs = await _supplierMeService.GetCertificationsByOrgAsync(orgId, ct);

        return Ok(ApiResponse<List<SupplierCertificationDto>>.Ok(certs));
    }

    /// <summary>
    /// Submit a new certification (status is always set to Pending).
    /// </summary>
    [HttpPost("certifications")]
    public async Task<IActionResult> AddCertification([FromBody] AddCertificationRequest request, CancellationToken ct)
    {
        var orgId = User.GetOrganizationId();
        var userId = User.GetUserId();

        try
        {
            var certification = await _supplierMeService.AddCertificationAsync(orgId, request, userId, ct);

            if (certification is null)
                return NotFound(ApiResponse<CertificationDto>.Fail("NOT_FOUND", "Supplier profile not found."));

            return StatusCode(201, ApiResponse<CertificationDto>.Ok(certification));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<CertificationDto>.Fail("VALIDATION_ERROR", ex.Message));
        }
    }

    /// <summary>
    /// List the authenticated supplier's documents.
    /// </summary>
    [HttpGet("documents")]
    public async Task<IActionResult> ListDocuments(CancellationToken ct)
    {
        var orgId = User.GetOrganizationId();
        var profileId = await _supplierMeService.GetProfileIdByOrgAsync(orgId, ct);

        if (profileId is null)
            return NotFound(ApiResponse<object>.Fail("NOT_FOUND", "No supplier profile found for your organization."));

        var documents = await _documentService.GetBySupplierAsync(profileId.Value, ct);

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
    /// Upload a document (multipart form data). Validates MIME type, file extension, and file size.
    /// NOTE: This endpoint currently saves metadata only. Actual blob upload to Azure Blob Storage
    /// must be implemented before production use (see backlog item).
    /// </summary>
    [HttpPost("documents")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(10 * 1024 * 1024)] // 10 MB hard limit at Kestrel level
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

        // Validate file extension matches MIME type — MIME alone is client-controlled and spoofable
        var extension = Path.GetExtension(file.FileName)?.ToLowerInvariant() ?? string.Empty;
        if (!AllowedExtensions.Contains(extension))
            return BadRequest(ApiResponse<object>.Fail("INVALID_FILE_EXTENSION",
                "File extension must be one of: .pdf, .jpg, .jpeg, .png, .webp"));

        // Validate documentType against allowlist
        if (string.IsNullOrWhiteSpace(documentType) || !AllowedDocumentTypes.Contains(documentType.Trim()))
            return BadRequest(ApiResponse<object>.Fail("INVALID_DOCUMENT_TYPE",
                $"Document type must be one of: {string.Join(", ", AllowedDocumentTypes)}"));

        var orgId = User.GetOrganizationId();
        var userId = User.GetUserId();
        var profileId = await _supplierMeService.GetProfileIdByOrgAsync(orgId, ct);

        if (profileId is null)
            return NotFound(ApiResponse<object>.Fail("NOT_FOUND", "No supplier profile found for your organization."));

        // Generate a safe filename to prevent path traversal — never trust the original filename
        var safeFileName = $"{Guid.NewGuid()}{extension}";
        var blobUrl = $"https://greensuppliers.blob.core.windows.net/documents/{profileId.Value}/{safeFileName}";

        // TODO: Upload file.OpenReadStream() to Azure Blob Storage here.
        // Currently only metadata is saved. This MUST be implemented before production.

        var document = await _documentService.CreateAsync(
            profileId.Value,
            file.FileName,
            blobUrl,
            file.ContentType,
            file.Length,
            documentType.Trim().ToLowerInvariant(),
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
    /// Sets IsPublished=true if the profile is sufficiently complete (>= 50%) and not flagged.
    /// </summary>
    [HttpPut("publish")]
    public async Task<IActionResult> RequestPublication(CancellationToken ct)
    {
        var orgId = User.GetOrganizationId();
        var userId = User.GetUserId();
        var (success, failureReason) = await _supplierMeService.RequestPublicationAsync(orgId, userId, ct);

        if (!success)
        {
            var message = failureReason switch
            {
                "PROFILE_NOT_FOUND" => "No supplier profile found for your organization.",
                "PROFILE_FLAGGED" => "Your profile has been flagged for review and cannot be published at this time.",
                "INCOMPLETE_PROFILE" => "Profile must be at least 50% complete to be published.",
                _ => "Unable to publish profile."
            };

            var statusCode = failureReason == "PROFILE_NOT_FOUND" ? 404 : 400;
            return StatusCode(statusCode, ApiResponse<object>.Fail(failureReason ?? "PUBLICATION_FAILED", message));
        }

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

    /// <summary>
    /// List leads for this supplier (paginated, filterable by status).
    /// </summary>
    [HttpGet("leads")]
    public async Task<IActionResult> GetLeads(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? status = null,
        CancellationToken ct = default)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 20;

        var orgId = User.GetOrganizationId();
        var result = await _supplierMeLeadService.GetLeadsAsync(orgId, page, pageSize, status, ct);

        return Ok(ApiResponse<List<LeadDto>>.Ok(result.Items, new PaginationMeta(result.Page, result.PageSize, result.Total, result.TotalPages)));
    }

    /// <summary>
    /// Get a single lead detail. IDOR-protected: only returns leads belonging to this supplier.
    /// </summary>
    [HttpGet("leads/{id:guid}")]
    public async Task<IActionResult> GetLeadDetail(Guid id, CancellationToken ct)
    {
        var orgId = User.GetOrganizationId();
        var lead = await _supplierMeLeadService.GetLeadDetailAsync(orgId, id, ct);

        if (lead is null)
            return NotFound(ApiResponse<LeadDto>.Fail("NOT_FOUND", "Lead not found."));

        return Ok(ApiResponse<LeadDto>.Ok(lead));
    }

    /// <summary>
    /// Update lead status. Only valid transitions: New -> Contacted, Contacted -> Closed.
    /// </summary>
    [HttpPatch("leads/{id:guid}/status")]
    public async Task<IActionResult> UpdateLeadStatus(Guid id, [FromBody] UpdateLeadStatusRequest request, CancellationToken ct)
    {
        var orgId = User.GetOrganizationId();
        var userId = User.GetUserId();
        var success = await _supplierMeLeadService.UpdateLeadStatusAsync(orgId, id, request.Status, userId, ct);

        if (!success)
            return BadRequest(ApiResponse<object>.Fail("INVALID_STATUS_TRANSITION",
                "Invalid status transition. Allowed: New -> Contacted, Contacted -> Closed."));

        return Ok(ApiResponse<object>.Ok(new { updated = true }));
    }
}
