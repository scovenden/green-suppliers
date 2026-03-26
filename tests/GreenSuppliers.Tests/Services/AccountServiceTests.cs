using FluentAssertions;
using GreenSuppliers.Api.Data;
using GreenSuppliers.Api.Models.DTOs;
using GreenSuppliers.Api.Models.Enums;
using GreenSuppliers.Api.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace GreenSuppliers.Tests.Services;

public class AccountServiceTests : IDisposable
{
    private readonly GreenSuppliersDbContext _db;
    private readonly AccountService _sut;
    private readonly AuditService _audit;

    public AccountServiceTests()
    {
        var options = new DbContextOptionsBuilder<GreenSuppliersDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _db = new GreenSuppliersDbContext(options);

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Notifications:BaseUrl", "https://www.greensuppliers.co.za" },
                { "Notifications:AdminEmail", "admin@greensuppliers.co.za" }
            })
            .Build();

        _audit = new AuditService(_db);

        _sut = new AccountService(
            _db,
            _audit,
            config,
            Mock.Of<ILogger<AccountService>>());
    }

    public void Dispose()
    {
        _db.Dispose();
    }

    private static RegisterRequest CreateSupplierRequest(string? email = null) => new(
        Email: email ?? $"supplier-{Guid.NewGuid():N}@example.com",
        Password: "SecurePass1",
        FirstName: "Jane",
        LastName: "Green",
        CompanyName: "EcoCorp",
        CountryCode: "ZA",
        AccountType: "supplier"
    );

    private static RegisterRequest CreateBuyerRequest(string? email = null) => new(
        Email: email ?? $"buyer-{Guid.NewGuid():N}@example.com",
        Password: "SecurePass1",
        FirstName: "John",
        LastName: "Buyer",
        CompanyName: null,
        CountryCode: "ZA",
        AccountType: "buyer"
    );

    [Fact]
    public async Task Register_Supplier_CreatesOrgAndUserAndProfile()
    {
        // Arrange
        var request = CreateSupplierRequest();

        // Act
        var (success, error) = await _sut.RegisterAsync(request, CancellationToken.None);

        // Assert
        success.Should().BeTrue();
        error.Should().BeNull();

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == request.Email.ToLowerInvariant());
        user.Should().NotBeNull();
        user!.Role.Should().Be(UserRole.SupplierAdmin);
        user.EmailVerified.Should().BeFalse();
        user.EmailVerificationToken.Should().NotBeNullOrEmpty();
        user.EmailVerificationToken!.Length.Should().Be(64);
        user.EmailVerificationExpiry.Should().BeAfter(DateTime.UtcNow);

        var org = await _db.Organizations.FirstOrDefaultAsync(o => o.Id == user.OrganizationId);
        org.Should().NotBeNull();
        org!.Name.Should().Be("EcoCorp");
        org.OrganizationType.Should().Be(OrganizationType.Supplier);
        org.CountryCode.Should().Be("ZA");

        var profile = await _db.SupplierProfiles.FirstOrDefaultAsync(sp => sp.OrganizationId == org.Id);
        profile.Should().NotBeNull();
        profile!.IsPublished.Should().BeFalse();
        profile.TradingName.Should().Be("EcoCorp");
        profile.VerificationStatus.Should().Be(VerificationStatus.Unverified);
        profile.EsgLevel.Should().Be(EsgLevel.None);

        // Verification email should be queued
        var emailItem = await _db.EmailQueue.FirstOrDefaultAsync(e => e.ToEmail == request.Email.ToLowerInvariant());
        emailItem.Should().NotBeNull();
        emailItem!.TemplateType.Should().Be("email_verification");
        emailItem.Status.Should().Be("pending");
    }

    [Fact]
    public async Task Register_Buyer_CreatesOrgAndUser()
    {
        // Arrange
        var request = CreateBuyerRequest();

        // Act
        var (success, error) = await _sut.RegisterAsync(request, CancellationToken.None);

        // Assert
        success.Should().BeTrue();
        error.Should().BeNull();

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == request.Email.ToLowerInvariant());
        user.Should().NotBeNull();
        user!.Role.Should().Be(UserRole.Buyer);
        user.EmailVerified.Should().BeFalse();

        var org = await _db.Organizations.FirstOrDefaultAsync(o => o.Id == user.OrganizationId);
        org.Should().NotBeNull();
        org!.Name.Should().Be("Buyer: John");
        org.OrganizationType.Should().Be(OrganizationType.Buyer);

        // No supplier profile should be created for buyers
        var profile = await _db.SupplierProfiles.FirstOrDefaultAsync(sp => sp.OrganizationId == org.Id);
        profile.Should().BeNull();

        // Verification email should be queued
        var emailItem = await _db.EmailQueue.FirstOrDefaultAsync(e => e.ToEmail == request.Email.ToLowerInvariant());
        emailItem.Should().NotBeNull();
    }

    [Fact]
    public async Task Register_DuplicateEmail_ReturnsFalse()
    {
        // Arrange
        var email = "duplicate@example.com";
        var request1 = CreateSupplierRequest(email);
        await _sut.RegisterAsync(request1, CancellationToken.None);

        var request2 = CreateBuyerRequest(email);

        // Act
        var (success, error) = await _sut.RegisterAsync(request2, CancellationToken.None);

        // Assert
        success.Should().BeFalse();
        error.Should().Be("EMAIL_EXISTS");
    }

    [Fact]
    public async Task VerifyEmail_ValidToken_SetsEmailVerified()
    {
        // Arrange
        var request = CreateSupplierRequest();
        await _sut.RegisterAsync(request, CancellationToken.None);

        var user = await _db.Users.FirstAsync(u => u.Email == request.Email.ToLowerInvariant());
        var token = user.EmailVerificationToken!;

        // Act
        var result = await _sut.VerifyEmailAsync(token, CancellationToken.None);

        // Assert
        result.Should().BeTrue();

        await _db.Entry(user).ReloadAsync();
        user.EmailVerified.Should().BeTrue();
        user.EmailVerificationToken.Should().BeNull();
        user.EmailVerificationExpiry.Should().BeNull();
    }

    [Fact]
    public async Task VerifyEmail_ExpiredToken_ReturnsFalse()
    {
        // Arrange
        var request = CreateSupplierRequest();
        await _sut.RegisterAsync(request, CancellationToken.None);

        var user = await _db.Users.FirstAsync(u => u.Email == request.Email.ToLowerInvariant());
        var token = user.EmailVerificationToken!;

        // Force expire the token
        user.EmailVerificationExpiry = DateTime.UtcNow.AddHours(-1);
        await _db.SaveChangesAsync();

        // Act
        var result = await _sut.VerifyEmailAsync(token, CancellationToken.None);

        // Assert
        result.Should().BeFalse();

        await _db.Entry(user).ReloadAsync();
        user.EmailVerified.Should().BeFalse();
    }

    [Fact]
    public async Task ForgotPassword_ExistingEmail_QueuesEmail()
    {
        // Arrange
        var request = CreateSupplierRequest();
        await _sut.RegisterAsync(request, CancellationToken.None);

        // Clear any registration emails from the queue
        var registrationEmails = _db.EmailQueue.Where(e => e.TemplateType == "email_verification");
        _db.EmailQueue.RemoveRange(registrationEmails);
        await _db.SaveChangesAsync();

        // Act
        await _sut.ForgotPasswordAsync(request.Email, CancellationToken.None);

        // Assert
        var user = await _db.Users.FirstAsync(u => u.Email == request.Email.ToLowerInvariant());
        user.PasswordResetToken.Should().NotBeNullOrEmpty();
        user.PasswordResetToken!.Length.Should().Be(64);
        user.PasswordResetExpiry.Should().BeAfter(DateTime.UtcNow);

        var resetEmail = await _db.EmailQueue.FirstOrDefaultAsync(e => e.TemplateType == "password_reset");
        resetEmail.Should().NotBeNull();
        resetEmail!.ToEmail.Should().Be(request.Email.ToLowerInvariant());
        resetEmail.Status.Should().Be("pending");
    }

    [Fact]
    public async Task ResetPassword_ValidToken_UpdatesHash()
    {
        // Arrange
        var request = CreateSupplierRequest();
        await _sut.RegisterAsync(request, CancellationToken.None);
        await _sut.ForgotPasswordAsync(request.Email, CancellationToken.None);

        var user = await _db.Users.FirstAsync(u => u.Email == request.Email.ToLowerInvariant());
        var token = user.PasswordResetToken!;
        var oldHash = user.PasswordHash;

        // Act
        var result = await _sut.ResetPasswordAsync(token, "NewSecure1Pass", CancellationToken.None);

        // Assert
        result.Should().BeTrue();

        await _db.Entry(user).ReloadAsync();
        user.PasswordHash.Should().NotBe(oldHash);
        user.PasswordResetToken.Should().BeNull();
        user.PasswordResetExpiry.Should().BeNull();

        // Verify the new password works
        BCrypt.Net.BCrypt.Verify("NewSecure1Pass", user.PasswordHash).Should().BeTrue();
    }

    [Fact]
    public async Task ResetPassword_ExpiredToken_ReturnsFalse()
    {
        // Arrange
        var request = CreateSupplierRequest();
        await _sut.RegisterAsync(request, CancellationToken.None);
        await _sut.ForgotPasswordAsync(request.Email, CancellationToken.None);

        var user = await _db.Users.FirstAsync(u => u.Email == request.Email.ToLowerInvariant());
        var token = user.PasswordResetToken!;

        // Force expire the token
        user.PasswordResetExpiry = DateTime.UtcNow.AddHours(-1);
        await _db.SaveChangesAsync();

        // Act
        var result = await _sut.ResetPasswordAsync(token, "NewSecure1Pass", CancellationToken.None);

        // Assert
        result.Should().BeFalse();
    }

    // =========================================================================
    // Sprint 1 — Additional AccountService coverage
    // =========================================================================

    [Fact]
    public async Task Register_BuyerWithCompanyName_UsesCompanyNameForOrg()
    {
        // Arrange — buyer can optionally provide a company name
        var request = new RegisterRequest(
            Email: $"buyer-corp-{Guid.NewGuid():N}@example.com",
            Password: "SecurePass1",
            FirstName: "Alice",
            LastName: "Corporate",
            CompanyName: "Buyer Corp Ltd",
            CountryCode: "ZA",
            AccountType: "buyer"
        );

        // Act
        var (success, error) = await _sut.RegisterAsync(request, CancellationToken.None);

        // Assert
        success.Should().BeTrue();
        error.Should().BeNull();

        var user = await _db.Users.FirstAsync(u => u.Email == request.Email.ToLowerInvariant());
        var org = await _db.Organizations.FirstAsync(o => o.Id == user.OrganizationId);
        org.Name.Should().Be("Buyer Corp Ltd");
        org.OrganizationType.Should().Be(OrganizationType.Buyer);
    }

    [Fact]
    public async Task Register_NormalizesEmailToLowerCase()
    {
        // Arrange — email with mixed casing
        var request = new RegisterRequest(
            Email: "UPPERCASE@Example.COM",
            Password: "SecurePass1",
            FirstName: "Jane",
            LastName: "Upper",
            CompanyName: "UpperCorp",
            CountryCode: "ZA",
            AccountType: "supplier"
        );

        // Act
        var (success, _) = await _sut.RegisterAsync(request, CancellationToken.None);

        // Assert
        success.Should().BeTrue();
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == "uppercase@example.com");
        user.Should().NotBeNull();
    }

    [Fact]
    public async Task Register_TrimsFirstNameAndLastName()
    {
        // Arrange
        var request = new RegisterRequest(
            Email: $"trimtest-{Guid.NewGuid():N}@example.com",
            Password: "SecurePass1",
            FirstName: "  Jane  ",
            LastName: "  Green  ",
            CompanyName: "TrimCorp",
            CountryCode: "ZA",
            AccountType: "supplier"
        );

        // Act
        await _sut.RegisterAsync(request, CancellationToken.None);

        // Assert
        var user = await _db.Users.FirstAsync(u => u.Email == request.Email.ToLowerInvariant());
        user.FirstName.Should().Be("Jane");
        user.LastName.Should().Be("Green");
    }

    [Fact]
    public async Task Register_CountryCodeIsUppercased()
    {
        // Arrange — lowercase country code
        var request = new RegisterRequest(
            Email: $"lower-cc-{Guid.NewGuid():N}@example.com",
            Password: "SecurePass1",
            FirstName: "Jane",
            LastName: "Green",
            CompanyName: "LowCountryCorp",
            CountryCode: "za",
            AccountType: "supplier"
        );

        // Act
        await _sut.RegisterAsync(request, CancellationToken.None);

        // Assert
        var org = await _db.Organizations.FirstAsync(o => o.Name == "LowCountryCorp");
        org.CountryCode.Should().Be("ZA");

        var profile = await _db.SupplierProfiles.FirstAsync(sp => sp.OrganizationId == org.Id);
        profile.CountryCode.Should().Be("ZA");
    }

    [Fact]
    public async Task Register_PasswordIsHashedNotStoredPlaintext()
    {
        // Arrange
        var request = CreateSupplierRequest();

        // Act
        await _sut.RegisterAsync(request, CancellationToken.None);

        // Assert
        var user = await _db.Users.FirstAsync(u => u.Email == request.Email.ToLowerInvariant());
        user.PasswordHash.Should().NotBe(request.Password);
        BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash).Should().BeTrue();
    }

    [Fact]
    public async Task Register_Supplier_GeneratesSlugFromCompanyName()
    {
        // Arrange
        var request = new RegisterRequest(
            Email: $"slug-test-{Guid.NewGuid():N}@example.com",
            Password: "SecurePass1",
            FirstName: "Jane",
            LastName: "Green",
            CompanyName: "Eco Solutions SA",
            CountryCode: "ZA",
            AccountType: "supplier"
        );

        // Act
        await _sut.RegisterAsync(request, CancellationToken.None);

        // Assert
        var profile = await _db.SupplierProfiles.FirstAsync();
        profile.Slug.Should().Be("eco-solutions-sa");
    }

    [Fact]
    public async Task Register_Supplier_DuplicateSlug_AppendsSuffix()
    {
        // Arrange — register two suppliers with the same company name
        var request1 = new RegisterRequest(
            Email: $"slug1-{Guid.NewGuid():N}@example.com",
            Password: "SecurePass1",
            FirstName: "Jane",
            LastName: "Green",
            CompanyName: "DupeSlug Corp",
            CountryCode: "ZA",
            AccountType: "supplier"
        );
        var request2 = new RegisterRequest(
            Email: $"slug2-{Guid.NewGuid():N}@example.com",
            Password: "SecurePass1",
            FirstName: "John",
            LastName: "Blue",
            CompanyName: "DupeSlug Corp",
            CountryCode: "ZA",
            AccountType: "supplier"
        );

        // Act
        await _sut.RegisterAsync(request1, CancellationToken.None);
        await _sut.RegisterAsync(request2, CancellationToken.None);

        // Assert
        var profiles = await _db.SupplierProfiles.ToListAsync();
        profiles.Should().HaveCount(2);
        profiles.Select(p => p.Slug).Should().OnlyHaveUniqueItems();
        profiles[0].Slug.Should().Be("dupeslug-corp");
        profiles[1].Slug.Should().StartWith("dupeslug-corp-");
    }

    [Fact]
    public async Task Register_UserIsActive_ByDefault()
    {
        // Arrange
        var request = CreateSupplierRequest();

        // Act
        await _sut.RegisterAsync(request, CancellationToken.None);

        // Assert
        var user = await _db.Users.FirstAsync(u => u.Email == request.Email.ToLowerInvariant());
        user.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Register_WritesAuditLog()
    {
        // Arrange
        var request = CreateSupplierRequest();

        // Act
        await _sut.RegisterAsync(request, CancellationToken.None);

        // Assert
        var auditEvent = await _db.AuditEvents.FirstOrDefaultAsync(a => a.Action == "Register");
        auditEvent.Should().NotBeNull();
        auditEvent!.EntityType.Should().Be("User");
    }

    [Fact]
    public async Task Register_VerificationTokenExpiry_Is24Hours()
    {
        // Arrange
        var request = CreateSupplierRequest();

        // Act
        await _sut.RegisterAsync(request, CancellationToken.None);

        // Assert
        var user = await _db.Users.FirstAsync(u => u.Email == request.Email.ToLowerInvariant());
        user.EmailVerificationExpiry.Should().NotBeNull();
        var expiry = user.EmailVerificationExpiry!.Value;
        // Expiry should be approximately 24 hours from now (within 5 minutes tolerance)
        expiry.Should().BeCloseTo(DateTime.UtcNow.AddHours(24), TimeSpan.FromMinutes(5));
    }

    [Fact]
    public async Task VerifyEmail_InvalidToken_ReturnsFalse()
    {
        // Act — token that does not exist in the database
        var result = await _sut.VerifyEmailAsync("NONEXISTENT_TOKEN_ABCDEF1234567890", CancellationToken.None);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task VerifyEmail_AlreadyVerified_ReturnsFalse()
    {
        // Arrange — register, verify once, then try to verify again
        var request = CreateSupplierRequest();
        await _sut.RegisterAsync(request, CancellationToken.None);

        var user = await _db.Users.FirstAsync(u => u.Email == request.Email.ToLowerInvariant());
        var token = user.EmailVerificationToken!;

        // First verification succeeds
        var firstResult = await _sut.VerifyEmailAsync(token, CancellationToken.None);
        firstResult.Should().BeTrue();

        // Act — second verification with same token should fail (token was cleared)
        var secondResult = await _sut.VerifyEmailAsync(token, CancellationToken.None);

        // Assert
        secondResult.Should().BeFalse();
    }

    [Fact]
    public async Task VerifyEmail_WritesAuditLog()
    {
        // Arrange
        var request = CreateSupplierRequest();
        await _sut.RegisterAsync(request, CancellationToken.None);
        var user = await _db.Users.FirstAsync(u => u.Email == request.Email.ToLowerInvariant());
        var token = user.EmailVerificationToken!;

        // Act
        await _sut.VerifyEmailAsync(token, CancellationToken.None);

        // Assert
        var auditEvent = await _db.AuditEvents.FirstOrDefaultAsync(a => a.Action == "VerifyEmail");
        auditEvent.Should().NotBeNull();
        auditEvent!.EntityType.Should().Be("User");
        auditEvent.EntityId.Should().Be(user.Id);
    }

    [Fact]
    public async Task ForgotPassword_NonExistentEmail_DoesNotQueueEmail()
    {
        // Act — email that does not exist
        await _sut.ForgotPasswordAsync("nobody@doesnotexist.com", CancellationToken.None);

        // Assert — no email should be queued
        var emailCount = await _db.EmailQueue.CountAsync();
        emailCount.Should().Be(0);
    }

    [Fact]
    public async Task ForgotPassword_NormalizesEmail()
    {
        // Arrange — register with lowercase
        var request = CreateSupplierRequest("normalizetest@example.com");
        await _sut.RegisterAsync(request, CancellationToken.None);

        // Clear registration emails
        _db.EmailQueue.RemoveRange(_db.EmailQueue);
        await _db.SaveChangesAsync();

        // Act — send forgot password with uppercase email
        await _sut.ForgotPasswordAsync("  NORMALIZETEST@EXAMPLE.COM  ", CancellationToken.None);

        // Assert — should still find the user and queue the email
        var resetEmail = await _db.EmailQueue.FirstOrDefaultAsync(e => e.TemplateType == "password_reset");
        resetEmail.Should().NotBeNull();
    }

    [Fact]
    public async Task ForgotPassword_ResetTokenExpiry_Is1Hour()
    {
        // Arrange
        var request = CreateSupplierRequest();
        await _sut.RegisterAsync(request, CancellationToken.None);

        // Act
        await _sut.ForgotPasswordAsync(request.Email, CancellationToken.None);

        // Assert
        var user = await _db.Users.FirstAsync(u => u.Email == request.Email.ToLowerInvariant());
        user.PasswordResetExpiry.Should().NotBeNull();
        user.PasswordResetExpiry!.Value.Should().BeCloseTo(DateTime.UtcNow.AddHours(1), TimeSpan.FromMinutes(5));
    }

    [Fact]
    public async Task ForgotPassword_WritesAuditLog()
    {
        // Arrange
        var request = CreateSupplierRequest();
        await _sut.RegisterAsync(request, CancellationToken.None);

        // Act
        await _sut.ForgotPasswordAsync(request.Email, CancellationToken.None);

        // Assert
        var auditEvent = await _db.AuditEvents.FirstOrDefaultAsync(a => a.Action == "ForgotPassword");
        auditEvent.Should().NotBeNull();
        auditEvent!.EntityType.Should().Be("User");
    }

    [Fact]
    public async Task ResetPassword_InvalidToken_ReturnsFalse()
    {
        // Act — token that does not exist in the database
        var result = await _sut.ResetPasswordAsync("NONEXISTENT_TOKEN_XYZ", "NewSecure1Pass", CancellationToken.None);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ResetPassword_ClearsTokenAfterSuccess()
    {
        // Arrange
        var request = CreateSupplierRequest();
        await _sut.RegisterAsync(request, CancellationToken.None);
        await _sut.ForgotPasswordAsync(request.Email, CancellationToken.None);

        var user = await _db.Users.FirstAsync(u => u.Email == request.Email.ToLowerInvariant());
        var token = user.PasswordResetToken!;

        // Act — reset password once
        await _sut.ResetPasswordAsync(token, "NewSecure1Pass", CancellationToken.None);

        // Try again with the same token — should fail (token was cleared)
        var result = await _sut.ResetPasswordAsync(token, "AnotherPass1", CancellationToken.None);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ResetPassword_WritesAuditLog()
    {
        // Arrange
        var request = CreateSupplierRequest();
        await _sut.RegisterAsync(request, CancellationToken.None);
        await _sut.ForgotPasswordAsync(request.Email, CancellationToken.None);

        var user = await _db.Users.FirstAsync(u => u.Email == request.Email.ToLowerInvariant());
        var token = user.PasswordResetToken!;

        // Act
        await _sut.ResetPasswordAsync(token, "NewSecure1Pass", CancellationToken.None);

        // Assert
        var auditEvent = await _db.AuditEvents.FirstOrDefaultAsync(a => a.Action == "ResetPassword");
        auditEvent.Should().NotBeNull();
        auditEvent!.EntityType.Should().Be("User");
    }

    [Fact]
    public async Task ForgotPassword_MultipleCalls_OverwritesPreviousToken()
    {
        // Arrange
        var request = CreateSupplierRequest();
        await _sut.RegisterAsync(request, CancellationToken.None);

        await _sut.ForgotPasswordAsync(request.Email, CancellationToken.None);
        var user = await _db.Users.FirstAsync(u => u.Email == request.Email.ToLowerInvariant());
        var firstToken = user.PasswordResetToken;

        // Act — request another reset
        await _sut.ForgotPasswordAsync(request.Email, CancellationToken.None);

        // Assert
        await _db.Entry(user).ReloadAsync();
        user.PasswordResetToken.Should().NotBe(firstToken);
        user.PasswordResetToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Register_Supplier_ProfileIsUnpublished()
    {
        // Arrange
        var request = CreateSupplierRequest();

        // Act
        await _sut.RegisterAsync(request, CancellationToken.None);

        // Assert — self-registered supplier profiles should start unpublished
        var profile = await _db.SupplierProfiles.FirstAsync();
        profile.IsPublished.Should().BeFalse();
    }

    [Fact]
    public async Task Register_VerificationEmailContainsCorrectUrl()
    {
        // Arrange
        var request = CreateSupplierRequest();

        // Act
        await _sut.RegisterAsync(request, CancellationToken.None);

        // Assert
        var emailItem = await _db.EmailQueue.FirstAsync(e => e.TemplateType == "email_verification");
        emailItem.BodyHtml.Should().Contain("https://www.greensuppliers.co.za/auth/verify-email?token=");
        emailItem.Subject.Should().Contain("Verify your Green Suppliers account");
    }

    [Fact]
    public async Task ForgotPassword_ResetEmailContainsCorrectUrl()
    {
        // Arrange
        var request = CreateSupplierRequest();
        await _sut.RegisterAsync(request, CancellationToken.None);
        _db.EmailQueue.RemoveRange(_db.EmailQueue);
        await _db.SaveChangesAsync();

        // Act
        await _sut.ForgotPasswordAsync(request.Email, CancellationToken.None);

        // Assert
        var emailItem = await _db.EmailQueue.FirstAsync(e => e.TemplateType == "password_reset");
        emailItem.BodyHtml.Should().Contain("https://www.greensuppliers.co.za/auth/reset-password?token=");
        emailItem.Subject.Should().Contain("Reset your Green Suppliers password");
    }
}
