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
}
