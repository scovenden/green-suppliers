# Green Suppliers Phase 1 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build the public-facing Green Suppliers directory — a searchable, SEO-optimised catalogue of admin-seeded green supplier profiles for South Africa, with ESG scoring, lead capture, and admin management.

**Architecture:** Tier 2 service-layer monolith. Single .NET API project + .NET Worker Service + Next.js 14 App Router frontend. Azure SQL with full-text search. No repository pattern — services call EF Core DbContext directly.

**Tech Stack:** ASP.NET Core 8+ / EF Core / Azure SQL / Next.js 14 / TypeScript / Tailwind / shadcn/ui / xUnit / Vitest / Playwright

**Source Documents:**
- Spec: `docs/superpowers/specs/2026-03-24-green-suppliers-phase1-design.md`
- CLAUDE.md: `CLAUDE.md`
- SQL Schema: `docs/decisions/design-milestone-01.md`
- ADR-0001: `docs/decisions/0001-architecture-tier-and-pattern.md`

---

## File Structure

### Backend — `src/GreenSuppliers.Api/`

| File | Responsibility |
|------|---------------|
| `Program.cs` | DI registration, middleware pipeline, CORS, JWT config |
| `appsettings.json` | Connection strings, JWT settings, CORS origins |
| **Models/Enums/** | |
| `EsgLevel.cs` | Enum: None, Bronze, Silver, Gold, Platinum |
| `VerificationStatus.cs` | Enum: Unverified, Pending, Verified, Flagged |
| `LeadStatus.cs` | Enum: New, Contacted, Closed |
| `CertificationStatus.cs` | Enum: Pending, Accepted, Rejected, Expired |
| `OrganizationType.cs` | Enum: Supplier, Buyer, Admin |
| `UserRole.cs` | Enum: SupplierAdmin, SupplierUser, Buyer, Admin |
| **Models/Entities/** | |
| `Organization.cs` | Company entity |
| `User.cs` | User entity with role and org FK |
| `SupplierProfile.cs` | Core listing entity with sustainability attributes |
| `Industry.cs` | Taxonomy with hierarchical parent |
| `SupplierIndustry.cs` | M2M join entity |
| `ServiceTag.cs` | Product/service tag |
| `SupplierServiceTag.cs` | M2M join entity |
| `CertificationType.cs` | Cert taxonomy |
| `SupplierCertification.cs` | Specific cert with expiry |
| `Document.cs` | File metadata |
| `Lead.cs` | Buyer inquiry |
| `Country.cs` | Reference data |
| `ContentPage.cs` | CMS-lite for SEO pages |
| `AuditEvent.cs` | Append-only log |
| `EmailQueueItem.cs` | Email queue for async dispatch |
| `Plan.cs` | Phase 2 stub |
| `Subscription.cs` | Phase 2 stub |
| `Payment.cs` | Phase 2 stub |
| `SponsoredPlacement.cs` | Phase 2 stub |
| **Models/DTOs/** | |
| `ApiResponse.cs` | Standard envelope: `ApiResponse<T>` |
| `PagedResult.cs` | Pagination wrapper |
| `SupplierSearchQuery.cs` | Search parameters DTO |
| `SupplierSearchResult.cs` | Search result DTO |
| `SupplierProfileDto.cs` | Full profile response |
| `SupplierSummaryDto.cs` | Card/list item response |
| `CreateSupplierRequest.cs` | Admin create supplier |
| `UpdateSupplierRequest.cs` | Admin update supplier |
| `LeadRequest.cs` | Lead submission |
| `GetListedRequest.cs` | Get Listed form |
| `LoginRequest.cs` / `LoginResponse.cs` | Auth DTOs |
| `IndustryDto.cs` / `CountryDto.cs` / `CertTypeDto.cs` / `ServiceTagDto.cs` | Taxonomy DTOs |
| `ContentPageDto.cs` | Content page response |
| `LeadDto.cs` | Lead response |
| `CertificationDto.cs` | Certification response |
| **Data/** | |
| `GreenSuppliersDbContext.cs` | EF Core DbContext with all DbSets |
| `Configurations/OrganizationConfig.cs` | EF entity type config |
| `Configurations/UserConfig.cs` | |
| `Configurations/SupplierProfileConfig.cs` | Includes indexes, check constraints |
| `Configurations/IndustryConfig.cs` | |
| `Configurations/ServiceTagConfig.cs` | |
| `Configurations/CertificationTypeConfig.cs` | |
| `Configurations/SupplierCertificationConfig.cs` | |
| `Configurations/DocumentConfig.cs` | |
| `Configurations/LeadConfig.cs` | |
| `Configurations/CountryConfig.cs` | |
| `Configurations/ContentPageConfig.cs` | |
| `Configurations/AuditEventConfig.cs` | |
| `Configurations/PlanConfig.cs` | Phase 2 stub |
| `Configurations/SubscriptionConfig.cs` | Phase 2 stub |
| `Configurations/PaymentConfig.cs` | Phase 2 stub |
| `Configurations/SponsoredPlacementConfig.cs` | Phase 2 stub |
| `SeedData.cs` | Admin user, countries, industries, cert types |
| **Services/** | |
| `EsgScoringService.cs` | ESG level calculation (Bronze-Platinum) |
| `VerificationService.cs` | Verification state machine |
| `ISupplierSearchService.cs` | Interface for search abstraction |
| `SqlFullTextSearchService.cs` | Azure SQL FTS implementation |
| `LeadService.cs` | Lead creation + notification |
| `SupplierService.cs` | Supplier CRUD + rescore orchestration |
| `AuditService.cs` | Append audit events |
| `ContentService.cs` | Content page CRUD |
| `TaxonomyService.cs` | Industries, cert types, service tags CRUD |
| `DocumentService.cs` | Upload to Blob Storage + DB record |
| **Auth/** | |
| `JwtTokenService.cs` | Generate/validate JWT tokens |
| `AdminAuthorizationHandler.cs` | Admin role policy |
| **Middleware/** | |
| `ErrorHandlingMiddleware.cs` | Global exception → ApiResponse |
| `RequestLoggingMiddleware.cs` | Request/response logging |
| **Validators/** | |
| `CreateSupplierValidator.cs` | Validation for supplier creation |
| `LeadRequestValidator.cs` | Lead form validation |
| `GetListedRequestValidator.cs` | Get Listed form validation |
| **Controllers/** | |
| `SuppliersController.cs` | Public: search, get by slug, get certs |
| `IndustriesController.cs` | Public: list, get by slug |
| `CountriesController.cs` | Public: list, get by code |
| `ServiceTagsController.cs` | Public: list |
| `ContentController.cs` | Public: get by slug |
| `LeadsController.cs` | Public: submit lead |
| `GetListedController.cs` | Public: submit get-listed form |
| `AuthController.cs` | Login + refresh |
| `AdminSuppliersController.cs` | Admin: CRUD, status, publish, rescore |
| `AdminCertificationsController.cs` | Admin: list, accept/reject |
| `AdminLeadsController.cs` | Admin: list, update status |
| `AdminTaxonomyController.cs` | Admin: industries, cert types, tags |
| `AdminContentController.cs` | Admin: content pages CRUD |
| `AdminDocumentsController.cs` | Admin: upload documents |

### Worker — `src/GreenSuppliers.Worker/`

| File | Responsibility |
|------|---------------|
| `Program.cs` | Worker host setup, DI |
| `Jobs/CertExpiryScanner.cs` | Daily: check 30/14/7d expiry, mark expired, rescore |
| `Jobs/NightlyRescore.cs` | Daily: re-run ESG scoring on all suppliers |
| `Jobs/EmailDispatch.cs` | Continuous: process email queue |

### Frontend — `web/green-suppliers-web/`

| File | Responsibility |
|------|---------------|
| `app/(public)/page.tsx` | Homepage |
| `app/(public)/suppliers/page.tsx` | Search results |
| `app/(public)/suppliers/[slug]/page.tsx` | Supplier profile |
| `app/(public)/industries/[slug]/page.tsx` | Industry landing |
| `app/(public)/countries/[slug]/page.tsx` | Country landing |
| `app/(public)/guides/[slug]/page.tsx` | Content/guide page |
| `app/(public)/get-listed/page.tsx` | Get Listed intake form |
| `app/(public)/layout.tsx` | Public layout (header + footer) |
| `app/admin/layout.tsx` | Admin layout (sidebar + auth) |
| `app/admin/page.tsx` | Admin dashboard |
| `app/admin/suppliers/page.tsx` | Supplier management table |
| `app/admin/suppliers/new/page.tsx` | Create supplier form |
| `app/admin/suppliers/[id]/page.tsx` | Edit supplier |
| `app/admin/taxonomy/page.tsx` | Industries + cert types + tags |
| `app/admin/content/page.tsx` | Content pages list |
| `app/admin/content/[id]/page.tsx` | Content editor |
| `app/admin/leads/page.tsx` | Lead management |
| `app/admin/certifications/page.tsx` | Cert review queue |
| `app/admin/login/page.tsx` | Admin login |
| `app/sitemap.ts` | Dynamic sitemap generation |
| `app/robots.ts` | Robots.txt config |
| `components/layout/header.tsx` | Public header with nav + logo |
| `components/layout/footer.tsx` | Public footer |
| `components/layout/admin-sidebar.tsx` | Admin navigation |
| `components/suppliers/supplier-card.tsx` | Supplier list card with ESG badge |
| `components/suppliers/supplier-profile-header.tsx` | Profile hero section |
| `components/suppliers/esg-badge.tsx` | ESG level badge component |
| `components/suppliers/verification-badge.tsx` | Verified checkmark |
| `components/suppliers/certification-list.tsx` | Cert display with status |
| `components/search/search-bar.tsx` | Hero search input |
| `components/search/filter-sidebar.tsx` | Search filters panel |
| `components/search/search-results.tsx` | Results grid/list |
| `components/leads/lead-form.tsx` | Contact supplier form |
| `components/leads/get-listed-form.tsx` | Get Listed intake form |
| `components/seo/json-ld.tsx` | JSON-LD structured data |
| `components/seo/breadcrumbs.tsx` | Breadcrumb navigation |
| `lib/api-client.ts` | Typed fetch wrapper for API |
| `lib/types.ts` | TypeScript types matching API DTOs |
| `lib/validators.ts` | Zod schemas for forms |
| `lib/utils.ts` | Helpers (slug, formatting) |
| `tailwind.config.ts` | Brand green theme extension |
| `app/globals.css` | shadcn/ui CSS variable overrides |

### Tests

| File | What it tests |
|------|--------------|
| `tests/GreenSuppliers.Tests/Services/EsgScoringServiceTests.cs` | All 5 ESG levels + edge cases |
| `tests/GreenSuppliers.Tests/Services/VerificationServiceTests.cs` | All state transitions |
| `tests/GreenSuppliers.Tests/Services/SqlFullTextSearchServiceTests.cs` | Search + filter combinations |
| `tests/GreenSuppliers.Tests/Services/LeadServiceTests.cs` | Lead creation + validation |
| `tests/GreenSuppliers.Tests/Integration/SuppliersEndpointTests.cs` | Public supplier API |
| `tests/GreenSuppliers.Tests/Integration/AdminEndpointTests.cs` | Admin CRUD API |
| `tests/GreenSuppliers.Tests/Integration/AuthEndpointTests.cs` | Login + JWT |
| `web/green-suppliers-web/__tests__/components/supplier-card.test.tsx` | Card rendering |
| `web/green-suppliers-web/__tests__/components/esg-badge.test.tsx` | Badge variants |
| `web/green-suppliers-web/__tests__/components/lead-form.test.tsx` | Form validation |
| `web/green-suppliers-web/e2e/search.spec.ts` | E2E: search → profile → lead |

---

## Task 1: Solution Scaffold + Project Setup

**Files:**
- Create: `GreenSuppliers.sln`
- Create: `src/GreenSuppliers.Api/GreenSuppliers.Api.csproj`
- Create: `src/GreenSuppliers.Worker/GreenSuppliers.Worker.csproj`
- Create: `tests/GreenSuppliers.Tests/GreenSuppliers.Tests.csproj`

- [ ] **Step 1: Initialise git repo and .gitignore**

```bash
cd "C:/Users/SiviCovenden/OneDrive - Agilus (Pty) Ltd/Documents/Claude/Green Suppliers"
git init
curl -sL https://raw.githubusercontent.com/github/gitignore/main/VisualStudio.gitignore > .gitignore
echo "node_modules/" >> .gitignore
echo ".next/" >> .gitignore
echo ".env*.local" >> .gitignore
echo ".superpowers/" >> .gitignore
git add .gitignore && git commit -m "chore: add .gitignore for .NET + Node.js"
```

- [ ] **Step 2: Create .NET solution and API project**

```bash
dotnet new sln -n GreenSuppliers
mkdir -p src
dotnet new webapi -n GreenSuppliers.Api -o src/GreenSuppliers.Api --no-https false
dotnet sln add src/GreenSuppliers.Api/GreenSuppliers.Api.csproj
```

- [ ] **Step 3: Create Worker project**

```bash
dotnet new worker -n GreenSuppliers.Worker -o src/GreenSuppliers.Worker
dotnet sln add src/GreenSuppliers.Worker/GreenSuppliers.Worker.csproj
dotnet add src/GreenSuppliers.Worker/GreenSuppliers.Worker.csproj reference src/GreenSuppliers.Api/GreenSuppliers.Api.csproj
```

> **Note (pragmatic trade-off):** The Worker references the API project directly. This means it inherits ASP.NET MVC dependencies it doesn't need. For Tier 2 with 1-2 devs, this is acceptable — extracting a shared project would add ceremony without benefit. Revisit if the Worker project's build time or dependency graph becomes a problem.

- [ ] **Step 3: Create test project**

```bash
mkdir -p tests
dotnet new xunit -n GreenSuppliers.Tests -o tests/GreenSuppliers.Tests
dotnet sln add tests/GreenSuppliers.Tests/GreenSuppliers.Tests.csproj
dotnet add tests/GreenSuppliers.Tests/GreenSuppliers.Tests.csproj reference src/GreenSuppliers.Api/GreenSuppliers.Api.csproj
```

- [ ] **Step 4: Add NuGet packages to API project**

```bash
cd src/GreenSuppliers.Api
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.EntityFrameworkCore.Tools
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add package Azure.Storage.Blobs
dotnet add package BCrypt.Net-Next
dotnet add package FluentValidation.AspNetCore
```

- [ ] **Step 5: Add NuGet packages to test project**

```bash
cd ../../tests/GreenSuppliers.Tests
dotnet add package FluentAssertions
dotnet add package Moq
dotnet add package Microsoft.EntityFrameworkCore.InMemory
dotnet add package Microsoft.AspNetCore.Mvc.Testing
```

- [ ] **Step 6: Create folder structure in API project**

```bash
cd ../../src/GreenSuppliers.Api
mkdir -p Controllers Services Models/Entities Models/DTOs Models/Enums Data/Configurations Middleware Auth Validators
```

- [ ] **Step 7: Build solution to verify setup**

Run: `dotnet build ../../GreenSuppliers.sln`
Expected: Build succeeded with 0 errors

- [ ] **Step 8: Commit**

```bash
git add -A && git commit -m "feat: scaffold .NET solution with API, Worker, and Test projects"
```

---

## Task 2: Enums + Entity Models

**Files:**
- Create: `src/GreenSuppliers.Api/Models/Enums/EsgLevel.cs`
- Create: `src/GreenSuppliers.Api/Models/Enums/VerificationStatus.cs`
- Create: `src/GreenSuppliers.Api/Models/Enums/LeadStatus.cs`
- Create: `src/GreenSuppliers.Api/Models/Enums/CertificationStatus.cs`
- Create: `src/GreenSuppliers.Api/Models/Enums/OrganizationType.cs`
- Create: `src/GreenSuppliers.Api/Models/Enums/UserRole.cs`
- Create: All entity files in `src/GreenSuppliers.Api/Models/Entities/`

- [ ] **Step 1: Create all enum types**

```csharp
// EsgLevel.cs
namespace GreenSuppliers.Api.Models.Enums;
public enum EsgLevel { None, Bronze, Silver, Gold, Platinum }

// VerificationStatus.cs
public enum VerificationStatus { Unverified, Pending, Verified, Flagged }

// LeadStatus.cs
public enum LeadStatus { New, Contacted, Closed }

// CertificationStatus.cs
public enum CertificationStatus { Pending, Accepted, Rejected, Expired }

// OrganizationType.cs
public enum OrganizationType { Supplier, Buyer, Admin }

// UserRole.cs
public enum UserRole { SupplierAdmin, SupplierUser, Buyer, Admin }
```

- [ ] **Step 2: Create core entity classes**

Create `Organization.cs`, `User.cs`, `SupplierProfile.cs`, `Industry.cs`, `SupplierIndustry.cs`, `ServiceTag.cs`, `SupplierServiceTag.cs`, `CertificationType.cs`, `SupplierCertification.cs`, `Document.cs`, `Lead.cs`, `Country.cs`, `ContentPage.cs`, `AuditEvent.cs`.

Each entity matches the SQL schema in `docs/decisions/design-milestone-01.md` exactly. Use string-backed enums via `[Column(TypeName = "nvarchar(20)")]` to match the DB schema.

Key entity: `SupplierProfile.cs` must include:
- All sustainability attributes (RenewableEnergyPercent, WasteRecyclingPercent, CarbonReporting, WaterManagement, SustainablePackaging)
- Computed fields (VerificationStatus, EsgLevel, EsgScore)
- Navigation properties to Industries, ServiceTags, Certifications, Documents, Leads
- Slug for URL routing

- [ ] **Step 3: Create EmailQueueItem entity**

```csharp
// EmailQueueItem.cs — async email queue for Worker to process
public class EmailQueueItem
{
    public Guid Id { get; set; }
    public string ToEmail { get; set; } = string.Empty;
    public string ToName { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string BodyHtml { get; set; } = string.Empty;
    public string TemplateType { get; set; } = string.Empty; // "lead_notification" | "cert_reminder" | "get_listed"
    public string? TemplateData { get; set; } // JSON
    public string Status { get; set; } = "pending"; // pending | sent | failed
    public int RetryCount { get; set; }
    public DateTime? SentAt { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

Add corresponding `EmailQueueItemConfig.cs` in Data/Configurations/ and `DbSet<EmailQueueItem> EmailQueue` to DbContext.

- [ ] **Step 4: Create Phase 2 stub entities**

Create `Plan.cs`, `Subscription.cs`, `Payment.cs`, `SponsoredPlacement.cs` — minimal entity classes matching the Phase 2 schema. These are schema-only, no services or controllers.

- [ ] **Step 4: Build to verify entities compile**

Run: `dotnet build`
Expected: Build succeeded

- [ ] **Step 5: Commit**

```bash
git add -A && git commit -m "feat: add all entity models and enums"
```

---

## Task 3: DbContext + EF Core Configurations + Initial Migration

**Files:**
- Create: `src/GreenSuppliers.Api/Data/GreenSuppliersDbContext.cs`
- Create: All files in `src/GreenSuppliers.Api/Data/Configurations/`

- [ ] **Step 1: Create DbContext with all DbSets**

```csharp
// GreenSuppliersDbContext.cs
public class GreenSuppliersDbContext : DbContext
{
    public DbSet<Organization> Organizations => Set<Organization>();
    public DbSet<User> Users => Set<User>();
    public DbSet<SupplierProfile> SupplierProfiles => Set<SupplierProfile>();
    public DbSet<Industry> Industries => Set<Industry>();
    public DbSet<ServiceTag> ServiceTags => Set<ServiceTag>();
    public DbSet<CertificationType> CertificationTypes => Set<CertificationType>();
    public DbSet<SupplierCertification> SupplierCertifications => Set<SupplierCertification>();
    public DbSet<Document> Documents => Set<Document>();
    public DbSet<Lead> Leads => Set<Lead>();
    public DbSet<Country> Countries => Set<Country>();
    public DbSet<ContentPage> ContentPages => Set<ContentPage>();
    public DbSet<AuditEvent> AuditEvents => Set<AuditEvent>();
    public DbSet<EmailQueueItem> EmailQueue => Set<EmailQueueItem>();
    // Phase 2 stubs
    public DbSet<Plan> Plans => Set<Plan>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<SponsoredPlacement> SponsoredPlacements => Set<SponsoredPlacement>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(GreenSuppliersDbContext).Assembly);
    }
}
```

- [ ] **Step 2: Create EF entity type configurations**

One configuration file per entity in `Data/Configurations/`. Each must:
- Set table name
- Configure primary key as `NEWSEQUENTIALID()` default: `.HasDefaultValueSql("NEWSEQUENTIALID()")`
- Define string column lengths matching the SQL schema exactly
- Configure indexes (filtered indexes for `IsDeleted = 0`)
- Configure foreign key relationships
- Configure M2M join tables (SupplierIndustries, SupplierServiceTags) with composite PKs

Critical: `SupplierProfileConfig.cs` must include:
- Check constraints on RenewableEnergyPercent and WasteRecyclingPercent (0-100)
- Unique constraint on Slug
- Unique constraint on OrganizationId (1:1)
- Filtered indexes on CountryCode, EsgLevel, VerificationStatus, EsgScore

- [ ] **Step 3: Register DbContext in Program.cs**

```csharp
builder.Services.AddDbContext<GreenSuppliersDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
```

Add connection string, JWT settings, and CORS to `appsettings.Development.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=GreenSuppliers;Trusted_Connection=true;"
  },
  "Jwt": {
    "Secret": "CHANGE-THIS-TO-A-64-CHAR-SECRET-FOR-DEVELOPMENT-ONLY-1234567890",
    "Issuer": "GreenSuppliers",
    "Audience": "GreenSuppliers",
    "AccessTokenExpiryMinutes": 60,
    "RefreshTokenExpiryDays": 7
  },
  "Cors": {
    "AllowedOrigins": ["http://localhost:3000"]
  }
}
```

- [ ] **Step 4: Create initial migration**

```bash
cd src/GreenSuppliers.Api
dotnet ef migrations add InitialCreate
```

- [ ] **Step 5: Apply migration to verify schema**

```bash
dotnet ef database update
```

Expected: Database created with all 18 tables

- [ ] **Step 6: Commit**

```bash
git add -A && git commit -m "feat: add DbContext, entity configurations, and initial migration"
```

---

## Task 4: Seed Data

**Files:**
- Create: `src/GreenSuppliers.Api/Data/SeedData.cs`

- [ ] **Step 1: Create seed data class**

Seeds:
- 1 admin organization + 1 admin user (password: hashed with BCrypt)
- 10 countries (ZA, KE, NG, GH, EG, MA, RW, TZ, UG, BW) — only ZA active
- 8 industries (Renewable Energy, Construction, Agriculture, Waste Management, Water Solutions, Logistics, Manufacturing, Mining Services)
- 6 certification types (ISO 14001, ISO 9001, B-Corp, FSC, Green Building Council SA, Carbon Neutral Certification)

```csharp
public static class SeedData
{
    public static async Task SeedAsync(GreenSuppliersDbContext context)
    {
        if (await context.Users.AnyAsync()) return; // Already seeded

        // Countries
        var countries = new List<Country> { /* ZA active, rest inactive */ };
        context.Countries.AddRange(countries);

        // Industries
        var industries = new List<Industry> { /* 8 industries with slugs */ };
        context.Industries.AddRange(industries);

        // Certification types
        var certTypes = new List<CertificationType> { /* 6 types with slugs */ };
        context.CertificationTypes.AddRange(certTypes);

        // Admin org + user
        var adminOrg = new Organization { Name = "Green Suppliers Admin", OrganizationType = OrganizationType.Admin, CountryCode = "ZA" };
        context.Organizations.Add(adminOrg);

        var adminUser = new User
        {
            OrganizationId = adminOrg.Id,
            Email = "admin@greensuppliers.co.za",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("ChangeMe123!"),
            FirstName = "Admin",
            LastName = "User",
            Role = UserRole.Admin,
            IsActive = true,
            EmailVerified = true
        };
        context.Users.Add(adminUser);

        await context.SaveChangesAsync();
    }
}
```

- [ ] **Step 2: Call seed on startup**

In `Program.cs`, after `app.Build()`:
```csharp
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<GreenSuppliersDbContext>();
    await context.Database.MigrateAsync();
    await SeedData.SeedAsync(context);
}
```

- [ ] **Step 3: Run and verify seed data**

Run: `dotnet run`
Expected: Application starts, seed data inserted into DB

- [ ] **Step 4: Commit**

```bash
git add -A && git commit -m "feat: add seed data for countries, industries, cert types, and admin user"
```

---

## Task 5: API Response Envelope + Error Handling Middleware

**Files:**
- Create: `src/GreenSuppliers.Api/Models/DTOs/ApiResponse.cs`
- Create: `src/GreenSuppliers.Api/Models/DTOs/PagedResult.cs`
- Create: `src/GreenSuppliers.Api/Middleware/ErrorHandlingMiddleware.cs`

- [ ] **Step 1: Create ApiResponse<T> envelope**

```csharp
public class ApiResponse<T>
{
    public bool Success { get; init; }
    public T? Data { get; init; }
    public ApiError? Error { get; init; }
    public PaginationMeta? Meta { get; init; }

    public static ApiResponse<T> Ok(T data) => new() { Success = true, Data = data };
    public static ApiResponse<T> Ok(T data, PaginationMeta meta) => new() { Success = true, Data = data, Meta = meta };
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
```

- [ ] **Step 2: Create PagedResult<T>**

```csharp
public class PagedResult<T>
{
    public List<T> Items { get; init; } = new();
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int Total { get; init; }
    public int TotalPages => (int)Math.Ceiling(Total / (double)PageSize);
}
```

- [ ] **Step 3: Create ErrorHandlingMiddleware**

Catches all unhandled exceptions, logs them, returns 500 with ApiResponse envelope (no stack trace in production).

- [ ] **Step 4: Create RequestLoggingMiddleware**

Logs request method, path, status code, and duration for every request. Uses `ILogger`. Skips logging for health check endpoints.

- [ ] **Step 5: Register middleware in Program.cs**

```csharp
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseMiddleware<ErrorHandlingMiddleware>();
```

- [ ] **Step 6: Commit**

```bash
git add -A && git commit -m "feat: add API response envelope, error handling, and request logging middleware"
```

---

## Task 6: JWT Authentication

**Files:**
- Create: `src/GreenSuppliers.Api/Auth/JwtTokenService.cs`
- Create: `src/GreenSuppliers.Api/Auth/AdminAuthorizationHandler.cs`
- Create: `src/GreenSuppliers.Api/Models/DTOs/LoginRequest.cs`
- Create: `src/GreenSuppliers.Api/Models/DTOs/LoginResponse.cs`
- Create: `src/GreenSuppliers.Api/Controllers/AuthController.cs`
- Test: `tests/GreenSuppliers.Tests/Integration/AuthEndpointTests.cs`

- [ ] **Step 1: Write failing test for login**

```csharp
[Fact]
public async Task Login_WithValidCredentials_ReturnsJwtToken()
{
    // POST /api/v1/auth/login with admin@greensuppliers.co.za / ChangeMe123!
    // Assert: 200, token is not null, token contains admin role claim
}

[Fact]
public async Task Login_WithInvalidPassword_Returns401()
{
    // POST /api/v1/auth/login with wrong password
    // Assert: 401
}
```

- [ ] **Step 2: Run tests to verify they fail**

Run: `dotnet test`
Expected: Both tests FAIL

- [ ] **Step 3: Implement JwtTokenService**

Generate access token (1 hour) + refresh token (7 days). Include claims: sub (userId), email, role, organizationId.

- [ ] **Step 4: Implement AuthController with login + refresh endpoints**

```
POST /api/v1/auth/login     → validate credentials → return tokens
POST /api/v1/auth/refresh   → validate refresh token → return new tokens
```

- [ ] **Step 5: Configure JWT auth in Program.cs**

Add JWT bearer authentication, add authorization policy for "Admin" role.

- [ ] **Step 6: Run tests to verify they pass**

Run: `dotnet test`
Expected: All tests PASS

- [ ] **Step 7: Commit**

```bash
git add -A && git commit -m "feat: add JWT authentication with login and refresh endpoints"
```

---

## Task 7: ESG Scoring Service (TDD)

**Files:**
- Create: `src/GreenSuppliers.Api/Services/EsgScoringService.cs`
- Test: `tests/GreenSuppliers.Tests/Services/EsgScoringServiceTests.cs`

- [ ] **Step 1: Write failing tests for all 5 ESG levels**

```csharp
[Fact] public void CalculateScore_IncompleteProfile_ReturnsNone()
[Fact] public void CalculateScore_CompleteProfileNoCerts_ReturnsBronze()
[Fact] public void CalculateScore_OneCert_Renewable20_ReturnsSilver()
[Fact] public void CalculateScore_TwoCerts_Renewable50_CarbonReporting_ReturnsGold()
[Fact] public void CalculateScore_ThreeCerts_Renewable70_Waste70_CarbonReporting_ReturnsPlatinum()
[Fact] public void CalculateScore_ExpiredCertsNotCounted()
[Fact] public void CalculateScore_ReturnsReasonsList()
[Fact] public void CalculateScore_BoundaryValues_Renewable19_NotSilver()
[Fact] public void CalculateScore_BoundaryValues_Renewable20_IsSilver()
```

- [ ] **Step 2: Run tests to verify they fail**

Run: `dotnet test --filter "EsgScoringServiceTests"`
Expected: All FAIL

- [ ] **Step 3: Implement EsgScoringService**

Pure function — takes `SupplierProfile` + `List<SupplierCertification>` (accepted, not expired). Returns `EsgScoreResult(Level, NumericScore, Reasons)`. Follows rules from spec exactly.

- [ ] **Step 4: Run tests to verify they pass**

Run: `dotnet test --filter "EsgScoringServiceTests"`
Expected: All PASS

- [ ] **Step 5: Commit**

```bash
git add -A && git commit -m "feat: add ESG scoring service with TDD tests for all 5 levels"
```

---

## Task 8: Verification Service (TDD)

**Files:**
- Create: `src/GreenSuppliers.Api/Services/VerificationService.cs`
- Test: `tests/GreenSuppliers.Tests/Services/VerificationServiceTests.cs`

- [ ] **Step 1: Write failing tests for all state transitions**

```csharp
[Fact] public void Evaluate_NoCerts_ReturnsUnverified()
[Fact] public void Evaluate_PendingCert_ReturnsPending()
[Fact] public void Evaluate_AcceptedCertAndCompleteProfile_ReturnsVerified()
[Fact] public void Evaluate_AllCertsExpired_ReturnsUnverified()
[Fact] public void Evaluate_FlaggedProfile_StaysFlagged()
[Fact] public void Evaluate_IncompleteProfileWithAcceptedCert_ReturnsPending()
```

- [ ] **Step 2: Run tests to verify they fail**

- [ ] **Step 3: Implement VerificationService**

State machine: evaluates profile completeness + certification statuses. Does NOT change flagged status (only admin can unflag).

- [ ] **Step 4: Run tests to verify they pass**

- [ ] **Step 5: Commit**

```bash
git add -A && git commit -m "feat: add verification service with state machine tests"
```

---

## Task 9: Audit Service + Supplier Service (TDD)

**Files:**
- Create: `src/GreenSuppliers.Api/Services/AuditService.cs`
- Create: `src/GreenSuppliers.Api/Services/SupplierService.cs`
- Create: All supplier DTOs in `Models/DTOs/`
- Test: `tests/GreenSuppliers.Tests/Services/SupplierServiceTests.cs`

- [ ] **Step 1: Write failing tests for SupplierService**

```csharp
[Fact] public async Task CreateAsync_ValidRequest_CreatesOrgAndProfile()
[Fact] public async Task CreateAsync_GeneratesSlugFromTradingName()
[Fact] public async Task CreateAsync_RunsEsgScoringOnCreate()
[Fact] public async Task CreateAsync_WritesAuditLog()
[Fact] public async Task UpdateAsync_UpdatesProfileAndRescores()
[Fact] public async Task GetBySlugAsync_ReturnsProfile()
[Fact] public async Task GetBySlugAsync_NotFound_ReturnsNull()
[Fact] public async Task SetVerificationStatusAsync_Flagged_HidesFromSearch()
[Fact] public async Task RescoreAsync_UpdatesEsgLevelAndScore()
```

- [ ] **Step 2: Run tests to verify they fail**

Run: `dotnet test --filter "SupplierServiceTests"`
Expected: All FAIL

- [ ] **Step 3: Create AuditService**

Simple append-only: `LogAsync(userId, action, entityType, entityId, oldValues, newValues)`. Writes to AuditEvents table.

- [ ] **Step 2: Create supplier DTOs**

`CreateSupplierRequest`, `UpdateSupplierRequest`, `SupplierProfileDto`, `SupplierSummaryDto`. Map between entities and DTOs manually (no AutoMapper — keep it simple for Tier 2).

- [ ] **Step 3: Create SupplierService**

Methods:
- `CreateAsync(CreateSupplierRequest)` — creates Org + SupplierProfile, runs ESG scoring, audits
- `UpdateAsync(Guid id, UpdateSupplierRequest)` — updates profile, re-runs ESG scoring, audits
- `GetBySlugAsync(string slug)` — returns SupplierProfileDto
- `SetVerificationStatusAsync(Guid id, VerificationStatus, string? reason)` — admin action, audits
- `SetPublishedAsync(Guid id, bool published)` — audits
- `RescoreAsync(Guid id)` — re-runs ESG scoring + verification

- [ ] **Step 6: Run tests to verify they pass**

Run: `dotnet test --filter "SupplierServiceTests"`
Expected: All PASS

- [ ] **Step 7: Commit**

```bash
git add -A && git commit -m "feat: add audit service and supplier service with TDD tests"
```

---

## Task 10: Search Service (TDD)

**Files:**
- Create: `src/GreenSuppliers.Api/Services/ISupplierSearchService.cs`
- Create: `src/GreenSuppliers.Api/Services/SqlFullTextSearchService.cs`
- Test: `tests/GreenSuppliers.Tests/Services/SqlFullTextSearchServiceTests.cs`

- [ ] **Step 1: Write failing tests**

```csharp
[Fact] public async Task Search_NoFilters_ReturnsAllPublished()
[Fact] public async Task Search_ByCountry_FiltersCorrectly()
[Fact] public async Task Search_ByIndustry_FiltersCorrectly()
[Fact] public async Task Search_ByEsgLevel_FiltersCorrectly()
[Fact] public async Task Search_ByVerificationStatus_FiltersCorrectly()
[Fact] public async Task Search_Pagination_ReturnsCorrectPage()
[Fact] public async Task Search_ExcludesUnpublished()
[Fact] public async Task Search_ExcludesSoftDeleted()
[Fact] public async Task Search_SortByEsgScore_DescendingDefault()
```

- [ ] **Step 2: Run tests to verify they fail**

- [ ] **Step 3: Implement ISupplierSearchService interface**

```csharp
public interface ISupplierSearchService
{
    Task<PagedResult<SupplierSearchResult>> SearchAsync(SupplierSearchQuery query, CancellationToken ct);
}
```

- [ ] **Step 4: Implement SqlFullTextSearchService**

Builds dynamic LINQ query from `SupplierSearchQuery`. For `q` parameter, uses `EF.Functions.FreeText()` or `EF.Functions.Contains()` on TradingName + Description. Applies all filters, pagination, sorting.

Register as: `builder.Services.AddScoped<ISupplierSearchService, SqlFullTextSearchService>();`

- [ ] **Step 5: Run tests to verify they pass**

- [ ] **Step 6: Commit**

```bash
git add -A && git commit -m "feat: add supplier search service with full-text search and filtering"
```

---

## Task 11: Lead, Taxonomy, Content, and Document Services

**Files:**
- Create: `src/GreenSuppliers.Api/Services/LeadService.cs`
- Create: `src/GreenSuppliers.Api/Services/TaxonomyService.cs`
- Create: `src/GreenSuppliers.Api/Services/ContentService.cs`
- Create: `src/GreenSuppliers.Api/Services/DocumentService.cs`
- Create: Remaining DTOs
- Test: `tests/GreenSuppliers.Tests/Services/LeadServiceTests.cs`

- [ ] **Step 1: Write failing tests for LeadService**

```csharp
[Fact] public async Task CreateLead_ValidRequest_ReturnsLead()
[Fact] public async Task CreateLead_MissingEmail_ThrowsValidation()
[Fact] public async Task CreateLead_AuditsSubmission()
```

- [ ] **Step 2: Implement LeadService**

`CreateAsync(LeadRequest)` — saves lead, audits, returns LeadDto. IP address captured from HttpContext.

- [ ] **Step 3: Implement TaxonomyService**

CRUD for Industries, CertificationTypes, ServiceTags. Simple pass-through to DbContext.

- [ ] **Step 4: Implement ContentService**

CRUD for ContentPages. `GetBySlugAsync` only returns published pages for public endpoint.

- [ ] **Step 5: Implement DocumentService**

`UploadAsync(IFormFile, supplierProfileId, documentType)` — validates content type + size, uploads to Azure Blob Storage, saves metadata to Documents table.

- [ ] **Step 6: Run all tests**

Run: `dotnet test`
Expected: All PASS

- [ ] **Step 7: Commit**

```bash
git add -A && git commit -m "feat: add lead, taxonomy, content, and document services"
```

---

## Task 12: Validators

**Files:**
- Create: `src/GreenSuppliers.Api/Validators/CreateSupplierValidator.cs`
- Create: `src/GreenSuppliers.Api/Validators/LeadRequestValidator.cs`
- Create: `src/GreenSuppliers.Api/Validators/GetListedRequestValidator.cs`

- [ ] **Step 1: Create FluentValidation validators**

`CreateSupplierValidator`: country code required, legal name required, valid email format, slug auto-generated if empty.

`LeadRequestValidator`: contactName, contactEmail, message required. Email format validation. Message max 2000 chars.

`GetListedRequestValidator`: companyName, contactName, contactEmail, country required. Description max 500 chars.

- [ ] **Step 2: Register FluentValidation in Program.cs**

```csharp
builder.Services.AddValidatorsFromAssemblyContaining<CreateSupplierValidator>();
```

- [ ] **Step 3: Commit**

```bash
git add -A && git commit -m "feat: add FluentValidation validators for supplier, lead, and get-listed forms"
```

---

## Task 13: All API Controllers

**Files:**
- Create: All controller files listed in the file structure

- [ ] **Step 1: Create public controllers**

`SuppliersController`: GET /suppliers (search), GET /suppliers/{slug}, GET /suppliers/{slug}/certifications
`IndustriesController`: GET /industries, GET /industries/{slug}
`CountriesController`: GET /countries, GET /countries/{code}
`ServiceTagsController`: GET /service-tags
`ContentController`: GET /content/{slug}
`LeadsController`: POST /leads
`GetListedController`: POST /get-listed

All return `ApiResponse<T>`. Thin controllers — call service, wrap result.

- [ ] **Step 2: Create admin controllers**

All require `[Authorize(Policy = "Admin")]`.

`AdminSuppliersController`: Full CRUD + status/publish/rescore
`AdminCertificationsController`: List + accept/reject
`AdminLeadsController`: List + update status
`AdminTaxonomyController`: CRUD for industries, cert types, tags
`AdminContentController`: CRUD for content pages
`AdminDocumentsController`: Upload

- [ ] **Step 3: Configure routing and CORS in Program.cs**

API versioning via route prefix `/api/v1/`. CORS allows frontend origin.

- [ ] **Step 4: Write integration tests for key endpoints**

```csharp
// SuppliersEndpointTests.cs
[Fact] public async Task GetSuppliers_ReturnsPagedResults()
[Fact] public async Task GetSupplierBySlug_ReturnsProfile()
[Fact] public async Task GetSupplierBySlug_NotFound_Returns404()

// AdminEndpointTests.cs
[Fact] public async Task CreateSupplier_AsAdmin_Returns201()
[Fact] public async Task CreateSupplier_Unauthorized_Returns401()
```

- [ ] **Step 5: Run all tests**

Run: `dotnet test`
Expected: All PASS

- [ ] **Step 6: Commit**

```bash
git add -A && git commit -m "feat: add all public and admin API controllers with integration tests"
```

---

## Task 14: Worker Service — Background Jobs

**Files:**
- Create: `src/GreenSuppliers.Worker/Jobs/CertExpiryScanner.cs`
- Create: `src/GreenSuppliers.Worker/Jobs/NightlyRescore.cs`
- Create: `src/GreenSuppliers.Worker/Jobs/EmailDispatch.cs`
- Modify: `src/GreenSuppliers.Worker/Program.cs`

- [ ] **Step 1: Implement CertExpiryScanner**

`BackgroundService` that runs daily at 2am UTC. Queries certifications expiring in 30/14/7 days. Updates expired certs to `Expired` status. Triggers rescore on affected supplier profiles. Queues reminder emails.

- [ ] **Step 2: Implement NightlyRescore**

`BackgroundService` that runs daily at 3am UTC. Iterates all published suppliers. Calls `EsgScoringService.CalculateScore()` and `VerificationService.Evaluate()`. Updates DB.

- [ ] **Step 3: Implement EmailDispatch**

`BackgroundService` (continuous, polls every 30 seconds). Reads pending items from the `EmailQueue` table (`Status = 'pending'`). Sends via `IEmailSender` interface (concrete implementation TBD — SendGrid or Resend). On success: update status to `sent`, set `SentAt`. On failure: increment `RetryCount`, set `ErrorMessage`, retry up to 3 times then mark `failed`.

Create `IEmailSender` interface + `ConsoleEmailSender` stub (logs to console for development). Register in DI so the real provider can be swapped in later.

- [ ] **Step 4: Configure Worker Program.cs**

Register DbContext, services (shared with API), and hosted services.

- [ ] **Step 5: Build and verify**

Run: `dotnet build`
Expected: Build succeeded

- [ ] **Step 6: Commit**

```bash
git add -A && git commit -m "feat: add background jobs for cert expiry, nightly rescore, and email dispatch"
```

---

## Task 15: Next.js Scaffold + Brand Theme

**Files:**
- Create: `web/green-suppliers-web/` (entire Next.js app)

- [ ] **Step 1: Create Next.js app**

```bash
cd "C:/Users/SiviCovenden/OneDrive - Agilus (Pty) Ltd/Documents/Claude/Green Suppliers"
mkdir -p web
cd web
npx create-next-app@latest green-suppliers-web --typescript --tailwind --eslint --app --src-dir=false --import-alias="@/*"
```

- [ ] **Step 2: Install dependencies**

```bash
cd green-suppliers-web
npm install @tanstack/react-query @tanstack/react-table react-hook-form @hookform/resolvers zod
npx shadcn@latest init
```

Configure shadcn with green theme CSS variables from CLAUDE.md.

- [ ] **Step 3: Configure Tailwind brand theme**

In `tailwind.config.ts`:
```typescript
theme: {
  extend: {
    colors: {
      brand: {
        green: { DEFAULT: '#16A34A', dark: '#166534', light: '#F0FDF4', hover: '#15803D' },
        emerald: { DEFAULT: '#059669', hover: '#047857' },
        earth: { DEFAULT: '#78716C', light: '#D6D3D1', dark: '#44403C' },
        dark: { DEFAULT: '#1C1917' },
      }
    }
  }
}
```

- [ ] **Step 4: Set shadcn/ui CSS variables in globals.css**

```css
:root {
  --primary: 142 71% 45%;
  --primary-foreground: 0 0% 100%;
  --secondary: 143 64% 24%;
  --secondary-foreground: 0 0% 100%;
  --ring: 142 71% 45%;
  --accent: 160 84% 39%;
  --accent-foreground: 0 0% 100%;
}
```

- [ ] **Step 5: Install core shadcn components**

```bash
npx shadcn@latest add button card input select badge table dialog sheet dropdown-menu form label textarea separator tabs
```

- [ ] **Step 6: Create .env.local with API URL**

```bash
echo "NEXT_PUBLIC_API_URL=http://localhost:5000/api/v1" > .env.local
```

Add `"type-check": "tsc --noEmit"` to `package.json` scripts.

- [ ] **Step 7: Create lib/api-client.ts and lib/types.ts**

Typed API client wrapping fetch. Reads `NEXT_PUBLIC_API_URL` from env. Types matching all API DTOs.

- [ ] **Step 7: Create lib/validators.ts**

Zod schemas for LeadRequest, GetListedRequest, LoginRequest.

- [ ] **Step 8: Verify dev server starts**

Run: `npm run dev`
Expected: Next.js dev server running on localhost:3000

- [ ] **Step 9: Commit**

```bash
git add -A && git commit -m "feat: scaffold Next.js app with Tailwind brand theme and shadcn/ui"
```

---

## Task 16: Layout Components

**Files:**
- Create: `components/layout/header.tsx`
- Create: `components/layout/footer.tsx`
- Create: `app/(public)/layout.tsx`

- [ ] **Step 1: Create Header component**

Logo (from assets/logo.png), nav links (Suppliers, Industries, Guides), "Get Listed" CTA button, "Sign In" link. Frosted glass dark green background. Responsive hamburger on mobile.

- [ ] **Step 2: Create Footer component**

Logo, copyright, links (Privacy, Terms, Contact), "Powered by Agilus.AI". Dark green background.

- [ ] **Step 3: Create public layout**

Wraps all (public) routes with Header + Footer + main content area.

- [ ] **Step 4: Verify layout renders**

Run: `npm run dev`, visit localhost:3000
Expected: Header and footer visible

- [ ] **Step 5: Commit**

```bash
git add -A && git commit -m "feat: add public layout with header and footer components"
```

---

## Task 17: Homepage

**Files:**
- Create: `app/(public)/page.tsx`
- Create: `components/search/search-bar.tsx`
- Create: `components/suppliers/supplier-card.tsx`
- Create: `components/suppliers/esg-badge.tsx`

- [ ] **Step 1: Create EsgBadge component**

Renders ESG level badge with tier-specific gradient colours (Bronze/amber, Silver/grey, Gold/amber, Platinum/green). Accepts `level` prop.

- [ ] **Step 2: Create SupplierCard component**

Card with: tier-coloured header gradient, logo/initials, company name, city/country, description, industry tags, verification badge, ESG badge, "View Profile" link.

- [ ] **Step 3: Create SearchBar component**

Dual-field search (keyword + industry dropdown). Green gradient search button. Quick filter chips below.

- [ ] **Step 4: Build Homepage**

Sections (matching approved mockup):
1. Hero with gradient background, search bar, quick filters
2. Floating stats card (suppliers count, industries, countries)
3. Featured Suppliers grid (3 cards, fetched from API)
4. Browse by Industry grid (8 cards with icons + supplier counts)
5. How Verification Works (Bronze → Platinum explainer)
6. "Are You a Green Supplier?" CTA section

All data fetched server-side (SSR).

- [ ] **Step 5: Verify homepage renders**

Run: `npm run dev`, visit localhost:3000
Expected: Full homepage with all sections (data may be empty until API connected)

- [ ] **Step 6: Commit**

```bash
git add -A && git commit -m "feat: build homepage with hero, featured suppliers, industry grid, and CTA"
```

---

## Task 18: Supplier Search + Profile Pages

**Files:**
- Create: `app/(public)/suppliers/page.tsx`
- Create: `app/(public)/suppliers/[slug]/page.tsx`
- Create: `components/search/filter-sidebar.tsx`
- Create: `components/search/search-results.tsx`
- Create: `components/suppliers/supplier-profile-header.tsx`
- Create: `components/suppliers/certification-list.tsx`
- Create: `components/suppliers/verification-badge.tsx`
- Create: `components/leads/lead-form.tsx`

- [ ] **Step 1: Create FilterSidebar component**

Filters: Country (select), Industry (multi-select), ESG Level (checkboxes), Verification Status (checkboxes), Certification Type (multi-select). Applies filters to URL search params.

- [ ] **Step 2: Create SearchResults component**

Grid of SupplierCards with pagination. Shows result count and active filters.

- [ ] **Step 3: Build Supplier Search page**

SSR page at `/suppliers`. Reads search params, calls API, renders FilterSidebar + SearchResults. Meta title: "Find Green Suppliers | Green Suppliers".

- [ ] **Step 4: Create SupplierProfileHeader component**

Hero with banner, logo, company name, city, ESG badge, verification badge, sustainability metrics (renewable %, waste recycling %, carbon reporting).

- [ ] **Step 5: Create CertificationList component**

List of certifications with type, status badge, expiry date.

- [ ] **Step 6: Create LeadForm component**

Contact form: name, email, phone (optional), company (optional), message. Zod validation. Submits to POST /leads. Success/error feedback.

- [ ] **Step 7: Build Supplier Profile page**

SSR page at `/suppliers/[slug]`. Fetches full profile from API. Sections: header, description, industries, services, certifications, ESG methodology explanation, lead form. JSON-LD structured data (Organization + LocalBusiness).

- [ ] **Step 8: Commit**

```bash
git add -A && git commit -m "feat: build supplier search and profile pages with lead form"
```

---

## Task 19: SEO Landing Pages

**Files:**
- Create: `app/(public)/industries/[slug]/page.tsx`
- Create: `app/(public)/countries/[slug]/page.tsx`
- Create: `app/(public)/guides/[slug]/page.tsx`
- Create: `app/(public)/get-listed/page.tsx`
- Create: `components/leads/get-listed-form.tsx`
- Create: `components/seo/json-ld.tsx`
- Create: `components/seo/breadcrumbs.tsx`

- [ ] **Step 1: Create JSON-LD and Breadcrumbs components**

Reusable structured data component. Breadcrumbs with schema markup.

- [ ] **Step 2: Build Industry landing page**

SSR at `/industries/[slug]`. Fetches industry detail + filtered suppliers from API. Intro text, top suppliers grid, FAQ block, "Get Listed" CTA. JSON-LD: CollectionPage + ItemList.

- [ ] **Step 3: Build Country landing page**

SSR at `/countries/[slug]`. Same pattern as industry page but filtered by country.

- [ ] **Step 4: Build Guide/Content page**

SSR at `/guides/[slug]`. Fetches ContentPage from API. Renders markdown body. JSON-LD: Article.

- [ ] **Step 5: Build Get Listed page**

Form: company name, contact name, email, phone, website, industries (multi-select), country, city, description, certifications (freetext). Zod validation. Submits to POST /get-listed. Success confirmation.

- [ ] **Step 6: Commit**

```bash
git add -A && git commit -m "feat: build SEO landing pages and get-listed intake form"
```

---

## Task 20: SEO Infrastructure

**Files:**
- Create: `app/sitemap.ts`
- Create: `app/robots.ts`
- Modify: All page files to add metadata

- [ ] **Step 1: Create dynamic sitemap**

```typescript
// app/sitemap.ts
export default async function sitemap(): Promise<MetadataRoute.Sitemap> {
  // Fetch all published suppliers, active industries, active countries, published guides
  // Return URL entries for each
}
```

- [ ] **Step 2: Create robots.txt**

```typescript
// app/robots.ts
export default function robots(): MetadataRoute.Robots {
  return {
    rules: { userAgent: '*', allow: '/', disallow: '/admin/' },
    sitemap: 'https://greensuppliers.co.za/sitemap.xml',
  }
}
```

- [ ] **Step 3: Add metadata to all pages**

Each page exports `generateMetadata()` with:
- Title (template: `[Page] | Green Suppliers - SA's Green Directory`)
- Description (unique per page)
- Open Graph (title, description, image)
- Canonical URL

- [ ] **Step 4: Commit**

```bash
git add -A && git commit -m "feat: add sitemap, robots.txt, and SEO metadata to all pages"
```

---

## Task 21: Admin UI

**Files:**
- Create: `app/admin/layout.tsx`
- Create: `app/admin/login/page.tsx`
- Create: `app/admin/page.tsx`
- Create: `app/admin/suppliers/page.tsx`
- Create: `app/admin/suppliers/new/page.tsx`
- Create: `app/admin/suppliers/[id]/page.tsx`
- Create: `app/admin/taxonomy/page.tsx`
- Create: `app/admin/content/page.tsx`
- Create: `app/admin/content/[id]/page.tsx`
- Create: `app/admin/leads/page.tsx`
- Create: `app/admin/certifications/page.tsx`
- Create: `components/layout/admin-sidebar.tsx`

- [ ] **Step 1: Create Admin login page**

Email + password form. Calls POST /auth/login. Stores JWT in httpOnly cookie or localStorage. Redirects to /admin on success.

- [ ] **Step 2: Create Admin layout + sidebar**

Dark sidebar with nav: Dashboard, Suppliers, Certifications, Leads, Taxonomy, Content. JWT auth check — redirect to login if not authenticated.

- [ ] **Step 3: Build Admin Dashboard**

Overview stats: supplier count, pending certs, new leads, published content pages.

- [ ] **Step 4: Build Supplier Management pages**

List: TanStack Table with columns (name, country, ESG level, verification, published). Actions: edit, publish/unpublish, flag/unflag, rescore.
New: Form for creating supplier (all fields from CreateSupplierRequest).
Edit: Pre-populated form for updating supplier.

- [ ] **Step 5: Build Taxonomy Management page**

Tabs: Industries | Certification Types | Service Tags. Each tab has a list + add/edit modal.

- [ ] **Step 6: Build Content Management pages**

List: content pages with title, type, published status.
Editor: title, slug, meta title, meta desc, body (markdown textarea), page type, publish toggle.

- [ ] **Step 7: Build Lead Management page**

Table: contact name, email, company, supplier, status, date. Actions: update status (new → contacted → closed).

- [ ] **Step 8: Build Certification Review page**

Table: supplier name, cert type, status, expiry date, document link. Actions: accept/reject with notes.

- [ ] **Step 9: Commit**

```bash
git add -A && git commit -m "feat: build admin UI with supplier management, taxonomy, content, leads, and cert review"
```

---

## Task 22: Frontend Tests

**Files:**
- Create: `web/green-suppliers-web/__tests__/components/supplier-card.test.tsx`
- Create: `web/green-suppliers-web/__tests__/components/esg-badge.test.tsx`
- Create: `web/green-suppliers-web/__tests__/components/lead-form.test.tsx`
- Create: `web/green-suppliers-web/e2e/search.spec.ts`

- [ ] **Step 1: Set up Vitest**

```bash
npm install -D vitest @testing-library/react @testing-library/jest-dom @vitejs/plugin-react jsdom
```

Configure `vitest.config.ts`.

- [ ] **Step 2: Write component tests**

```typescript
// supplier-card.test.tsx
test('renders supplier name and ESG badge', () => { ... })
test('renders verified badge when verified', () => { ... })
test('links to correct supplier profile', () => { ... })

// esg-badge.test.tsx
test('renders Bronze with amber gradient', () => { ... })
test('renders Platinum with green gradient', () => { ... })

// lead-form.test.tsx
test('validates required fields', () => { ... })
test('submits valid form data', () => { ... })
test('shows success message on submission', () => { ... })
```

- [ ] **Step 3: Run component tests**

Run: `npm test`
Expected: All PASS

- [ ] **Step 4: Set up Playwright**

```bash
npm install -D @playwright/test
npx playwright install
```

- [ ] **Step 5: Write E2E test for critical path**

```typescript
// e2e/search.spec.ts
test('search for supplier, view profile, submit lead', async ({ page }) => {
  await page.goto('/suppliers');
  await page.fill('[data-testid="search-input"]', 'solar');
  await page.click('[data-testid="search-button"]');
  await expect(page.locator('[data-testid="supplier-card"]')).toBeVisible();
  await page.click('[data-testid="supplier-card"]:first-child a');
  await expect(page.locator('[data-testid="supplier-profile"]')).toBeVisible();
  await page.fill('[data-testid="lead-name"]', 'Test Buyer');
  await page.fill('[data-testid="lead-email"]', 'buyer@test.com');
  await page.fill('[data-testid="lead-message"]', 'Interested in your services');
  await page.click('[data-testid="lead-submit"]');
  await expect(page.locator('[data-testid="lead-success"]')).toBeVisible();
});
```

- [ ] **Step 6: Commit**

```bash
git add -A && git commit -m "feat: add frontend component tests and E2E search-to-lead test"
```

---

## Task 23: CI/CD Pipeline

**Files:**
- Create: `.github/workflows/ci.yml`

- [ ] **Step 1: Create GitHub Actions CI pipeline**

```yaml
name: CI
on: [push, pull_request]
jobs:
  backend:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with: { dotnet-version: '8.0.x' }
      - run: dotnet restore
      - run: dotnet build --no-restore
      - run: dotnet test --no-build --collect:"XPlat Code Coverage"
      # Fail if coverage below 80%

  frontend:
    runs-on: ubuntu-latest
    defaults:
      run:
        working-directory: web/green-suppliers-web
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-node@v4
        with: { node-version: '20' }
      - run: npm ci
      - run: npm run lint
      - run: npm run type-check
      - run: npm test -- --coverage
      - run: npm run build
```

- [ ] **Step 2: Verify pipeline config is valid**

Run: `dotnet build && dotnet test` locally to confirm all tests pass before push.

- [ ] **Step 3: Commit**

```bash
git add -A && git commit -m "feat: add GitHub Actions CI pipeline with build, test, lint, and coverage gates"
```

---

## Task 24: Final Integration + Smoke Test

- [ ] **Step 1: Start API and verify all endpoints**

```bash
cd src/GreenSuppliers.Api
dotnet run
```

Test with curl or Postman:
- `GET /api/v1/suppliers` → 200, empty list
- `POST /api/v1/auth/login` → 200, JWT token
- `POST /api/v1/admin/suppliers` (with JWT) → 201, supplier created
- `GET /api/v1/suppliers` → 200, 1 result
- `POST /api/v1/leads` → 201, lead created

- [ ] **Step 2: Start frontend and verify all pages**

```bash
cd web/green-suppliers-web
npm run dev
```

Visit:
- `/` → Homepage renders
- `/suppliers` → Search page renders
- `/admin/login` → Login form renders

- [ ] **Step 3: Run full test suite**

```bash
dotnet test
cd web/green-suppliers-web && npm test
```

Expected: All tests PASS

- [ ] **Step 4: Final commit**

```bash
git add -A && git commit -m "feat: Green Suppliers Phase 1 complete — public directory with admin, ESG scoring, and lead capture"
```
