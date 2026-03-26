using System.Security.Cryptography;
using GreenSuppliers.Api.Data;
using GreenSuppliers.Api.Helpers;
using GreenSuppliers.Api.Models.DTOs;
using GreenSuppliers.Api.Models.Entities;
using GreenSuppliers.Api.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace GreenSuppliers.Api.Services;

public class AccountService
{
    private readonly GreenSuppliersDbContext _context;
    private readonly AuditService _audit;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AccountService> _logger;

    public AccountService(
        GreenSuppliersDbContext context,
        AuditService audit,
        IConfiguration configuration,
        ILogger<AccountService> logger)
    {
        _context = context;
        _audit = audit;
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Register a new supplier or buyer account. Creates Organization + User + (SupplierProfile if supplier).
    /// Queues a verification email.
    /// </summary>
    public async Task<(bool Success, string? Error)> RegisterAsync(RegisterRequest request, CancellationToken ct)
    {
        // Check for duplicate email
        var emailExists = await _context.Users
            .AnyAsync(u => u.Email == request.Email && !u.IsDeleted, ct);

        if (emailExists)
        {
            return (false, "EMAIL_EXISTS");
        }

        var now = DateTime.UtcNow;
        var verificationToken = GenerateToken();
        var isSupplier = request.AccountType == "supplier";

        // Create Organization
        var org = new Organization
        {
            Id = Guid.NewGuid(),
            Name = isSupplier
                ? request.CompanyName!
                : (!string.IsNullOrWhiteSpace(request.CompanyName)
                    ? request.CompanyName
                    : $"Buyer: {request.FirstName}"),
            CountryCode = request.CountryCode.ToUpperInvariant(),
            OrganizationType = isSupplier ? OrganizationType.Supplier : OrganizationType.Buyer,
            CreatedAt = now,
            UpdatedAt = now
        };

        // Create User
        var user = new User
        {
            Id = Guid.NewGuid(),
            OrganizationId = org.Id,
            Email = request.Email.Trim().ToLowerInvariant(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Role = isSupplier ? UserRole.SupplierAdmin : UserRole.Buyer,
            IsActive = true,
            EmailVerified = false,
            EmailVerificationToken = verificationToken,
            EmailVerificationExpiry = now.AddHours(24),
            CreatedAt = now,
            UpdatedAt = now
        };

        _context.Organizations.Add(org);
        _context.Users.Add(user);

        // Create SupplierProfile if supplier
        if (isSupplier)
        {
            var slug = SlugHelper.Slugify(request.CompanyName!);

            // Ensure unique slug
            var slugExists = await _context.SupplierProfiles
                .AnyAsync(sp => sp.Slug == slug, ct);
            if (slugExists)
            {
                slug = $"{slug}-{Guid.NewGuid().ToString("N")[..6]}";
            }

            var profile = new SupplierProfile
            {
                Id = Guid.NewGuid(),
                OrganizationId = org.Id,
                Slug = slug,
                TradingName = request.CompanyName,
                CountryCode = request.CountryCode.ToUpperInvariant(),
                IsPublished = false,
                VerificationStatus = VerificationStatus.Unverified,
                EsgLevel = EsgLevel.None,
                EsgScore = 0,
                CreatedAt = now,
                UpdatedAt = now
            };
            _context.SupplierProfiles.Add(profile);
        }

        // Queue verification email
        var baseUrl = _configuration["Notifications:BaseUrl"] ?? "https://www.greensuppliers.co.za";
        var verifyUrl = $"{baseUrl}/auth/verify-email?token={verificationToken}";

        _context.EmailQueue.Add(new EmailQueueItem
        {
            Id = Guid.NewGuid(),
            ToEmail = user.Email,
            ToName = $"{user.FirstName} {user.LastName}",
            Subject = "Verify your Green Suppliers account",
            BodyHtml = BuildVerificationEmailHtml(user.FirstName, verifyUrl),
            TemplateType = "email_verification",
            Status = "pending",
            CreatedAt = now
        });

        await _context.SaveChangesAsync(ct);

        await _audit.LogAsync(
            user.Id,
            "Register",
            "User",
            user.Id,
            newValues: $"{{\"email\":\"{user.Email}\",\"accountType\":\"{request.AccountType}\",\"orgId\":\"{org.Id}\"}}",
            ct: ct);

        _logger.LogInformation(
            "New {AccountType} registered: {Email}, OrgId={OrgId}, UserId={UserId}",
            request.AccountType, user.Email, org.Id, user.Id);

        return (true, null);
    }

    /// <summary>
    /// Verify a user's email address using the verification token.
    /// </summary>
    public async Task<bool> VerifyEmailAsync(string token, CancellationToken ct)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u =>
                u.EmailVerificationToken == token
                && !u.IsDeleted, ct);

        if (user is null)
        {
            return false;
        }

        if (user.EmailVerificationExpiry.HasValue && user.EmailVerificationExpiry.Value < DateTime.UtcNow)
        {
            _logger.LogWarning("Email verification token expired for user {Email}", user.Email);
            return false;
        }

        user.EmailVerified = true;
        user.EmailVerificationToken = null;
        user.EmailVerificationExpiry = null;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);

        await _audit.LogAsync(
            user.Id,
            "VerifyEmail",
            "User",
            user.Id,
            ct: ct);

        _logger.LogInformation("Email verified for user {Email}", user.Email);
        return true;
    }

    /// <summary>
    /// Initiate a password reset by generating a reset token and queueing a reset email.
    /// Always returns successfully to avoid email enumeration attacks.
    /// </summary>
    public async Task ForgotPasswordAsync(string email, CancellationToken ct)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == normalizedEmail && !u.IsDeleted, ct);

        if (user is null)
        {
            // Do not reveal whether the email exists
            _logger.LogInformation("Forgot password requested for non-existent email: {Email}", normalizedEmail);
            return;
        }

        var now = DateTime.UtcNow;
        var resetToken = GenerateToken();

        user.PasswordResetToken = resetToken;
        user.PasswordResetExpiry = now.AddHours(1);
        user.UpdatedAt = now;

        var baseUrl = _configuration["Notifications:BaseUrl"] ?? "https://www.greensuppliers.co.za";
        var resetUrl = $"{baseUrl}/auth/reset-password?token={resetToken}";

        _context.EmailQueue.Add(new EmailQueueItem
        {
            Id = Guid.NewGuid(),
            ToEmail = user.Email,
            ToName = $"{user.FirstName} {user.LastName}",
            Subject = "Reset your Green Suppliers password",
            BodyHtml = BuildPasswordResetEmailHtml(user.FirstName, resetUrl),
            TemplateType = "password_reset",
            Status = "pending",
            CreatedAt = now
        });

        await _context.SaveChangesAsync(ct);

        await _audit.LogAsync(
            user.Id,
            "ForgotPassword",
            "User",
            user.Id,
            ct: ct);

        _logger.LogInformation("Password reset token generated for user {Email}", user.Email);
    }

    /// <summary>
    /// Reset a user's password using a valid reset token.
    /// </summary>
    public async Task<bool> ResetPasswordAsync(string token, string newPassword, CancellationToken ct)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u =>
                u.PasswordResetToken == token
                && !u.IsDeleted, ct);

        if (user is null)
        {
            return false;
        }

        if (user.PasswordResetExpiry.HasValue && user.PasswordResetExpiry.Value < DateTime.UtcNow)
        {
            _logger.LogWarning("Password reset token expired for user {Email}", user.Email);
            return false;
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        user.PasswordResetToken = null;
        user.PasswordResetExpiry = null;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);

        await _audit.LogAsync(
            user.Id,
            "ResetPassword",
            "User",
            user.Id,
            ct: ct);

        _logger.LogInformation("Password reset completed for user {Email}", user.Email);
        return true;
    }

    /// <summary>
    /// Generates a cryptographically secure 64-character hex token.
    /// </summary>
    private static string GenerateToken()
    {
        return Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
    }

    private static string BuildVerificationEmailHtml(string firstName, string verifyUrl)
    {
        return $"""
            <!DOCTYPE html>
            <html>
            <head><meta charset="utf-8"></head>
            <body style="font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;">
                <h2 style="color: #16A34A;">Welcome to Green Suppliers!</h2>
                <p>Hi {firstName},</p>
                <p>Thank you for registering. Please verify your email address by clicking the link below:</p>
                <p style="margin: 24px 0;">
                    <a href="{verifyUrl}"
                       style="background-color: #16A34A; color: white; padding: 12px 24px; text-decoration: none; border-radius: 6px; display: inline-block;">
                        Verify Email Address
                    </a>
                </p>
                <p>Or copy and paste this URL into your browser:</p>
                <p style="word-break: break-all; color: #059669;">{verifyUrl}</p>
                <p>This link expires in 24 hours.</p>
                <hr style="border: none; border-top: 1px solid #D6D3D1; margin: 24px 0;">
                <p style="color: #78716C; font-size: 12px;">
                    If you did not create an account, please ignore this email.
                </p>
            </body>
            </html>
            """;
    }

    private static string BuildPasswordResetEmailHtml(string firstName, string resetUrl)
    {
        return $"""
            <!DOCTYPE html>
            <html>
            <head><meta charset="utf-8"></head>
            <body style="font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;">
                <h2 style="color: #16A34A;">Reset Your Password</h2>
                <p>Hi {firstName},</p>
                <p>We received a request to reset your password. Click the link below to set a new password:</p>
                <p style="margin: 24px 0;">
                    <a href="{resetUrl}"
                       style="background-color: #16A34A; color: white; padding: 12px 24px; text-decoration: none; border-radius: 6px; display: inline-block;">
                        Reset Password
                    </a>
                </p>
                <p>Or copy and paste this URL into your browser:</p>
                <p style="word-break: break-all; color: #059669;">{resetUrl}</p>
                <p>This link expires in 1 hour.</p>
                <hr style="border: none; border-top: 1px solid #D6D3D1; margin: 24px 0;">
                <p style="color: #78716C; font-size: 12px;">
                    If you did not request a password reset, please ignore this email. Your password will remain unchanged.
                </p>
            </body>
            </html>
            """;
    }
}
