# Technical Design: Milestone 1 -- Public Directory

**Date:** 2026-03-24
**Project Complexity Tier:** 2 (MEDIUM)

---

## Summary

Milestone 1 delivers the public-facing Green Suppliers directory: admin-seeded supplier profiles with search/filter, individual profile pages (SSR/SEO), industry and country landing pages, lead capture forms, and background jobs for certification expiry and ESG rescoring. No self-service registration, no billing, no supplier dashboard.

---

## Data Model

All tables use UUID primary keys (`uniqueidentifier` in SQL Server, generated as `NEWSEQUENTIALID()` for index performance). All user-facing entities have `CreatedAt`, `UpdatedAt`, and soft-delete (`IsDeleted`, `DeletedAt`).

### Core Tables (Phase 1)

```sql
-- =============================================================
-- ORGANIZATIONS
-- =============================================================
CREATE TABLE Organizations (
    Id              UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
    Name            NVARCHAR(200)    NOT NULL,
    RegistrationNo  NVARCHAR(50)     NULL,        -- Company reg number (CIPC in SA)
    CountryCode     CHAR(2)          NOT NULL,     -- ISO 3166-1 alpha-2
    City            NVARCHAR(100)    NULL,
    Province        NVARCHAR(100)    NULL,
    Website         NVARCHAR(500)    NULL,
    Phone           NVARCHAR(30)     NULL,
    Email           NVARCHAR(254)    NULL,
    OrganizationType NVARCHAR(20)    NOT NULL,     -- 'supplier' | 'buyer' | 'admin'
    IsDeleted       BIT              NOT NULL DEFAULT 0,
    DeletedAt       DATETIME2        NULL,
    CreatedAt       DATETIME2        NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt       DATETIME2        NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_Organizations PRIMARY KEY (Id)
);

CREATE INDEX IX_Organizations_CountryCode ON Organizations (CountryCode) WHERE IsDeleted = 0;
CREATE INDEX IX_Organizations_Type ON Organizations (OrganizationType) WHERE IsDeleted = 0;

-- =============================================================
-- USERS
-- =============================================================
CREATE TABLE Users (
    Id              UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
    OrganizationId  UNIQUEIDENTIFIER NOT NULL,
    Email           NVARCHAR(254)    NOT NULL,
    PasswordHash    NVARCHAR(500)    NOT NULL,
    FirstName       NVARCHAR(100)    NOT NULL,
    LastName        NVARCHAR(100)    NOT NULL,
    Role            NVARCHAR(30)     NOT NULL,     -- 'supplier_admin' | 'supplier_user' | 'buyer' | 'admin'
    IsActive        BIT              NOT NULL DEFAULT 1,
    EmailVerified   BIT              NOT NULL DEFAULT 0,
    LastLoginAt     DATETIME2        NULL,
    IsDeleted       BIT              NOT NULL DEFAULT 0,
    DeletedAt       DATETIME2        NULL,
    CreatedAt       DATETIME2        NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt       DATETIME2        NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_Users PRIMARY KEY (Id),
    CONSTRAINT FK_Users_Organization FOREIGN KEY (OrganizationId) REFERENCES Organizations(Id),
    CONSTRAINT UQ_Users_Email UNIQUE (Email)
);

CREATE INDEX IX_Users_OrganizationId ON Users (OrganizationId) WHERE IsDeleted = 0;

-- =============================================================
-- SUPPLIER PROFILES
-- =============================================================
CREATE TABLE SupplierProfiles (
    Id                      UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
    OrganizationId          UNIQUEIDENTIFIER NOT NULL,
    Slug                    NVARCHAR(250)    NOT NULL,     -- URL-friendly unique slug
    TradingName             NVARCHAR(200)    NULL,
    Description             NVARCHAR(4000)   NULL,
    ShortDescription        NVARCHAR(500)    NULL,
    LogoUrl                 NVARCHAR(1000)   NULL,
    BannerUrl               NVARCHAR(1000)   NULL,
    YearFounded             INT              NULL,
    EmployeeCount           NVARCHAR(30)     NULL,         -- '1-10' | '11-50' | '51-200' | '201-500' | '500+'
    BbbeeLevel              NVARCHAR(20)     NULL,         -- SA-specific: Level 1-8 or 'exempt'
    CountryCode             CHAR(2)          NOT NULL,
    City                    NVARCHAR(100)    NULL,
    Province                NVARCHAR(100)    NULL,

    -- Sustainability attributes (used in ESG scoring)
    RenewableEnergyPercent  INT              NULL CHECK (RenewableEnergyPercent BETWEEN 0 AND 100),
    WasteRecyclingPercent   INT              NULL CHECK (WasteRecyclingPercent BETWEEN 0 AND 100),
    CarbonReporting         BIT              NOT NULL DEFAULT 0,
    WaterManagement         BIT              NOT NULL DEFAULT 0,
    SustainablePackaging    BIT              NOT NULL DEFAULT 0,

    -- Computed / engine-maintained fields
    VerificationStatus      NVARCHAR(20)     NOT NULL DEFAULT 'unverified',  -- unverified | pending | verified | flagged
    EsgLevel                NVARCHAR(20)     NOT NULL DEFAULT 'none',        -- none | bronze | silver | gold | platinum
    EsgScore                INT              NOT NULL DEFAULT 0,             -- Numeric score for ranking
    IsPublished             BIT              NOT NULL DEFAULT 1,
    PublishedAt             DATETIME2        NULL,
    FlaggedReason           NVARCHAR(500)    NULL,
    LastScoredAt            DATETIME2        NULL,

    IsDeleted               BIT              NOT NULL DEFAULT 0,
    DeletedAt               DATETIME2        NULL,
    CreatedAt               DATETIME2        NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt               DATETIME2        NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT PK_SupplierProfiles PRIMARY KEY (Id),
    CONSTRAINT FK_SupplierProfiles_Organization FOREIGN KEY (OrganizationId) REFERENCES Organizations(Id),
    CONSTRAINT UQ_SupplierProfiles_Slug UNIQUE (Slug),
    CONSTRAINT UQ_SupplierProfiles_Organization UNIQUE (OrganizationId)  -- 1:1
);

CREATE INDEX IX_SupplierProfiles_CountryCode ON SupplierProfiles (CountryCode) WHERE IsDeleted = 0 AND IsPublished = 1;
CREATE INDEX IX_SupplierProfiles_EsgLevel ON SupplierProfiles (EsgLevel) WHERE IsDeleted = 0 AND IsPublished = 1;
CREATE INDEX IX_SupplierProfiles_Verification ON SupplierProfiles (VerificationStatus) WHERE IsDeleted = 0;
CREATE INDEX IX_SupplierProfiles_EsgScore ON SupplierProfiles (EsgScore DESC) WHERE IsDeleted = 0 AND IsPublished = 1;

-- Full-text index (created after full-text catalog setup)
-- CREATE FULLTEXT INDEX ON SupplierProfiles (TradingName, Description, ShortDescription)
--   KEY INDEX PK_SupplierProfiles ON GreenSuppliersCatalog;

-- =============================================================
-- INDUSTRIES (taxonomy)
-- =============================================================
CREATE TABLE Industries (
    Id          UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
    Name        NVARCHAR(150)    NOT NULL,
    Slug        NVARCHAR(150)    NOT NULL,
    Description NVARCHAR(500)    NULL,
    ParentId    UNIQUEIDENTIFIER NULL,          -- Hierarchical: parent industry
    SortOrder   INT              NOT NULL DEFAULT 0,
    IsActive    BIT              NOT NULL DEFAULT 1,
    CreatedAt   DATETIME2        NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_Industries PRIMARY KEY (Id),
    CONSTRAINT FK_Industries_Parent FOREIGN KEY (ParentId) REFERENCES Industries(Id),
    CONSTRAINT UQ_Industries_Slug UNIQUE (Slug)
);

-- =============================================================
-- SUPPLIER_INDUSTRIES (many-to-many)
-- =============================================================
CREATE TABLE SupplierIndustries (
    SupplierProfileId UNIQUEIDENTIFIER NOT NULL,
    IndustryId        UNIQUEIDENTIFIER NOT NULL,
    CONSTRAINT PK_SupplierIndustries PRIMARY KEY (SupplierProfileId, IndustryId),
    CONSTRAINT FK_SI_Supplier FOREIGN KEY (SupplierProfileId) REFERENCES SupplierProfiles(Id),
    CONSTRAINT FK_SI_Industry FOREIGN KEY (IndustryId) REFERENCES Industries(Id)
);

-- =============================================================
-- SERVICE TAGS
-- =============================================================
CREATE TABLE ServiceTags (
    Id          UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
    Name        NVARCHAR(100)    NOT NULL,
    Slug        NVARCHAR(100)    NOT NULL,
    IsActive    BIT              NOT NULL DEFAULT 1,
    CreatedAt   DATETIME2        NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_ServiceTags PRIMARY KEY (Id),
    CONSTRAINT UQ_ServiceTags_Slug UNIQUE (Slug)
);

-- =============================================================
-- SUPPLIER_SERVICE_TAGS (many-to-many)
-- =============================================================
CREATE TABLE SupplierServiceTags (
    SupplierProfileId UNIQUEIDENTIFIER NOT NULL,
    ServiceTagId      UNIQUEIDENTIFIER NOT NULL,
    CONSTRAINT PK_SupplierServiceTags PRIMARY KEY (SupplierProfileId, ServiceTagId),
    CONSTRAINT FK_SST_Supplier FOREIGN KEY (SupplierProfileId) REFERENCES SupplierProfiles(Id),
    CONSTRAINT FK_SST_Tag FOREIGN KEY (ServiceTagId) REFERENCES ServiceTags(Id)
);

-- =============================================================
-- CERTIFICATION TYPES (taxonomy)
-- =============================================================
CREATE TABLE CertificationTypes (
    Id          UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
    Name        NVARCHAR(200)    NOT NULL,     -- 'ISO 14001', 'B-Corp', 'FSC', etc.
    Slug        NVARCHAR(200)    NOT NULL,
    Description NVARCHAR(500)    NULL,
    IsActive    BIT              NOT NULL DEFAULT 1,
    CreatedAt   DATETIME2        NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_CertificationTypes PRIMARY KEY (Id),
    CONSTRAINT UQ_CertificationTypes_Slug UNIQUE (Slug)
);

-- =============================================================
-- SUPPLIER CERTIFICATIONS
-- =============================================================
CREATE TABLE SupplierCertifications (
    Id                  UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
    SupplierProfileId   UNIQUEIDENTIFIER NOT NULL,
    CertificationTypeId UNIQUEIDENTIFIER NOT NULL,
    CertificateNumber   NVARCHAR(100)    NULL,
    IssuedAt            DATE             NULL,
    ExpiresAt           DATE             NULL,
    DocumentId          UNIQUEIDENTIFIER NULL,          -- Link to uploaded certificate
    Status              NVARCHAR(20)     NOT NULL DEFAULT 'pending',  -- pending | accepted | rejected | expired
    VerifiedByUserId    UNIQUEIDENTIFIER NULL,
    VerifiedAt          DATETIME2        NULL,
    Notes               NVARCHAR(500)    NULL,
    CreatedAt           DATETIME2        NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt           DATETIME2        NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_SupplierCertifications PRIMARY KEY (Id),
    CONSTRAINT FK_SC_Supplier FOREIGN KEY (SupplierProfileId) REFERENCES SupplierProfiles(Id),
    CONSTRAINT FK_SC_CertType FOREIGN KEY (CertificationTypeId) REFERENCES CertificationTypes(Id),
    CONSTRAINT FK_SC_VerifiedBy FOREIGN KEY (VerifiedByUserId) REFERENCES Users(Id)
);

CREATE INDEX IX_SC_SupplierProfile ON SupplierCertifications (SupplierProfileId);
CREATE INDEX IX_SC_ExpiresAt ON SupplierCertifications (ExpiresAt) WHERE Status = 'accepted';

-- =============================================================
-- DOCUMENTS
-- =============================================================
CREATE TABLE Documents (
    Id                  UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
    SupplierProfileId   UNIQUEIDENTIFIER NOT NULL,
    FileName            NVARCHAR(255)    NOT NULL,
    BlobUrl             NVARCHAR(1000)   NOT NULL,
    ContentType         NVARCHAR(100)    NOT NULL,
    FileSizeBytes       BIGINT           NOT NULL,
    DocumentType        NVARCHAR(50)     NOT NULL,     -- 'certificate' | 'policy' | 'report' | 'logo' | 'banner'
    UploadedByUserId    UNIQUEIDENTIFIER NULL,
    CreatedAt           DATETIME2        NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_Documents PRIMARY KEY (Id),
    CONSTRAINT FK_Documents_Supplier FOREIGN KEY (SupplierProfileId) REFERENCES SupplierProfiles(Id),
    CONSTRAINT FK_Documents_UploadedBy FOREIGN KEY (UploadedByUserId) REFERENCES Users(Id)
);

CREATE INDEX IX_Documents_SupplierProfile ON Documents (SupplierProfileId);

-- =============================================================
-- LEADS
-- =============================================================
CREATE TABLE Leads (
    Id                  UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
    SupplierProfileId   UNIQUEIDENTIFIER NOT NULL,
    BuyerOrganizationId UNIQUEIDENTIFIER NULL,         -- NULL if anonymous
    BuyerUserId         UNIQUEIDENTIFIER NULL,         -- NULL if anonymous
    ContactName         NVARCHAR(150)    NOT NULL,
    ContactEmail        NVARCHAR(254)    NOT NULL,
    ContactPhone        NVARCHAR(30)     NULL,
    CompanyName         NVARCHAR(200)    NULL,
    Message             NVARCHAR(2000)   NOT NULL,
    Status              NVARCHAR(20)     NOT NULL DEFAULT 'new',  -- new | contacted | closed
    IpAddress           NVARCHAR(45)     NULL,
    CreatedAt           DATETIME2        NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt           DATETIME2        NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_Leads PRIMARY KEY (Id),
    CONSTRAINT FK_Leads_Supplier FOREIGN KEY (SupplierProfileId) REFERENCES SupplierProfiles(Id),
    CONSTRAINT FK_Leads_BuyerOrg FOREIGN KEY (BuyerOrganizationId) REFERENCES Organizations(Id),
    CONSTRAINT FK_Leads_BuyerUser FOREIGN KEY (BuyerUserId) REFERENCES Users(Id)
);

CREATE INDEX IX_Leads_SupplierProfile ON Leads (SupplierProfileId);
CREATE INDEX IX_Leads_Status ON Leads (Status) WHERE Status = 'new';

-- =============================================================
-- COUNTRIES (reference data)
-- =============================================================
CREATE TABLE Countries (
    Code        CHAR(2)         NOT NULL,      -- ISO 3166-1 alpha-2
    Name        NVARCHAR(100)   NOT NULL,
    Slug        NVARCHAR(100)   NOT NULL,
    Region      NVARCHAR(50)    NULL,          -- 'Southern Africa', 'East Africa', etc.
    IsActive    BIT             NOT NULL DEFAULT 1,
    SortOrder   INT             NOT NULL DEFAULT 0,
    CONSTRAINT PK_Countries PRIMARY KEY (Code),
    CONSTRAINT UQ_Countries_Slug UNIQUE (Slug)
);

-- =============================================================
-- CONTENT PAGES (CMS-lite for SEO pillar pages)
-- =============================================================
CREATE TABLE ContentPages (
    Id          UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
    Slug        NVARCHAR(300)    NOT NULL,
    Title       NVARCHAR(200)    NOT NULL,
    MetaTitle   NVARCHAR(200)    NULL,
    MetaDesc    NVARCHAR(300)    NULL,
    Body        NVARCHAR(MAX)    NOT NULL,      -- Markdown or HTML
    PageType    NVARCHAR(30)     NOT NULL,       -- 'guide' | 'country' | 'industry' | 'general'
    IsPublished BIT              NOT NULL DEFAULT 0,
    PublishedAt DATETIME2        NULL,
    CreatedAt   DATETIME2        NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt   DATETIME2        NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_ContentPages PRIMARY KEY (Id),
    CONSTRAINT UQ_ContentPages_Slug UNIQUE (Slug)
);

-- =============================================================
-- AUDIT EVENTS (append-only log)
-- =============================================================
CREATE TABLE AuditEvents (
    Id          UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
    UserId      UNIQUEIDENTIFIER NULL,
    Action      NVARCHAR(100)    NOT NULL,     -- 'supplier.created', 'cert.verified', 'lead.submitted', etc.
    EntityType  NVARCHAR(100)    NOT NULL,
    EntityId    UNIQUEIDENTIFIER NOT NULL,
    OldValues   NVARCHAR(MAX)    NULL,         -- JSON
    NewValues   NVARCHAR(MAX)    NULL,         -- JSON
    IpAddress   NVARCHAR(45)     NULL,
    CreatedAt   DATETIME2        NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_AuditEvents PRIMARY KEY (Id)
);

CREATE INDEX IX_AuditEvents_Entity ON AuditEvents (EntityType, EntityId);
CREATE INDEX IX_AuditEvents_CreatedAt ON AuditEvents (CreatedAt DESC);

-- =============================================================
-- PHASE 2 TABLES (create now, empty until Phase 2)
-- =============================================================
CREATE TABLE Plans (
    Id              UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
    Name            NVARCHAR(50)     NOT NULL,     -- 'free' | 'pro' | 'premium'
    DisplayName     NVARCHAR(100)    NOT NULL,
    PriceMonthly    DECIMAL(10,2)    NOT NULL,
    PriceYearly     DECIMAL(10,2)    NOT NULL,
    Currency        CHAR(3)          NOT NULL DEFAULT 'ZAR',
    MaxLeadsPerMonth INT             NULL,
    MaxDocuments    INT              NULL,
    FeaturedListing BIT              NOT NULL DEFAULT 0,
    AnalyticsAccess BIT              NOT NULL DEFAULT 0,
    SortOrder       INT              NOT NULL DEFAULT 0,
    IsActive        BIT              NOT NULL DEFAULT 1,
    CreatedAt       DATETIME2        NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_Plans PRIMARY KEY (Id)
);

CREATE TABLE Subscriptions (
    Id              UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
    OrganizationId  UNIQUEIDENTIFIER NOT NULL,
    PlanId          UNIQUEIDENTIFIER NOT NULL,
    Status          NVARCHAR(20)     NOT NULL DEFAULT 'active',  -- active | past_due | cancelled | expired
    BillingCycle    NVARCHAR(10)     NOT NULL DEFAULT 'monthly', -- monthly | yearly
    CurrentPeriodStart DATETIME2     NOT NULL,
    CurrentPeriodEnd   DATETIME2     NOT NULL,
    CancelledAt     DATETIME2        NULL,
    ExternalId      NVARCHAR(200)    NULL,     -- PayFast/Stripe subscription ID
    CreatedAt       DATETIME2        NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt       DATETIME2        NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_Subscriptions PRIMARY KEY (Id),
    CONSTRAINT FK_Subscriptions_Org FOREIGN KEY (OrganizationId) REFERENCES Organizations(Id),
    CONSTRAINT FK_Subscriptions_Plan FOREIGN KEY (PlanId) REFERENCES Plans(Id)
);

CREATE TABLE Payments (
    Id              UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
    SubscriptionId  UNIQUEIDENTIFIER NOT NULL,
    Amount          DECIMAL(10,2)    NOT NULL,
    Currency        CHAR(3)          NOT NULL DEFAULT 'ZAR',
    Status          NVARCHAR(20)     NOT NULL,  -- pending | succeeded | failed | refunded
    Provider        NVARCHAR(20)     NOT NULL,  -- 'payfast' | 'stripe'
    ExternalId      NVARCHAR(200)    NULL,
    PaidAt          DATETIME2        NULL,
    CreatedAt       DATETIME2        NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_Payments PRIMARY KEY (Id),
    CONSTRAINT FK_Payments_Subscription FOREIGN KEY (SubscriptionId) REFERENCES Subscriptions(Id)
);

CREATE TABLE SponsoredPlacements (
    Id              UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
    SupplierProfileId UNIQUEIDENTIFIER NOT NULL,
    PlacementType   NVARCHAR(30)     NOT NULL,  -- 'search_top' | 'category_banner' | 'homepage_featured'
    StartsAt        DATETIME2        NOT NULL,
    EndsAt          DATETIME2        NOT NULL,
    ImpressionsCount INT             NOT NULL DEFAULT 0,
    ClicksCount     INT              NOT NULL DEFAULT 0,
    IsActive        BIT              NOT NULL DEFAULT 1,
    CreatedAt       DATETIME2        NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_SponsoredPlacements PRIMARY KEY (Id),
    CONSTRAINT FK_SP_Supplier FOREIGN KEY (SupplierProfileId) REFERENCES SupplierProfiles(Id)
);
```

### Entity Count Summary

| Phase | Table | Purpose |
|-------|-------|---------|
| 1 | Organizations | Companies (suppliers and buyers) |
| 1 | Users | User accounts with roles |
| 1 | SupplierProfiles | The core listing entity |
| 1 | Industries | Taxonomy of green industries |
| 1 | SupplierIndustries | M2M join |
| 1 | ServiceTags | Freeform product/service tags |
| 1 | SupplierServiceTags | M2M join |
| 1 | CertificationTypes | Taxonomy of certifications |
| 1 | SupplierCertifications | A supplier's specific cert instance |
| 1 | Documents | Uploaded files metadata |
| 1 | Leads | Buyer inquiries |
| 1 | Countries | Reference data |
| 1 | ContentPages | CMS-lite for SEO content |
| 1 | AuditEvents | Append-only audit log |
| 2 | Plans | Subscription tiers |
| 2 | Subscriptions | Org-to-plan link with billing state |
| 2 | Payments | Payment records |
| 2 | SponsoredPlacements | Paid featured slots |

**Total: 18 tables (14 Phase 1 + 4 Phase 2)**

---

## API Surface Map (Phase 1)

All endpoints return the standard API response envelope. Base path: `/api/v1/`.

```
API SURFACE MAP -- PHASE 1
============================================================================
METHOD  PATH                                    AUTH     VALIDATED  NOTES
----------------------------------------------------------------------------
PUBLIC DISCOVERY (anonymous access)
GET     /api/v1/suppliers                       No       Yes        Search + filter + paginate
GET     /api/v1/suppliers/{slug}                No       Yes        Single profile by slug
GET     /api/v1/suppliers/{slug}/certifications No       Yes        Certs for a supplier
GET     /api/v1/industries                      No       No         List all active industries
GET     /api/v1/industries/{slug}               No       Yes        Industry detail + suppliers
GET     /api/v1/countries                       No       No         List active countries
GET     /api/v1/countries/{code}                No       Yes        Country detail + suppliers
GET     /api/v1/service-tags                    No       No         List active tags
GET     /api/v1/content/{slug}                  No       Yes        CMS page by slug

LEADS (anonymous + authenticated)
POST    /api/v1/leads                           No*      Yes        Submit lead (*captcha for anon)

AUTH (admin only in Phase 1)
POST    /api/v1/auth/login                      No       Yes        Email + password login
POST    /api/v1/auth/refresh                    No       Yes        Refresh JWT token

ADMIN (admin role required)
GET     /api/v1/admin/suppliers                 Admin    Yes        List all suppliers (incl. unpublished)
POST    /api/v1/admin/suppliers                 Admin    Yes        Create supplier + org
PUT     /api/v1/admin/suppliers/{id}            Admin    Yes        Update supplier profile
PATCH   /api/v1/admin/suppliers/{id}/status     Admin    Yes        Change verification status
PATCH   /api/v1/admin/suppliers/{id}/publish    Admin    Yes        Publish / unpublish
POST    /api/v1/admin/suppliers/{id}/rescore    Admin    No         Trigger ESG rescore

GET     /api/v1/admin/certifications            Admin    Yes        List certs (filter by status)
PATCH   /api/v1/admin/certifications/{id}       Admin    Yes        Accept / reject cert

GET     /api/v1/admin/leads                     Admin    Yes        List all leads
PATCH   /api/v1/admin/leads/{id}/status         Admin    Yes        Update lead status

POST    /api/v1/admin/industries                Admin    Yes        Create industry
PUT     /api/v1/admin/industries/{id}           Admin    Yes        Update industry
POST    /api/v1/admin/certification-types       Admin    Yes        Create cert type
PUT     /api/v1/admin/certification-types/{id}  Admin    Yes        Update cert type
POST    /api/v1/admin/service-tags              Admin    Yes        Create tag

POST    /api/v1/admin/content                   Admin    Yes        Create content page
PUT     /api/v1/admin/content/{id}              Admin    Yes        Update content page

POST    /api/v1/admin/documents/upload          Admin    Yes        Upload document to Blob
----------------------------------------------------------------------------
TOTAL: 28 endpoints | Auth gaps: 0 | Public: 11 | Admin: 17
============================================================================
```

### Search Query Parameters (GET /api/v1/suppliers)

```
?q=solar                       -- Full-text search on name + description
&countryCode=ZA                -- Filter by country
&industrySlug=renewable-energy -- Filter by industry
&esgLevel=gold                 -- Filter by ESG level (none|bronze|silver|gold|platinum)
&verificationStatus=verified   -- Filter by verification
&certTypeSlug=iso-14001        -- Filter by certification type
&tags=solar-panels,inverters   -- Filter by service tags (comma-separated slugs)
&sortBy=esgScore               -- Sort: esgScore (default) | name | newest
&page=1                        -- Page number
&pageSize=20                   -- Items per page (max 50)
```

---

## Key Service Contracts

### EsgScoringService

```csharp
public class EsgScoringService
{
    // Called on: profile update, cert status change, nightly rescore
    public EsgScoreResult CalculateScore(SupplierProfile profile, List<SupplierCertification> activeCerts);
}

public record EsgScoreResult(EsgLevel Level, int NumericScore, List<string> Reasons);
```

**Scoring rules (from CLAUDE.md):**
- None: incomplete profile (missing required fields)
- Bronze (score 25): all required fields complete
- Silver (score 50): >= 1 accepted cert + renewable >= 20%
- Gold (score 75): >= 2 accepted certs + renewable >= 50% + carbon reporting
- Platinum (score 100): >= 3 accepted certs + renewable >= 70% + waste recycling >= 70% + carbon reporting

### VerificationService

```csharp
public class VerificationService
{
    // State transitions: unverified -> pending (cert uploaded)
    //                    pending -> verified (cert accepted + profile complete)
    //                    verified -> unverified (cert expired, no other valid certs)
    //                    any -> flagged (admin action)
    //                    flagged -> unverified (admin resolves)
    public VerificationStatus Evaluate(SupplierProfile profile, List<SupplierCertification> certs);
}
```

### ISupplierSearchService (interface for future swap)

```csharp
public interface ISupplierSearchService
{
    Task<PagedResult<SupplierSearchResult>> SearchAsync(SupplierSearchQuery query, CancellationToken ct);
}

// Phase 1 implementation: SqlFullTextSearchService (uses Azure SQL CONTAINS/FREETEXT)
// Phase 2+ upgrade path: AzureAiSearchService (uses Azure AI Search)
```

---

## Out of Scope for Milestone 1

- Supplier self-registration and onboarding wizard
- Billing, subscriptions, and payment processing
- Supplier dashboard (leads, analytics)
- Sponsored placements (tables exist but no UI/API to manage)
- SDG mapping
- Multi-currency
- RFQ workflows
- API access for enterprise procurement systems
- Azure AI Search (use SQL FTS, abstracted behind interface)
- Microsoft Entra ID integration

---

## Security Considerations

- Admin endpoints behind JWT + role claim check
- Lead submission: rate limit (10/hour per IP), captcha for anonymous users
- No sensitive data exposed in public supplier endpoints (no email/phone of org unless explicitly opted in)
- File uploads: validate content type, max 10MB, scan for common exploits
- SQL injection: EF Core parameterized queries only, no raw SQL
- CORS: allow Next.js frontend origin only

---

## Test Plan (Phase 1 Priority)

### Unit Tests (xUnit)
- EsgScoringService: all 5 levels with edge cases
- VerificationService: all state transitions
- CertificationExpiryService: 30/14/7 day thresholds, expired cert handling

### Integration Tests
- Supplier search with various filter combinations
- Lead submission (valid, invalid, rate limited)
- Admin CRUD operations
- Full-text search accuracy

### E2E Tests (Playwright)
- Search for supplier by keyword, filter, view profile
- Submit a lead form
- Admin: create supplier, upload cert, verify, check public visibility

---

## Open Questions / Decisions Needed Before Coding

1. **Hosting decision:** Azure App Service vs Azure Container Apps for the API? (Recommend App Service for simplicity at this tier)
2. **Next.js hosting:** Vercel vs Azure Static Web Apps? (Vercel is simpler; Azure SWA keeps everything in Azure)
3. **Email provider:** SendGrid, Azure Communication Services, or Resend? (Need transactional email for cert reminders and lead notifications)
4. **Admin auth in Phase 1:** Simple JWT with seeded admin user, or Entra ID from day one? (Recommend seeded admin + JWT; Entra ID is Phase 2)
5. **Full-text search catalog:** Need to decide Azure SQL tier -- Basic does not support full-text. Standard S0+ required.
