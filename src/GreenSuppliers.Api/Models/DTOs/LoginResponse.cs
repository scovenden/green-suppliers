namespace GreenSuppliers.Api.Models.DTOs;

public record LoginResponse(string AccessToken, string RefreshToken, DateTime ExpiresAt);

public record RefreshRequest(string RefreshToken);
