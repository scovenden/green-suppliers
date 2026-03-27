using System.Text;
using System.Threading.RateLimiting;
using FluentValidation;
using FluentValidation.AspNetCore;
using GreenSuppliers.Api.Auth;
using GreenSuppliers.Api.Data;
using GreenSuppliers.Api.Middleware;
using GreenSuppliers.Api.Services;
using GreenSuppliers.Api.Validators;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<CreateSupplierValidator>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database
builder.Services.AddDbContext<GreenSuppliersDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Application services
builder.Services.AddScoped<ISupplierSearchService, SqlFullTextSearchService>();
builder.Services.AddScoped<AuditService>();
builder.Services.AddScoped<EsgScoringService>();
builder.Services.AddScoped<VerificationService>();
builder.Services.AddScoped<ScoringRunner>();
builder.Services.AddScoped<SupplierService>();
builder.Services.AddScoped<LeadService>();
builder.Services.AddScoped<TaxonomyService>();
builder.Services.AddScoped<ContentService>();
builder.Services.AddScoped<DocumentService>();
builder.Services.AddScoped<AccountService>();
builder.Services.AddScoped<SupplierMeService>();
builder.Services.AddScoped<SupplierMeLeadService>();
builder.Services.AddScoped<BuyerService>();
builder.Services.Configure<PayFastSettings>(builder.Configuration.GetSection("PayFast"));
builder.Services.AddScoped<PayFastService>();
builder.Services.AddScoped<BillingService>();
builder.Services.AddScoped<SdgService>();
builder.Services.AddScoped<ProfileAnalyticsService>();
builder.Services.AddScoped<PlacementService>();

// JWT Authentication
builder.Services.AddScoped<JwtTokenService>();

// SECURITY: Validate JWT secret is set and strong enough
var jwtSecret = builder.Configuration["Jwt:Secret"];
if (string.IsNullOrEmpty(jwtSecret) || jwtSecret.Contains("MUST-BE-SET") || jwtSecret.Length < 32)
{
    if (!builder.Environment.IsDevelopment())
    {
        throw new InvalidOperationException(
            "JWT secret must be set via environment variable or Key Vault in production. " +
            "Minimum 32 characters. Set Jwt:Secret in configuration.");
    }
}

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"]!))
        };

        // CRITICAL: Reject refresh tokens used as Bearer access tokens.
        // Both token types share the same signing key, so without this check
        // a refresh token (7-day lifetime) can authenticate any [Authorize] endpoint.
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = context =>
            {
                var tokenType = context.Principal?.FindFirst("token_type")?.Value;
                if (tokenType != "access")
                {
                    context.Fail("Token is not an access token.");
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Admin", policy => policy.RequireClaim("role", "Admin"));
    options.AddPolicy("Supplier", policy => policy.RequireClaim("role", "SupplierAdmin", "SupplierUser"));
    options.AddPolicy("Buyer", policy => policy.RequireClaim("role", "Buyer"));
});

// Rate Limiting (global, applied to auth and public submission endpoints via path matching)
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
    {
        var path = context.Request.Path.Value ?? "";
        // Auth endpoints: strict rate limiting (10 per 15 min in production)
        if (path.Contains("/auth/login") || path.Contains("/auth/refresh")
            || path.Contains("/auth/register") || path.Contains("/auth/forgot-password")
            || path.Contains("/auth/reset-password"))
        {
            return RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: $"auth:{context.Connection.RemoteIpAddress?.ToString() ?? "unknown"}",
                factory: _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = builder.Environment.IsDevelopment() ? 1000 : 10,
                    Window = TimeSpan.FromMinutes(15),
                    QueueLimit = 0,
                });
        }
        // Lead submission and get-listed: rate limit to prevent spam (20 per 15 min)
        if ((path.Contains("/leads") || path.Contains("/get-listed"))
            && context.Request.Method == "POST")
        {
            return RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: $"leads:{context.Connection.RemoteIpAddress?.ToString() ?? "unknown"}",
                factory: _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = builder.Environment.IsDevelopment() ? 1000 : 20,
                    Window = TimeSpan.FromMinutes(15),
                    QueueLimit = 0,
                });
        }
        return RateLimitPartition.GetNoLimiter("");
    });
    options.RejectionStatusCode = 429;
    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.Headers["Retry-After"] = "900"; // 15 minutes
        context.HttpContext.Response.StatusCode = 429;
        context.HttpContext.Response.ContentType = "application/json";
        await context.HttpContext.Response.WriteAsync(
            "{\"success\":false,\"data\":null,\"error\":{\"code\":\"RATE_LIMITED\",\"message\":\"Too many requests. Please try again later.\"}}",
            cancellationToken);
    };
});

// Health Checks
builder.Services.AddHealthChecks()
    .AddSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")!);

// CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
            ?? new[] { "http://localhost:3000" };
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Seed data
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<GreenSuppliersDbContext>();
    await SeedData.SeedAsync(context);
}

// Middleware (order matters: logging first, then error handling)
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseMiddleware<ErrorHandlingMiddleware>();

// Security headers before routing
app.UseMiddleware<SecurityHeadersMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors();

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health");
app.MapControllers();

app.Run();

// Make Program accessible for WebApplicationFactory in integration tests
public partial class Program { }
