using GreenSuppliers.Api.Data;
using GreenSuppliers.Api.Models.Entities;

namespace GreenSuppliers.Api.Services;

public class AuditService
{
    private readonly GreenSuppliersDbContext _context;

    public AuditService(GreenSuppliersDbContext context)
    {
        _context = context;
    }

    public async Task LogAsync(Guid? userId, string action, string entityType, Guid entityId,
        string? oldValues = null, string? newValues = null, string? ipAddress = null)
    {
        _context.AuditEvents.Add(new AuditEvent
        {
            Id = Guid.NewGuid(),
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            UserId = userId,
            OldValues = oldValues,
            NewValues = newValues,
            IpAddress = ipAddress,
            CreatedAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();
    }
}
