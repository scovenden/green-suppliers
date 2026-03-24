using GreenSuppliers.Api.Models.DTOs;

namespace GreenSuppliers.Api.Services;

public interface ISupplierSearchService
{
    Task<PagedResult<SupplierSearchResult>> SearchAsync(SupplierSearchQuery query, CancellationToken ct = default);
}
