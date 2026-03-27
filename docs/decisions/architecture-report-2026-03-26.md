# Architecture Report -- Green Suppliers -- 2026-03-26

Post-Sprint 3 Review (Phase 1 complete, Phase 2 Sprints 0-3 complete)

---

## Complexity Assessment

| Dimension | Phase 1 (Mar 24) | Current (Mar 26) | Score |
|-----------|-------------------|-------------------|-------|
| Entities | 18 tables | 21 active entities (+ 4 Phase 2 stubs) | 2/4 |
| API endpoints | 28 | 60 | 3/4 |
| External integrations | 2 (Blob, console email) | 3 (Blob Storage, Resend email, JWT auth) | 2/4 |
| Team size | 1 developer | 1 developer | 1/4 |
| Business rules | ESG scoring, verification, cert expiry | + auth state machine, registration, email verification, password reset, lead status transitions, profile publication rules, buyer saved suppliers, profile completeness scoring | 3/4 |
| **Total** | **9/20** | **11/20** | |

**Tier: 2 (MEDIUM) -- upper boundary of range (9-13)**

### Entity Count Detail

Active entities with DbSet registrations in GreenSuppliersDbContext:

| # | Entity | Status |
|---|--------|--------|
| 1 | Organization | Active |
| 2 | User | Active |
| 3 | SupplierProfile | Active |
| 4 | Industry | Active |
| 5 | ServiceTag | Active |
| 6 | CertificationType | Active |
| 7 | SupplierCertification | Active |
| 8 | Document | Active |
| 9 | Lead | Active |
| 10 | Country | Active |
| 11 | ContentPage | Active |
| 12 | AuditEvent | Active |
| 13 | EmailQueueItem | Active |
| 14 | RefreshToken | Active (Sprint 1) |
| 15 | SavedSupplier | Active (Sprint 3) |
| 16 | SupplierIndustry | Active (join table) |
| 17 | SupplierServiceTag | Active (join table) |
| 18 | Plan | Stub (Phase 2) |
| 19 | Subscription | Stub (Phase 2) |
| 20 | Payment | Stub (Phase 2) |
| 21 | SponsoredPlacement | Stub (Phase 2) |

**Active entities: 17 (excluding join tables) or 21 (including stubs)**
**Entity count trigger (25) NOT hit.** The stubs do not count -- they have no services, no endpoints, and no business logic.

---

## Codebase Size

| Metric | Value |
|--------|-------|
| Total C# source files (excluding migrations) | 134 |
| Total lines of C# (excluding migrations) | ~7,844 |
| Test files | 18 |
| Test lines | ~7,209 |
| Migration files | 5 |
| API controllers | 17 |
| Service classes | 14 (12 API + 2 Worker) |
| Entity files | 21 |
| DTO files | 30 |
| Validators | 12 |
| Test-to-code ratio | ~0.92:1 (good) |

---

## ADR-0001 Upgrade Trigger Assessment

ADR-0001 defined five upgrade triggers for moving from Tier 2 to Tier 3. Here is the current status of each:

| # | Trigger | Threshold | Current | Hit? |
|---|---------|-----------|---------|------|
| 1 | Team size | > 4 developers | 1 developer | NO |
| 2 | Entity count | > 25 | 17 active (21 with stubs) | NO |
| 3 | Bounded contexts emerge | Billing becomes complex | Billing not yet built | NO |
| 4 | Service classes exceed 300 lines regularly | 300+ lines | **3 services over 300** | **PARTIAL** |
| 5 | Circular dependencies between services | Any cycle | No cycles detected | NO |

### Trigger 4 Detail: Services Over 300 Lines

| Service | Lines | Assessment |
|---------|-------|------------|
| **SupplierMeService** | **616** | **CRITICAL -- 2x the threshold** |
| **AccountService** | **374** | Over threshold, ~115 lines are email HTML templates |
| **SupplierService** | **366** | Over threshold, ~60 lines are DTO mapping |
| TaxonomyService | 302 | At threshold, acceptable (4 CRUD domains in 1 service) |
| LeadService | 205 | Under threshold |
| BuyerService | 169 | Under threshold |
| SqlFullTextSearchService | 138 | Under threshold |
| ContentService | 120 | Under threshold |
| EsgScoringService | 108 | Under threshold |
| VerificationService | 67 | Under threshold |
| DocumentService | 46 | Under threshold |
| AuditService | 32 | Under threshold |

**Verdict: Trigger 4 is partially hit.** Three services exceed 300 lines, but the root cause is addressable within Tier 2 (extraction into sub-services, not an architectural tier change). See Critical Findings below.

---

## Service Dependency Graph

```
                   AuditService (leaf -- no service deps)
                  /     |     \       \         \
   SupplierService  SupplierMeService  LeadService  BuyerService  AccountService
        |     \         |     \
   EsgScoringService  VerificationService

   ContentService      (standalone -- DbContext only)
   DocumentService     (standalone -- DbContext only)
   TaxonomyService     (standalone -- DbContext only)
   SqlFullTextSearchService (standalone -- DbContext only)
```

**No circular dependencies detected.** All service dependencies flow downward:
- SupplierService -> EsgScoringService, VerificationService, AuditService
- SupplierMeService -> EsgScoringService, VerificationService, AuditService
- LeadService -> AuditService
- BuyerService -> AuditService
- AccountService -> AuditService

EsgScoringService and VerificationService are pure logic services (no dependencies on other services). AuditService is a leaf dependency.

**Trigger 5 (circular dependencies): NOT hit.**

---

## Architectural Drift Check

### vs ADR-0001 (Tier 2 Service-Layer Monolith)

| ADR-0001 Rule | Current Status | Compliant? |
|---------------|---------------|------------|
| Controllers are thin | Most controllers are thin. SupplierMeController is 323 lines but delegates to service. | YES |
| Services own business logic | ESG scoring, verification, registration, lead management all in services. | YES |
| Services call DbContext directly | All services call DbContext directly, no repository pattern. | YES |
| Interfaces only where justified | Only ISupplierSearchService has an interface. IEmailSender in Worker. | YES |
| DTOs separate from entities | All DTOs in Models/DTOs/, entities in Models/Entities/. | YES |
| Worker is separate project | GreenSuppliers.Worker is a separate project referencing the API project. | YES |

### Layer Discipline Violations

**FINDING: 5 controllers inject DbContext directly.**

Controllers that bypass the service layer and access DbContext:

| Controller | DbContext Usage | Severity |
|------------|----------------|----------|
| AuthController | Login + refresh token logic directly queries Users table | MEDIUM |
| AdminSuppliersController | Some admin queries bypass SupplierService | LOW |
| AdminCertificationsController | Direct DB queries for certification management | LOW |
| AdminDashboardController | Direct DB queries for dashboard stats | LOW |
| SuppliersController | Direct DB queries for public supplier listing | LOW |

The AuthController is the most significant violation -- login and token refresh logic are in the controller rather than a service. This should be extracted to AccountService or a dedicated AuthService.

The admin controllers doing direct DB reads for simple queries is acceptable at Tier 2 but should be monitored. If the pattern grows, extract to an AdminService.

### vs ADR-0002 (Remain at Tier 2 for Phase 2)

ADR-0002 predicted:
- ~27 tables by end of Phase 2 -> Current: 21 (stubs inactive, on track)
- ~67 endpoints -> Current: 60 (Sprints 0-3 of 6 complete, on track)
- Services under 300 lines -> **VIOLATED: 3 services over 300 lines**
- No circular dependencies -> **CONFIRMED: no cycles**

---

## Code Duplication Findings

### CRITICAL: BuildProfileDto and MapToDto duplication

`SupplierService.BuildProfileDtoAsync()` (lines 277-348, 70 lines) and `SupplierMeService.BuildProfileDtoAsync()` (lines 547-615, 68 lines) contain **nearly identical** DTO mapping logic. Both:
1. Load the same includes (Organization, Industries, ServiceTags, Certifications)
2. Map to the same `SupplierProfileDto`
3. Map the same nested DTOs (SupplierIndustryDto, SupplierServiceTagDto, SupplierCertificationDto)

Similarly, `SupplierMeService.MapLeadToDto()` and `BuyerService.MapLeadToDto()` and `LeadService.MapToDto()` all contain identical Lead-to-LeadDto mapping (3 copies of the same 18-line method).

### CRITICAL: RunScoringAsync duplication

`SupplierService.RunScoringAsync()` (lines 258-275) and `SupplierMeService.RunScoringAsync()` (lines 528-545) are **identical**. Both load certifications, call EsgScoringService, call VerificationService, and save.

---

## API Surface Map

```
API SURFACE MAP (60 endpoints across 17 controllers)
===========================================================================
METHOD  PATH                                          AUTH     VALIDATED
---------------------------------------------------------------------------
PUBLIC ENDPOINTS (no auth required)
GET     /api/v1/suppliers                             -        -
GET     /api/v1/suppliers/search                      -        -
GET     /api/v1/suppliers/{slug}                      -        -
GET     /api/v1/industries                            -        -
GET     /api/v1/industries/{slug}                     -        -
GET     /api/v1/countries                             -        -
GET     /api/v1/countries/{code}                      -        -
GET     /api/v1/service-tags                          -        -
GET     /api/v1/content/{slug}                        -        -
POST    /api/v1/leads                                 -        YES
POST    /api/v1/get-listed                            -        YES

AUTH ENDPOINTS (rate limited)
POST    /api/v1/auth/login                            -        YES
POST    /api/v1/auth/refresh                          -        -
POST    /api/v1/auth/register                         -        YES
POST    /api/v1/auth/verify-email                     -        YES
POST    /api/v1/auth/forgot-password                  -        YES
POST    /api/v1/auth/reset-password                   -        YES

SUPPLIER SELF-SERVICE (/supplier/me) -- Requires Supplier policy
GET     /api/v1/supplier/me/profile                   Supplier  -
PUT     /api/v1/supplier/me/profile                   Supplier  YES
GET     /api/v1/supplier/me/certifications            Supplier  -
POST    /api/v1/supplier/me/certifications            Supplier  YES
GET     /api/v1/supplier/me/documents                 Supplier  -
POST    /api/v1/supplier/me/documents                 Supplier  YES (inline)
PUT     /api/v1/supplier/me/publish                   Supplier  -
GET     /api/v1/supplier/me/dashboard                 Supplier  -
GET     /api/v1/supplier/me/leads                     Supplier  -
GET     /api/v1/supplier/me/leads/{id}                Supplier  -
PATCH   /api/v1/supplier/me/leads/{id}/status         Supplier  -

BUYER ENDPOINTS -- Requires Buyer policy
GET     /api/v1/buyer/saved-suppliers                 Buyer     -
POST    /api/v1/buyer/saved-suppliers                 Buyer     -
DELETE  /api/v1/buyer/saved-suppliers/{id}            Buyer     -
GET     /api/v1/buyer/leads                           Buyer     -
GET     /api/v1/buyer/dashboard                       Buyer     -

ADMIN ENDPOINTS -- Requires Admin policy
GET     /api/v1/admin/suppliers                       Admin     -
POST    /api/v1/admin/suppliers                       Admin     YES
PUT     /api/v1/admin/suppliers/{id}                  Admin     YES
PATCH   /api/v1/admin/suppliers/{id}/verification     Admin     -
PATCH   /api/v1/admin/suppliers/{id}/publish          Admin     -
POST    /api/v1/admin/suppliers/{id}/rescore          Admin     -
GET     /api/v1/admin/taxonomy/industries             Admin     -
POST    /api/v1/admin/taxonomy/industries             Admin     -
PUT     /api/v1/admin/taxonomy/industries/{id}        Admin     -
GET     /api/v1/admin/taxonomy/cert-types             Admin     -
POST    /api/v1/admin/taxonomy/cert-types             Admin     -
PUT     /api/v1/admin/taxonomy/cert-types/{id}        Admin     -
GET     /api/v1/admin/taxonomy/service-tags           Admin     -
POST    /api/v1/admin/taxonomy/service-tags           Admin     -
GET     /api/v1/admin/taxonomy/countries              Admin     -
POST    /api/v1/admin/taxonomy/countries              Admin     -
GET     /api/v1/admin/certifications                  Admin     -
PATCH   /api/v1/admin/certifications/{id}             Admin     -
GET     /api/v1/admin/leads                           Admin     -
PATCH   /api/v1/admin/leads/{id}                      Admin     -
GET     /api/v1/admin/content                         Admin     -
POST    /api/v1/admin/content                         Admin     -
PUT     /api/v1/admin/content/{slug}                  Admin     -
GET     /api/v1/admin/dashboard                       Admin     -
GET     /api/v1/admin/dashboard/recent-activity       Admin     -
POST    /api/v1/admin/documents/upload                Admin     -

HEALTH
GET     /health                                       -         -
===========================================================================
Total: 60 endpoints | Auth gaps: 0 | Controllers with direct DB access: 5
```

---

## Architecture Health Summary

| Area | Status | Finding |
|------|--------|---------|
| Layer discipline | WARNING | 5 controllers inject DbContext directly. AuthController login logic should be in a service. |
| API surface | OK | 60 endpoints, all auth-protected as expected. Public endpoints correctly open. |
| DB schema quality | OK | UUID PKs, soft deletes, audit timestamps, FTS indexes present. |
| CLAUDE.md accuracy | WARNING | Phase roadmap outdated (still shows Phase 2 as future). Architecture section needs update for current service count and new entities. Email provider decided (Resend, not SendGrid). Auth section outdated. |
| ADR coverage | OK | ADR-0001 (architecture tier), ADR-0002 (Phase 2 tier decision). Missing: ADR for Resend email provider choice, ADR for auth approach (JWT + refresh tokens). |
| Tech drift | WARNING | CLAUDE.md says "SendGrid or Resend (TBD)" but Resend is now the implemented choice. Auth section says "Seeded admin user + JWT" but full registration/verification/reset is now implemented. |
| Code duplication | CRITICAL | DTO mapping duplicated across 3 services. RunScoringAsync duplicated between SupplierService and SupplierMeService. |
| Service size | WARNING | SupplierMeService at 616 lines is 2x the ADR-0001 threshold. Addressable via extraction. |
| Test coverage | OK | 7,209 lines of tests for 7,844 lines of code. Good test-to-code ratio. Key services tested. |

---

## Critical Findings (fix before Sprint 4)

### 1. SupplierMeService is 616 lines -- 2x the ADR-0001 threshold

**What:** SupplierMeService has grown to 616 lines, more than double the 300-line threshold documented in ADR-0001 as an upgrade trigger.

**Why it matters:** At 616 lines, the service handles profile management, certification submission, publication requests, dashboard stats, lead management, lead status transitions, completeness calculation, ESG scoring delegation, and DTO building. This is too many responsibilities for one class.

**Fix (within Tier 2 -- no architectural upgrade needed):**

Extract into focused sub-services:
- `SupplierMeService` -> profile operations only (~200 lines)
- `SupplierCertificationMeService` -> certification CRUD for self-service (~80 lines)
- `SupplierLeadMeService` -> leads inbox and status transitions (~120 lines)
- `SupplierDashboardService` -> dashboard stats and completeness (~80 lines)
- Extract shared `ProfileMappingHelper` static class -> BuildProfileDto + RunScoring (~80 lines)

Estimated post-refactor: no service over 250 lines.

### 2. Duplicated DTO mapping across 3 services

**What:** `BuildProfileDtoAsync` is copy-pasted between SupplierService (70 lines) and SupplierMeService (68 lines). `MapLeadToDto` is copy-pasted across LeadService, SupplierMeService, and BuyerService (18 lines x 3).

**Why it matters:** Any change to SupplierProfileDto or LeadDto requires updating 2-3 places. This is a maintenance and correctness risk.

**Fix:** Extract to static mapping helpers:
- `ProfileMapper.ToDto(SupplierProfile profile)` -- shared by all services
- `LeadMapper.ToDto(Lead lead)` -- shared by all services
- `ProfileMapper.BuildAsync(DbContext, Guid profileId)` -- shared eager-load + map

### 3. RunScoringAsync duplicated between SupplierService and SupplierMeService

**What:** Both services contain identical 18-line `RunScoringAsync` methods that load certs, call EsgScoringService, call VerificationService, and save.

**Fix:** Extract to a shared `ScoringOrchestrator` or add a `RescoreProfileAsync(Guid profileId)` method to EsgScoringService that encapsulates the full workflow.

---

## High Findings (fix this sprint or next)

### 4. AuthController contains login logic that should be in a service

**What:** AuthController directly queries Users table, verifies passwords, updates LastLoginAt, and generates tokens. This is business logic in a controller.

**Why it matters:** Violates the "controllers are thin" rule from ADR-0001. Login logic is untestable without an integration test. The refresh token logic is similarly in the controller.

**Fix:** Extract login and refresh logic to AccountService (or a dedicated AuthService). The controller should be: validate input, call service, return response.

### 5. 4 admin controllers inject DbContext directly

**What:** AdminSuppliersController, AdminCertificationsController, AdminDashboardController, and SuppliersController all inject GreenSuppliersDbContext alongside services.

**Why it matters:** For simple read-only admin queries (dashboard stats, listing), direct DB access in controllers is acceptable at Tier 2. However, it should not grow further. AdminCertificationsController does writes (status changes) through DbContext -- this should go through a service.

**Fix:** Monitor. If more admin controllers need writes through DbContext, extract an AdminService. AdminCertificationsController write operations should move to a service now.

### 6. AccountService contains 115 lines of email HTML templates

**What:** `BuildVerificationEmailHtml` and `BuildPasswordResetEmailHtml` are inline HTML template methods totaling 115 lines (30% of the service).

**Fix:** Extract to an `EmailTemplateService` or static `EmailTemplates` class. This is a readability fix, not architectural.

---

## Tier Upgrade Decision

### Should we upgrade to Tier 3 (Clean Architecture)?

**NO.** The recommendation is to remain at Tier 2.

**Rationale:**
- Only 1 of 5 ADR-0001 upgrade triggers is partially hit (service size), and it is addressable within Tier 2 via extraction into sub-services
- Team size is still 1 developer -- the primary signal for needing Clean Architecture (team > 4) is far from being hit
- No circular dependencies exist
- Entity count (17 active) is well below the 25 threshold
- No bounded contexts have emerged -- billing (Sprint 4) will be a single PayFast service
- The code duplication issues are extractable to shared helpers, not architectural layers

**Cost of upgrading now:** Restructuring 134 source files into 4 projects (Domain, Application, Infrastructure, API), adding interfaces for all services, configuring cross-project DI. Estimated 3-5 days with zero feature value. Sprint 4 (PayFast billing) is higher priority.

**Action:** Fix Critical Findings 1-3 (extract sub-services and shared helpers) to bring all services under 300 lines. This resolves the trigger without an architectural change.

---

## Bounded Context Assessment

### Are bounded contexts emerging?

| Candidate | Entities | Services | Verdict |
|-----------|----------|----------|---------|
| Supplier Directory | SupplierProfile, Organization, Industry, ServiceTag, CertificationType, SupplierCertification, Document, Country | SupplierService, SupplierMeService, EsgScoringService, VerificationService, TaxonomyService, SqlFullTextSearchService, DocumentService | Core domain, well-defined |
| Auth & Accounts | User, Organization, RefreshToken | AccountService, JwtTokenService | Distinct concern but small |
| Leads | Lead, SavedSupplier | LeadService, BuyerService | Overlaps with Supplier (leads belong to profiles) |
| Content | ContentPage | ContentService | Trivial -- 1 entity, 1 service |
| Billing (Sprint 4) | Plan, Subscription, Payment | (not yet built) | Will be distinct once PayFast is integrated |

**Verdict:** No bounded context is complex enough to warrant extraction. Auth + Accounts is the closest to a distinct boundary, but at 2 services and 3 entities, it does not justify a separate project. Billing might change this in Phase 3 if multi-provider (PayFast + Stripe) + invoicing + dunning is added.

---

## Sprint 4 (PayFast Billing) -- Pre-Build Architecture Guidance

### Decisions Needed Before Sprint 4

1. **PayFast integration pattern:** Single `PayFastService` handling subscription creation, ITN (Instant Transaction Notification) webhooks, payment recording, and cancellation. At Tier 2, this is one service -- not a bounded context.

2. **Webhook security:** PayFast ITN webhooks must be validated (IP allowlist + signature verification). This is middleware or a filter on the webhook endpoint, not a service concern.

3. **Subscription state machine:** Plan enforcement (what features each tier unlocks) should be a separate `PlanEnforcementService` from the payment processing in `PayFastService`. This keeps both under 300 lines.

4. **Entity activation:** The Plan, Subscription, and Payment entity stubs already exist. They need:
   - Real EF Core configurations with proper indexes
   - Migration to add any missing columns (e.g., PayFast transaction IDs, ITN data)
   - Service implementations

5. **New ADR needed:** ADR-0003 should document the PayFast integration decision (why PayFast first, Stripe as fallback, webhook pattern, subscription state machine).

### Projected Post-Sprint 4 Complexity

| Dimension | Current | Post-Sprint 4 | Score |
|-----------|---------|---------------|-------|
| Entities | 17 active | ~22 active (+Plan, Subscription, Payment, PayFastWebhookEvent, SponsoredPlacement active) | 2/4 |
| API endpoints | 60 | ~72 (+subscription CRUD, PayFast webhook, billing dashboard, plan enforcement) | 3/4 |
| External integrations | 3 | 4 (+PayFast) | 2/4 |
| Team size | 1 | 1 | 1/4 |
| Business rules | 3/4 | + billing state machine, plan enforcement, webhook validation | 3/4 |
| **Total** | **11/20** | **12/20** | |

**Verdict:** Still Tier 2 after Sprint 4. The score moves from 11 to 12, remaining well within the 9-13 range.

---

## CLAUDE.md Updates Required

The following sections of CLAUDE.md are outdated and need updating:

1. **Tech Stack > Backend > Email:** Change from "SendGrid or Resend (TBD)" to "Resend (implemented)"
2. **Tech Stack > Backend > Auth:** Change from "Seeded admin user + JWT (Entra ID deferred to Phase 2)" to "JWT + refresh tokens, supplier/buyer self-registration with email verification, password reset (Entra ID deferred to Phase 3)"
3. **Architecture Pattern > Complexity score:** Update from "9/20" to "11/20"
4. **Key Domain Entities:** Add RefreshToken and SavedSupplier to Core Entities
5. **Phase Roadmap:** Update Phase 2 to reflect Sprint 0-3 completion status
6. **References:** Add link to this report and ADR-0002

---

## Actions Taken This Run

- **Report created:** `docs/decisions/architecture-report-2026-03-26.md`
- **CLAUDE.md updated:** Tech stack (email, auth), complexity score, entities, phase roadmap, references
- **ADRs reviewed:** ADR-0001 and ADR-0002 remain valid; no supersession needed
- **ADRs to create (deferred):**
  - ADR-0003: PayFast billing integration pattern (before Sprint 4 starts)
  - ADR-0004: Email provider choice (Resend) -- optional, low priority

---

## Recommended Next Architect Run

**Trigger:** After Sprint 4 (PayFast billing) is complete, OR if any of:
- A service exceeds 400 lines after the Critical Findings refactoring
- Circular dependencies appear between services
- Team grows past 2 active developers
- Phase 3 scope is confirmed

**Focus areas for next run:**
- Re-score with billing active
- Check if PayFast + Subscription services stay under 300 lines
- Assess whether billing is emerging as a bounded context
- Review webhook reliability patterns (idempotency, retry handling)
