namespace GreenSuppliers.Api.Models.DTOs;

public record ResetPasswordRequest(string Token, string NewPassword);
