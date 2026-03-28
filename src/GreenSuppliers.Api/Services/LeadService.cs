using System.Net;
using System.Text.Json;
using GreenSuppliers.Api.Data;
using GreenSuppliers.Api.Helpers;
using GreenSuppliers.Api.Models.DTOs;
using GreenSuppliers.Api.Models.Entities;
using GreenSuppliers.Api.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace GreenSuppliers.Api.Services;

public class LeadService
{
    private readonly GreenSuppliersDbContext _context;
    private readonly AuditService _audit;
    private readonly IConfiguration _configuration;

    public LeadService(GreenSuppliersDbContext context, AuditService audit, IConfiguration configuration)
    {
        _context = context;
        _audit = audit;
        _configuration = configuration;
    }

    public async Task<LeadDto> CreateLeadAsync(LeadRequest request, string? ipAddress, Guid? buyerUserId = null, Guid? buyerOrgId = null, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;

        var lead = new Lead
        {
            Id = Guid.NewGuid(),
            SupplierProfileId = request.SupplierProfileId,
            BuyerUserId = buyerUserId,
            BuyerOrganizationId = buyerOrgId,
            ContactName = request.ContactName,
            ContactEmail = request.ContactEmail,
            ContactPhone = request.ContactPhone,
            CompanyName = request.CompanyName,
            Message = request.Message,
            Status = LeadStatus.New,
            LeadType = "inquiry",
            IpAddress = ipAddress,
            CreatedAt = now,
            UpdatedAt = now
        };

        _context.Leads.Add(lead);

        // Queue email notification -- HTML-encode all user-supplied values to prevent XSS
        var safeName = WebUtility.HtmlEncode(request.ContactName);
        var safeEmail = WebUtility.HtmlEncode(request.ContactEmail);
        var safeMessage = WebUtility.HtmlEncode(request.Message);

        _context.EmailQueue.Add(new EmailQueueItem
        {
            Id = Guid.NewGuid(),
            ToEmail = request.ContactEmail,
            ToName = request.ContactName,
            Subject = $"New lead inquiry from {safeName}",
            BodyHtml = $"<p>New inquiry from {safeName} ({safeEmail})</p><p>{safeMessage}</p>",
            TemplateType = "lead_notification",
            TemplateData = JsonSerializer.Serialize(new { lead.Id, lead.SupplierProfileId, request.ContactName, request.ContactEmail }),
            Status = "pending",
            CreatedAt = now
        });

        await _context.SaveChangesAsync(ct);

        // Audit
        await _audit.LogAsync(null, "LeadCreated", "Lead", lead.Id, ipAddress: ipAddress, ct: ct);

        return LeadMapper.MapToDto(lead);
    }

    public async Task<LeadDto> CreateGetListedAsync(GetListedRequest request, string? ipAddress, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;

        // Build the message from the GetListed form fields
        var messageParts = new List<string>
        {
            $"Company: {request.CompanyName}",
            $"Country: {request.Country}",
        };

        if (!string.IsNullOrWhiteSpace(request.City))
            messageParts.Add($"City: {request.City}");
        if (!string.IsNullOrWhiteSpace(request.Website))
            messageParts.Add($"Website: {request.Website}");
        if (!string.IsNullOrWhiteSpace(request.Certifications))
            messageParts.Add($"Certifications: {request.Certifications}");
        if (request.IndustryIds is { Count: > 0 })
            messageParts.Add($"Industry IDs: {string.Join(", ", request.IndustryIds)}");

        messageParts.Add($"Description: {request.Description}");

        var message = string.Join("\n", messageParts);

        // NOTE: SupplierProfileId is NOT NULL on the Lead entity.
        // For "get_listed" leads there is no target supplier. We use Guid.Empty as a sentinel value.
        // A future migration should make SupplierProfileId nullable for get_listed leads.
        var lead = new Lead
        {
            Id = Guid.NewGuid(),
            SupplierProfileId = null,
            ContactName = request.ContactName,
            ContactEmail = request.ContactEmail,
            ContactPhone = request.ContactPhone,
            CompanyName = request.CompanyName,
            Message = message,
            Status = LeadStatus.New,
            LeadType = "get_listed",
            IpAddress = ipAddress,
            CreatedAt = now,
            UpdatedAt = now
        };

        _context.Leads.Add(lead);

        // Queue email notification to admin -- HTML-encode all user-supplied values to prevent XSS
        var adminEmail = _configuration["Notifications:AdminEmail"] ?? "admin@greensuppliers.co.za";
        var safeCompanyName = WebUtility.HtmlEncode(request.CompanyName);
        var safeContactName = WebUtility.HtmlEncode(request.ContactName);
        var safeMessageHtml = WebUtility.HtmlEncode(message);

        _context.EmailQueue.Add(new EmailQueueItem
        {
            Id = Guid.NewGuid(),
            ToEmail = adminEmail,
            ToName = "Green Suppliers Admin",
            Subject = $"New Get Listed request from {safeCompanyName}",
            BodyHtml = $"<p>New listing request from {safeContactName} at {safeCompanyName}</p><p>{safeMessageHtml}</p>",
            TemplateType = "get_listed_notification",
            TemplateData = JsonSerializer.Serialize(new { lead.Id, request.CompanyName, request.ContactName, request.ContactEmail }),
            Status = "pending",
            CreatedAt = now
        });

        await _context.SaveChangesAsync(ct);

        // Audit
        await _audit.LogAsync(null, "GetListedCreated", "Lead", lead.Id, ipAddress: ipAddress, ct: ct);

        return LeadMapper.MapToDto(lead);
    }

    public async Task<PagedResult<LeadDto>> GetAllAsync(int page, int pageSize, string? status, CancellationToken ct = default)
    {
        var query = _context.Leads.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<LeadStatus>(status, ignoreCase: true, out var parsedStatus))
        {
            query = query.Where(l => l.Status == parsedStatus);
        }

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(l => l.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<LeadDto>
        {
            Items = items.Select(LeadMapper.MapToDto).ToList(),
            Page = page,
            PageSize = pageSize,
            Total = total
        };
    }

    public async Task<bool> UpdateStatusAsync(Guid id, string status, Guid adminUserId, CancellationToken ct = default)
    {
        if (!Enum.TryParse<LeadStatus>(status, ignoreCase: true, out var parsedStatus))
            return false;

        var lead = await _context.Leads.FindAsync(new object[] { id }, ct);
        if (lead is null)
            return false;

        var oldStatus = lead.Status.ToString();
        lead.Status = parsedStatus;
        lead.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);

        await _audit.LogAsync(adminUserId, "LeadStatusChanged", "Lead", id,
            oldValues: $"{{\"status\":\"{oldStatus}\"}}",
            newValues: $"{{\"status\":\"{parsedStatus}\"}}", ct: ct);

        return true;
    }

}
