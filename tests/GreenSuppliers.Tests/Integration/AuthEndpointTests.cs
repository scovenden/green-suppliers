using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using GreenSuppliers.Api.Data;
using GreenSuppliers.Api.Models.DTOs;
using GreenSuppliers.Api.Models.Entities;
using GreenSuppliers.Api.Models.Enums;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GreenSuppliers.Tests.Integration;

public class AuthEndpointTests : IClassFixture<AuthEndpointTests.AuthWebAppFactory>
{
    private readonly HttpClient _client;

    public AuthEndpointTests(AuthWebAppFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsJwtToken()
    {
        // Arrange
        var request = new LoginRequest("admin@greensuppliers.co.za", "ChangeMe123!");

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<ApiResponse<LoginResponse>>();
        body.Should().NotBeNull();
        body!.Success.Should().BeTrue();
        body.Data.Should().NotBeNull();
        body.Data!.AccessToken.Should().NotBeNullOrWhiteSpace();
        body.Data.RefreshToken.Should().NotBeNullOrWhiteSpace();
        body.Data.ExpiresAt.Should().BeAfter(DateTime.UtcNow);

        // Decode the JWT and verify claims
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(body.Data.AccessToken);
        jwt.Claims.Should().Contain(c => c.Type == "sub");
        jwt.Claims.Should().Contain(c => c.Type == "email" && c.Value == "admin@greensuppliers.co.za");
        jwt.Claims.Should().Contain(c => c.Type == "role" && c.Value == "Admin");
    }

    [Fact]
    public async Task Login_WithInvalidPassword_Returns401()
    {
        // Arrange
        var request = new LoginRequest("admin@greensuppliers.co.za", "WrongPassword");

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_WithNonExistentEmail_Returns401()
    {
        // Arrange
        var request = new LoginRequest("nobody@test.com", "SomePassword");

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_WithInactiveUser_Returns401()
    {
        // Arrange
        var request = new LoginRequest("inactive@greensuppliers.co.za", "ChangeMe123!");

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Refresh_WithValidToken_ReturnsNewTokens()
    {
        // Arrange — login first to get a refresh token
        var loginRequest = new LoginRequest("admin@greensuppliers.co.za", "ChangeMe123!");
        var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);
        var loginBody = await loginResponse.Content.ReadFromJsonAsync<ApiResponse<LoginResponse>>();
        var refreshToken = loginBody!.Data!.RefreshToken;

        var refreshRequest = new RefreshRequest(refreshToken);

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/refresh", refreshRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<ApiResponse<LoginResponse>>();
        body.Should().NotBeNull();
        body!.Success.Should().BeTrue();
        body.Data!.AccessToken.Should().NotBeNullOrWhiteSpace();
        body.Data.RefreshToken.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Refresh_WithInvalidToken_Returns401()
    {
        // Arrange
        var refreshRequest = new RefreshRequest("invalid-refresh-token");

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/refresh", refreshRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    public class AuthWebAppFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Development");

            builder.ConfigureServices(services =>
            {
                // Remove the real DbContext registration
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<GreenSuppliersDbContext>));
                if (descriptor != null)
                    services.Remove(descriptor);

                // Add InMemory database
                services.AddDbContext<GreenSuppliersDbContext>(options =>
                    options.UseInMemoryDatabase("AuthTests"));

                // Seed test data
                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<GreenSuppliersDbContext>();
                db.Database.EnsureCreated();

                if (!db.Users.Any())
                {
                    var orgId = Guid.NewGuid();
                    db.Organizations.Add(new Organization
                    {
                        Id = orgId,
                        Name = "Test Admin Org",
                        OrganizationType = OrganizationType.Admin,
                        CountryCode = "ZA",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                    });

                    db.Users.Add(new User
                    {
                        Id = Guid.NewGuid(),
                        OrganizationId = orgId,
                        Email = "admin@greensuppliers.co.za",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("ChangeMe123!"),
                        FirstName = "Admin",
                        LastName = "User",
                        Role = UserRole.Admin,
                        IsActive = true,
                        EmailVerified = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                    });

                    // Inactive user for testing
                    db.Users.Add(new User
                    {
                        Id = Guid.NewGuid(),
                        OrganizationId = orgId,
                        Email = "inactive@greensuppliers.co.za",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("ChangeMe123!"),
                        FirstName = "Inactive",
                        LastName = "User",
                        Role = UserRole.Admin,
                        IsActive = false,
                        EmailVerified = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                    });

                    db.SaveChanges();
                }
            });
        }
    }
}
