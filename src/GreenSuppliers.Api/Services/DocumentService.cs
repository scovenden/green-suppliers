using GreenSuppliers.Api.Data;
using GreenSuppliers.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace GreenSuppliers.Api.Services;

public class DocumentService
{
    private readonly GreenSuppliersDbContext _context;

    public DocumentService(GreenSuppliersDbContext context)
    {
        _context = context;
    }

    public async Task<Document> CreateAsync(Guid supplierProfileId, string fileName, string blobUrl,
        string contentType, long fileSizeBytes, string documentType, Guid? uploadedByUserId)
    {
        var document = new Document
        {
            Id = Guid.NewGuid(),
            SupplierProfileId = supplierProfileId,
            FileName = fileName,
            BlobUrl = blobUrl,
            ContentType = contentType,
            FileSizeBytes = fileSizeBytes,
            DocumentType = documentType,
            UploadedByUserId = uploadedByUserId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Documents.Add(document);
        await _context.SaveChangesAsync();

        return document;
    }

    public async Task<List<Document>> GetBySupplierAsync(Guid supplierProfileId)
    {
        return await _context.Documents
            .AsNoTracking()
            .Where(d => d.SupplierProfileId == supplierProfileId)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync();
    }
}
