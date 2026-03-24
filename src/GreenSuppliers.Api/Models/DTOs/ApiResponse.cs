namespace GreenSuppliers.Api.Models.DTOs;

public class ApiResponse<T>
{
    public bool Success { get; init; }
    public T? Data { get; init; }
    public ApiError? Error { get; init; }
    public PaginationMeta? Meta { get; init; }

    public static ApiResponse<T> Ok(T data) =>
        new() { Success = true, Data = data };

    public static ApiResponse<T> Ok(T data, PaginationMeta meta) =>
        new() { Success = true, Data = data, Meta = meta };

    public static ApiResponse<T> Fail(string code, string message) =>
        new() { Success = false, Error = new ApiError(code, message) };

    public static ApiResponse<T> Fail(string code, string message, Dictionary<string, string[]> details) =>
        new() { Success = false, Error = new ApiError(code, message) { Details = details } };
}

public record ApiError(string Code, string Message)
{
    public Dictionary<string, string[]>? Details { get; init; }
}

public record PaginationMeta(int Page, int PageSize, int Total, int TotalPages);
