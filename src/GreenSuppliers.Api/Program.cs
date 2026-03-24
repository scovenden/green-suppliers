using System.Text;
using GreenSuppliers.Api.Auth;
using GreenSuppliers.Api.Data;
using GreenSuppliers.Api.Middleware;
using GreenSuppliers.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database
builder.Services.AddDbContext<GreenSuppliersDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Application services
builder.Services.AddScoped<ISupplierSearchService, SqlFullTextSearchService>();
builder.Services.AddScoped<AuditService>();
builder.Services.AddScoped<SupplierService>();
builder.Services.AddScoped<EsgScoringService>();
builder.Services.AddScoped<VerificationService>();
builder.Services.AddScoped<LeadService>();
builder.Services.AddScoped<TaxonomyService>();
builder.Services.AddScoped<ContentService>();
builder.Services.AddScoped<DocumentService>();

// JWT Authentication
builder.Services.AddScoped<JwtTokenService>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
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
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Admin", policy => policy.RequireClaim("role", "Admin"));
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

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

// Make Program accessible for WebApplicationFactory in integration tests
public partial class Program { }
