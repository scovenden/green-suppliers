namespace GreenSuppliers.Api.Models.DTOs;

public class PagedResult<T>
{
    public List<T> Items { get; init; } = new();
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int Total { get; init; }
    public int TotalPages => (int)Math.Ceiling(Total / (double)PageSize);
}
