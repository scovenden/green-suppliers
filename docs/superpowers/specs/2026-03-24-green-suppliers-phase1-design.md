# Green Suppliers — Phase 1 Design Spec

**Date:** 2026-03-24
**Status:** Approved
**Complexity Tier:** 2 (MEDIUM) — Service-layer monolith

---

## 1. Summary

Phase 1 delivers the public-facing Green Suppliers directory: a searchable, SEO-optimised catalogue of admin-seeded green supplier profiles for South Africa. Buyers can discover, filter, compare, and contact suppliers. A rules-based ESG scoring engine provides verification badges. Background jobs automate certification expiry and rescoring. No self-service registration, billing, or supplier dashboard in this phase.

### What Phase 1 Includes
- Admin-seeded supplier profiles (20-30 initially, growing via outreach)
- Public search + filters (country, industry, certification, ESG level)
- Individual supplier profile pages (SSR, SEO-optimised)
- SEO landing pages (industry, country, pillar guides)
- Lead capture forms (anonymous allowed, registered buyers get extras)
- ESG scoring engine (Bronze/Silver/Gold/Platinum)
- Verification state machine (unverified/pending/verified/flagged)
- Background jobs (cert expiry scanning, nightly rescore, email dispatch)
- Admin UI (supplier CRUD, taxonomy management, content editor, flags)
- Public "Get Listed" intake form
- SDG brand messaging (structured SDG tagging deferred to Phase 2)

### What Phase 1 Excludes
- Supplier self-registration and onboarding wizard
- Billing, subscriptions, and payment processing
- Supplier dashboard (leads, analytics)
- Sponsored placements (tables exist but no UI)
- Structured SDG mapping
- Multi-currency
- RFQ workflows
- API access for enterprise procurement systems
- Azure AI Search (SQL FTS behind interface)
- Microsoft Entra ID integration

---

## 2. Decisions Made

| Decision | Choice | Rationale |
|----------|--------|-----------|
| Architecture tier | Tier 2 — service-layer monolith | Complexity score 9/20; Clean Architecture is premature for 1-2 devs, 18 tables |
| Brand | Green Suppliers (greensuppliers.co.za), own green palette | Not Agilus red/black; sustainability-focused identity |
| Geographic scope | South Africa launch, pan-African data model | Country tables seeded for 10 African countries |
| Database | Azure SQL Database Standard S0 | Minimum tier for full-text search support |
| Backend | ASP.NET Core Web API (.NET 8+), EF Core | Agilus standard stack |
| Frontend | Next.js 14+ (App Router), Vercel hosting | SSR for SEO; Vercel for better Next.js DX |
| API hosting | Azure App Service | Simpler than Container Apps for Tier 2 |
| Search | Azure SQL full-text behind `ISupplierSearchService` | Abstracted for future swap to Azure AI Search |
| Admin auth | Seeded admin user + JWT | Entra ID deferred to Phase 2 |
| Email | SendGrid or Resend (TBD) | Transactional email for leads + cert reminders |
| Leads | Anonymous allowed, registered buyers get extras | Maximise leads, incentivise accounts |
| SDGs | Brand messaging only | Structured tagging in Phase 2 |
| Data seeding | 20-30 manual + outreach | "Get Listed" form for intake |
| WordPress | Keep live, back up, hard cutover | No downtime, no lost SEO equity |
| Logo | Existing logo from WordPress site | Reuse in Phase 1, refresh later if needed |

---

## 3. Architecture

### System Structure

```
GreenSuppliers/
  src/
    GreenSuppliers.Api/           -- Single .NET project
      Controllers/                -- Thin controllers
      Services/                   -- Business logic
      Models/
        Entities/                 -- EF Core entities
        DTOs/                     -- Request/response DTOs
        Enums/                    -- VerificationStatus, EsgLevel, etc.
      Data/
        GreenSuppliersDbContext.cs
        Configurations/           -- EF Core entity type configs
        Migrations/
      Middleware/                  -- Error handling, request logging
      Auth/                       -- JWT auth
      Validators/                 -- Input validation

    GreenSuppliers.Worker/        -- Separate .NET Worker Service
      Jobs/
        CertExpiryScanner.cs
        NightlyRescore.cs
        EmailDispatch.cs

  web/
    green-suppliers-web/          -- Next.js 14 App Router
      app/
        (public)/                 -- SSR public pages
        admin/                    -- Admin route group (CSR, protected)
      components/
        ui/                       -- shadcn/ui
        suppliers/                -- Supplier-specific
        search/                   -- Search and filters
        leads/                    -- Lead forms
        layout/                   -- Header, footer, nav
      lib/
        api-client.ts             -- Typed API client
        types.ts
        validators.ts             -- Zod schemas
```

### Key Architectural Rules
1. Controllers are thin — validate input, call a service, return a response
2. Services own business logic — ESG scoring, verification, cert expiry
3. Services call DbContext directly — no repository pattern
4. Interfaces only where justified — `ISupplierSearchService` for future swap
5. DTOs separate from entities

### Infrastructure
- **API:** Azure App Service
- **Frontend:** Vercel (Next.js)
- **Database:** Azure SQL Database Standard S0
- **Storage:** Azure Blob Storage (documents, certificates, logos)
- **Secrets:** Azure Key Vault
- **Monitoring:** Azure Application Insights
- **CI/CD:** GitHub Actions (build, test, lint, type-check, coverage gate 80%)

---

## 4. Data Model

18 tables total (14 Phase 1 + 4 Phase 2 stubs). All use UUID primary keys (`NEWSEQUENTIALID()`), `CreatedAt`/`UpdatedAt`, soft deletes on user-facing entities.

### Phase 1 Tables

| Table | Purpose |
|-------|---------|
| `Organizations` | Companies (supplier or buyer type) |
| `Users` | Auth, roles (supplier_admin, supplier_user, buyer, admin) |
| `SupplierProfiles` | The listing — structured ESG data, verification status, ESG level |
| `Industries` | Taxonomy with hierarchical parent support |
| `SupplierIndustries` | Many-to-many join |
| `ServiceTags` | Product/service tags |
| `SupplierServiceTags` | Many-to-many join |
| `CertificationTypes` | ISO 14001, B-Corp, FSC, etc. |
| `SupplierCertifications` | Specific certs with expiry dates and status |
| `Documents` | Uploaded files metadata (actual files in Azure Blob) |
| `Leads` | Buyer inquiries with status tracking |
| `Countries` | Reference data (ISO 3166-1 alpha-2) |
| `ContentPages` | CMS-lite for SEO pillar pages |
| `AuditEvents` | Append-only log of sensitive actions |

### Phase 2 Stub Tables (schema only, no UI)
- `Plans` — subscription tiers (free/pro/premium)
- `Subscriptions` — org-to-plan with billing state
- `Payments` — payment records
- `SponsoredPlacements` — paid featured slots

Full SQL schema is in [design-milestone-01.md](../decisions/design-milestone-01.md).

---

## 5. API Surface (Phase 1)

Base path: `/api/v1/`. Standard response envelope per global CLAUDE.md.

### Public Endpoints (13)
- `GET /suppliers` — search + filter + paginate
- `GET /suppliers/{slug}` — single profile
- `GET /suppliers/{slug}/certifications` — certs for a supplier
- `GET /industries` — list all active
- `GET /industries/{slug}` — industry detail + suppliers
- `GET /countries` — list active countries
- `GET /countries/{code}` — country detail + suppliers
- `GET /service-tags` — list active tags
- `GET /content/{slug}` — CMS page by slug
- `POST /leads` — submit lead (captcha for anonymous)
- `POST /get-listed` — supplier intake form submission (see fields below)
- `POST /auth/login` — admin login
- `POST /auth/refresh` — refresh JWT token

### Admin Endpoints (17)
- Supplier CRUD: list, create, update, change status, publish/unpublish, trigger rescore
- Certification management: list, accept/reject
- Lead management: list, update status
- Taxonomy CRUD: industries, certification types, service tags
- Content CRUD: create/update pages
- Document upload

### Search Query Parameters
```
?q=solar&countryCode=ZA&industrySlug=renewable-energy&esgLevel=gold
&verificationStatus=verified&certTypeSlug=iso-14001&tags=solar-panels
&sortBy=esgScore&page=1&pageSize=20
```

### "Get Listed" Intake Form Fields
```
POST /api/v1/get-listed
{
  companyName: string (required)
  contactName: string (required)
  contactEmail: string (required)
  contactPhone: string (optional)
  website: string (optional)
  industryIds: string[] (optional, multi-select from industries)
  country: string (required, ISO 3166-1 alpha-2)
  city: string (optional)
  description: string (required, max 500 chars)
  certifications: string (optional, freetext — "ISO 14001, B-Corp")
}
```
Submissions are stored as leads with a `type: 'get_listed'` flag. Admin receives email notification. Admin then creates the supplier profile manually from the submission data.

---

## 6. ESG Scoring Engine

### `EsgScoringService`

Triggers: profile update, cert status change, nightly rescore.

```
Input:  SupplierProfile + List<SupplierCertification> (accepted, not expired)
Output: EsgScoreResult { Level, NumericScore, List<Reasons> }

Rules:
  None (0):       Missing required fields
  Bronze (25):    All required fields complete
  Silver (50):    >= 1 valid cert + renewable_energy_percent >= 20
  Gold (75):      >= 2 valid certs + renewable_energy_percent >= 50 + carbon_reporting
  Platinum (100): >= 3 valid certs + renewable >= 70% + waste_recycling >= 70% + carbon_reporting
```

Methodology displayed on each supplier profile for transparency.

### Verification State Machine

```
unverified → pending    (cert uploaded)
pending    → verified   (cert accepted + profile complete)
verified   → unverified (all certs expired)
any        → flagged    (admin action)
flagged    → unverified (admin resolves)
```

---

## 7. Background Jobs

| Job | Schedule | What It Does |
|-----|----------|-------------|
| `CertExpiryScanner` | Daily 2am | Find certs expiring in 30/14/7 days; send reminders; mark expired; trigger rescore |
| `NightlyRescore` | Daily 3am | Re-run EsgScoringService + VerificationService on all published suppliers |
| `EmailDispatch` | Continuous | Process email queue (lead notifications, cert reminders, get-listed confirmations) |

### Email Triggers

| Event | Recipient | Template |
|-------|-----------|----------|
| Lead submitted | Supplier contact | "New inquiry from [buyer name]" |
| Lead submitted | Buyer email | "Your inquiry has been sent to [supplier]" |
| Cert expiring (30/14/7d) | Supplier contact | "Your [cert] expires in X days" |
| Cert expired | Supplier contact | "Your [cert] has expired — ESG level updated" |
| Get Listed submitted | Admin | "New listing request from [company]" |

---

## 8. Frontend Pages

All public pages are server-side rendered for SEO.

| Route | Rendering | Purpose |
|-------|-----------|---------|
| `/` | SSR | Homepage — search hero, featured suppliers, industries, ESG explainer, CTA |
| `/suppliers` | SSR | Search results with filter sidebar |
| `/suppliers/[slug]` | SSR | Supplier profile (ESG badge, certs, lead form) |
| `/industries/[slug]` | SSR | SEO industry page + filtered supplier list |
| `/countries/[slug]` | SSR | SEO country page + filtered supplier list |
| `/guides/[slug]` | SSR | CMS pillar content pages |
| `/get-listed` | SSR | Intake form for new suppliers |
| `/admin/*` | CSR | Admin dashboard (protected, JWT auth) |

### Design Direction
- Modern glassmorphism aesthetic with gradient hero
- Green brand palette (#16A34A primary, #166534 dark, #059669 emerald accent)
- Bold typography (800 weight headings, tight letter-spacing)
- ESG level-coloured card headers (Bronze/amber, Silver/grey, Gold/amber, Platinum/green gradients)
- Floating stats card overlapping hero
- Industry grid with icons and supplier counts
- Existing Green Suppliers logo (leaf in circle + wordmark)
- shadcn/ui + Tailwind CSS
- Mobile-first responsive, WCAG 2.1 AA

---

## 9. SEO Strategy

### URL Structure
```
/suppliers/{slug}                    -- Supplier profiles
/industries/{slug}                   -- Industry landing pages
/countries/{slug}                    -- Country landing pages
/guides/{slug}                       -- Pillar content pages
```

### Structured Data (JSON-LD)
- Supplier profiles: `Organization` + `LocalBusiness`
- Industry/country pages: `CollectionPage` + `ItemList`
- Guide pages: `Article`
- Homepage: `WebSite` + `SearchAction`
- All pages: `BreadcrumbList`

### Phase 1 Content
- 4 pillar guides (Green Manufacturing, Renewable Energy, Sustainable Packaging, ESG Compliance)
- 1 active country page (South Africa), 9 seeded/unpublished
- 8 industry pages

### Technical SEO
- Auto-generated sitemap.xml
- Robots.txt (disallow /admin/*)
- Self-referencing canonical URLs
- Meta title template: `[Page] | Green Suppliers - SA's Green Directory`
- OG tags on every page
- Next.js Image optimisation, lazy loading

### WordPress Cutover Plan
1. Full WordPress backup to Azure Blob before build starts
2. WordPress stays live during development
3. New site tested on Vercel preview URLs
4. DNS cutover when ready + 301 redirects for changed paths
5. Monitor Search Console post-cutover

---

## 10. Testing Strategy

### Frameworks
- Backend: xUnit + FluentAssertions + Moq
- Frontend: Vitest + React Testing Library + Playwright (E2E)
- API: Integration tests against real Azure SQL test database

### Coverage Targets
- Services layer: 90%+ (ESG scoring, verification — critical)
- Controllers: 80%+
- Frontend components: 70%+

### Priority Test Areas
1. ESG scoring rules — all 5 levels with edge cases
2. Verification state transitions
3. Certification expiry logic (30/14/7 day thresholds)
4. Search + filter accuracy
5. Lead submission (valid, invalid, rate-limited)
6. E2E: search → profile → lead submission

---

## 11. Security

- Admin endpoints behind JWT + role claim check
- Lead submission: rate limit 10/hour per IP, captcha for anonymous
- No sensitive data in public endpoints (org email/phone only if opted in)
- File uploads: validate content type, max 10MB
- EF Core parameterized queries only, no raw SQL
- CORS: allow Next.js frontend origin only
- Passwords: bcrypt/argon2
- Audit log for sensitive actions (profile changes, cert verification, flagging)

---

## 12. Phase Roadmap

### Phase 1 — Public Directory (Current)
Everything in this spec.

### Phase 2 — Self-Service + Monetisation
- Supplier self-registration + email verification
- Profile wizard (self-service)
- Subscription billing (Free/Pro/Premium) via PayFast
- Supplier dashboard (leads, analytics, billing)
- Sponsored placements
- Structured SDG tagging
- Microsoft Entra ID for admin

### Phase 3 — Pan-African Expansion
- Multi-country content activation
- Multi-currency billing
- RFQ workflow + success fees
- Buyer subscriptions
- API access for enterprise procurement systems
