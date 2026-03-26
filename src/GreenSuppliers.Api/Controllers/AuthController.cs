using System.Threading.RateLimiting;
using GreenSuppliers.Api.Auth;
using GreenSuppliers.Api.Data;
using GreenSuppliers.Api.Models.DTOs;
using GreenSuppliers.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace GreenSuppliers.Api.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly GreenSuppliersDbContext _db;
    private readonly JwtTokenService _jwtTokenService;
    private readonly AccountService _accountService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        GreenSuppliersDbContext db,
        JwtTokenService jwtTokenService,
        AccountService accountService,
        ILogger<AuthController> logger)
    {
        _db = db;
        _jwtTokenService = jwtTokenService;
        _accountService = accountService;
        _logger = logger;
    }

    [HttpPost("login")]
    // Rate limiting applied via middleware in Program.cs
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return Unauthorized(ApiResponse<LoginResponse>.Fail("INVALID_CREDENTIALS", "Invalid email or password."));
        }

        var user = await _db.Users.FirstOrDefaultAsync(
            u => u.Email == request.Email && !u.IsDeleted,
            cancellationToken);

        if (user is null)
        {
            _logger.LogWarning("Login attempt for non-existent email: {Email}", request.Email);
            return Unauthorized(ApiResponse<LoginResponse>.Fail("INVALID_CREDENTIALS", "Invalid email or password."));
        }

        if (!user.IsActive)
        {
            _logger.LogWarning("Login attempt for inactive user: {Email}", request.Email);
            return Unauthorized(ApiResponse<LoginResponse>.Fail("INVALID_CREDENTIALS", "Invalid email or password."));
        }

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            _logger.LogWarning("Failed login attempt for user: {Email}", request.Email);
            return Unauthorized(ApiResponse<LoginResponse>.Fail("INVALID_CREDENTIALS", "Invalid email or password."));
        }

        // Check email verification
        if (!user.EmailVerified)
        {
            _logger.LogWarning("Login attempt for unverified email: {Email}", request.Email);
            return StatusCode(403, ApiResponse<LoginResponse>.Fail("EMAIL_NOT_VERIFIED", "Please verify your email address before logging in."));
        }

        // Update last login timestamp
        user.LastLoginAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);

        var response = _jwtTokenService.GenerateTokens(user);

        _logger.LogInformation("User {Email} logged in successfully", request.Email);

        return Ok(ApiResponse<LoginResponse>.Ok(response));
    }

    [HttpPost("refresh")]
    // Rate limiting applied via middleware in Program.cs
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return Unauthorized(ApiResponse<LoginResponse>.Fail("INVALID_TOKEN", "Invalid refresh token."));
        }

        var principal = _jwtTokenService.ValidateRefreshToken(request.RefreshToken);
        if (principal is null)
        {
            return Unauthorized(ApiResponse<LoginResponse>.Fail("INVALID_TOKEN", "Invalid or expired refresh token."));
        }

        // Extract userId from refresh token (sub claim) and look up user
        var userIdClaim = principal.FindFirst("sub") ?? principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userIdClaim is null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return Unauthorized(ApiResponse<LoginResponse>.Fail("INVALID_TOKEN", "Invalid refresh token."));
        }

        var user = await _db.Users.FirstOrDefaultAsync(
            u => u.Id == userId && u.IsActive && !u.IsDeleted,
            cancellationToken);

        if (user is null)
        {
            return Unauthorized(ApiResponse<LoginResponse>.Fail("INVALID_TOKEN", "User no longer valid."));
        }

        var response = _jwtTokenService.GenerateTokens(user);
        return Ok(ApiResponse<LoginResponse>.Ok(response));
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        var (success, error) = await _accountService.RegisterAsync(request, cancellationToken);

        if (!success)
        {
            if (error == "EMAIL_EXISTS")
            {
                return Conflict(ApiResponse<object>.Fail("EMAIL_EXISTS", "An account with this email address already exists."));
            }

            return BadRequest(ApiResponse<object>.Fail("REGISTRATION_FAILED", error ?? "Registration failed."));
        }

        return StatusCode(201, ApiResponse<object>.Ok(new { message = "Registration successful. Please check your email to verify your account." }));
    }

    [HttpPost("verify-email")]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Token))
        {
            return BadRequest(ApiResponse<object>.Fail("INVALID_TOKEN", "Verification token is required."));
        }

        var success = await _accountService.VerifyEmailAsync(request.Token, cancellationToken);

        if (!success)
        {
            return BadRequest(ApiResponse<object>.Fail("INVALID_TOKEN", "Invalid or expired verification token."));
        }

        return Ok(ApiResponse<object>.Ok(new { message = "Email verified successfully. You can now log in." }));
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
        {
            return BadRequest(ApiResponse<object>.Fail("INVALID_EMAIL", "Email is required."));
        }

        await _accountService.ForgotPasswordAsync(request.Email, cancellationToken);

        // Always return success to prevent email enumeration
        return Ok(ApiResponse<object>.Ok(new { message = "If an account with that email exists, a password reset link has been sent." }));
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request, CancellationToken cancellationToken)
    {
        var success = await _accountService.ResetPasswordAsync(request.Token, request.NewPassword, cancellationToken);

        if (!success)
        {
            return BadRequest(ApiResponse<object>.Fail("INVALID_TOKEN", "Invalid or expired reset token."));
        }

        return Ok(ApiResponse<object>.Ok(new { message = "Password reset successfully. You can now log in with your new password." }));
    }
}
