# Green Suppliers — Project CLAUDE.md

## Product Overview

**Green Suppliers** is a SaaS directory and marketplace portal that lists verified green/sustainable suppliers. The platform enables procurement and ESG teams to discover, filter, compare, and contact eco-conscious suppliers.

- **Domain:** greensuppliers.co.za
- **Launch market:** South Africa
- **Expansion:** Pan-African (data model supports multiple countries from day one)
- **Business model:** SaaS — Free / Pro / Premium supplier subscriptions, sponsored placements, lead capture
- **Phase 1 focus:** Public directory with seeded supplier data, search/discovery, SEO pages. Self-service onboarding and billing are Phase 2.

### What It Does
- Buyers search, filter, and compare verified green suppliers
- Supplier profiles display structured ESG data, certifications, and verification badges
- Rules-based verification and ESG scoring (Bronze / Silver / Gold / Platinum)
- Automated certification expiry tracking and reminders
- Lead capture (buyer-to-supplier contact forms)
- Monetisation via tiered subscriptions and sponsored placements

### Who It's For
- **Buyers:** Procurement managers, ESG officers, and sustainability teams at mid-to-large enterprises sourcing sustainable suppliers in Africa
- **Suppliers:** Green manufacturers, renewable energy providers, sustainable service companies seeking visibility and credibility
- **Admins:** Exception-handling only (fraud, disputes, taxonomy management)

---

## ICP — Ideal Customer Profile

### Buyer ICP
- **Job title:** Procurement Manager, ESG Officer, Supply Chain Director, Sustainability Lead
- **Company size:** 50–5000+ employees
- **Industry:** Manufacturing, construction, mining, retail, FMCG, financial services
- **Geography:** South Africa (Phase 1), expanding to Kenya, Nigeria, Ghana, Egypt, Morocco, Rwanda, Tanzania, Uganda, Botswana
- **Pain:** Difficulty finding verified, ESG-compliant suppliers in Africa; manual due diligence is slow and expensive; no trusted centralised directory exists

### Supplier ICP
- **Type:** Green manufacturers, renewable energy companies, sustainable packaging, waste management, water solutions, eco-construction, sustainable agriculture
- **Size:** SME to mid-market
- **Pain:** Lack of visibility to enterprise buyers; no standardised way to showcase ESG credentials; losing deals to less-green competitors who market better

---

## Tech Stack

### Backend
- **Framework:** ASP.NET Core Web API (.NET 8+)
- **Architecture:** Tier 2 — Service-layer monolith (see ADR-0001)
- **ORM:** Entity Framework Core (direct DbContext access, no repository pattern)
- **Background jobs:** .NET Worker Service (separate project)
- **Email:** Resend (transactional email for lead notifications, cert reminders, verification, password reset)
- **Auth:** JWT access tokens + refresh tokens, supplier/buyer self-registration with email verification, password reset flow (Entra ID deferred to Phase 3)

### Database
- **Primary:** Azure SQL Database
- **Full-text search:** Azure SQL full-text indexes on supplier profiles
- **File storage:** Azure Blob Storage (certifications, documents, media)
- **Migrations:** EF Core migrations

### Frontend
- **Framework:** Next.js 14+ (App Router) with TypeScript strict mode
- **UI:** shadcn/ui + Tailwind CSS (custom green/sustainable brand palette — NOT Agilus colours)
- **Server state:** TanStack Query v5
- **Forms:** React Hook Form + Zod validation
- **Tables:** TanStack Table v8
- **Charts:** Recharts (supplier analytics dashboards)
- **SSR:** Server-side rendering for all public/SEO pages

### Cloud (Azure)
- Azure App Service (API hosting)
- Azure SQL Database Standard S0 (minimum for full-text search)
- Azure Blob Storage (documents, certificates, media)
- Azure Key Vault (secrets, API keys)
- Azure Application Insights (monitoring)
- Azure Service Bus (messaging/events — Phase 2)

### Frontend Hosting
- Vercel (Next.js hosting — better DX, free tier for MVP)

### Payments (Phase 2)
- PayFast (South Africa) — primary
- Stripe (international fallback)
- Must support recurring subscriptions + webhooks

---

## Architecture Pattern

**Tier 2 — Service-layer monolith** (see [ADR-0001](docs/decisions/0001-architecture-tier-and-pattern.md))

Complexity score: 11/20 (re-scored 2026-03-26). Clean Architecture is premature for 1 dev and 21 tables. Upgrade triggers documented in ADR-0001, re-assessed in ADR-0002 and architecture-report-2026-03-26.

### Folder Structure

```
GreenSuppliers/
  src/
    GreenSuppliers.Api/           -- Single .NET project (134 source files)
      Controllers/                -- 17 controllers (thin, delegate to services)
      Services/                   -- 12 service classes (business logic)
      Models/
        Entities/                 -- 21 EF Core entities (POCOs)
        DTOs/                     -- 30 request/response DTOs
        Enums/                    -- VerificationStatus, EsgLevel, LeadStatus, etc.
      Data/
        GreenSuppliersDbContext.cs
        SeedData.cs               -- Initial data seeding
        Configurations/           -- 22 EF Core entity type configurations
        Migrations/               -- 5 migrations
      Middleware/                  -- Error handling, request logging, security headers
      Auth/                       -- JwtTokenService
      Validators/                 -- 12 FluentValidation validators
      Helpers/                    -- SlugHelper
      Extensions/                 -- ClaimsPrincipalExtensions
      Program.cs

    GreenSuppliers.Worker/        -- Separate .NET Worker Service
      Jobs/
        CertExpiryScanner.cs      -- Daily cert expiry check + email reminders
        NightlyRescore.cs         -- Nightly ESG rescore for all profiles
        EmailDispatch.cs          -- Async email dispatch from queue
      Services/
        IEmailSender.cs           -- Email sender interface
        ResendEmailSender.cs      -- Resend API implementation
        ConsoleEmailSender.cs     -- Dev fallback

  tests/
    GreenSuppliers.Tests/         -- 18 test files (~7,200 lines)

  web/
    green-suppliers-web/          -- Next.js 14 App Router (not yet built)

  docs/decisions/                 -- ADRs + design docs + architecture reports
  CLAUDE.md
```

### Key Architectural Rules
1. **Controllers are thin** — validate input, call a service, return a response
2. **Services own business logic** — ESG scoring, verification, cert expiry
3. **Services call DbContext directly** — EF Core IS the repository/unit-of-work, no repository pattern
4. **Interfaces only where justified** — `ISupplierSearchService` (future swap to Azure AI Search). Other services: no interface needed.
5. **DTOs separate from entities** — API contracts are separate from EF Core entities

### Key Architectural Decisions
- SQL-first: all core data in Azure SQL, no NoSQL
- UUID primary keys on all tables (`NEWSEQUENTIALID()` for index performance)
- Multi-tenant by country (single database, country_code column)
- API versioning: `/api/v1/`
- Standard API response envelope (see global CLAUDE.md)
- Supplier profiles are the central entity — everything links to them
- Soft deletes on user-facing entities (`IsDeleted`, `DeletedAt`)

---

## Key Domain Entities

### Core Entities
- **Organization** — company (supplier or buyer), has users
- **User** — belongs to organization, has role (supplier_admin, supplier_user, buyer, admin)
- **SupplierProfile** — the listing: legal name, trading name, country, city, description, sustainability attributes, verification status, ESG level
- **Industry** — taxonomy (renewable energy, construction, agriculture, etc.)
- **ServiceTag** — freeform tags for products/services
- **CertificationType** — ISO 14001, B-Corp, etc.
- **SupplierCertification** — a supplier's specific certification with expiry date and status
- **Document** — uploaded files (certificates, policies, carbon reports)
- **Lead** — buyer inquiry to a supplier
- **ContentPage** — CMS-lite for SEO pillar/industry/country pages
- **AuditEvent** — append-only log of sensitive actions
- **RefreshToken** — JWT refresh tokens for auth session management
- **SavedSupplier** — buyer's saved/bookmarked suppliers
- **EmailQueueItem** — queued emails for async dispatch by Worker

### Phase 2 Entities
- **Plan** — subscription tier (free, pro, premium)
- **Subscription** — links organization to plan with payment state
- **Payment** — individual payment records
- **SponsoredPlacement** — paid featured slots

### Relationships
```
Organization 1──* User
Organization 1──1 SupplierProfile
SupplierProfile *──* Industry (via supplier_industries)
SupplierProfile *──* ServiceTag (via supplier_service_tags)
SupplierProfile 1──* SupplierCertification
SupplierCertification *──1 CertificationType
SupplierProfile 1──* Document
SupplierProfile 1──* Lead
User 1──* RefreshToken
User 1──* SavedSupplier (buyer bookmarks)
SavedSupplier *──1 SupplierProfile
```

---

## Critical Business Rules

### Verification Rules (MVP)
- **Verified** if: at least 1 accepted certification uploaded AND not expired AND all required company fields complete
- **Unverified**: default state, no badge
- **Pending**: certification uploaded, awaiting validation
- **Flagged**: suspicious activity detected, admin must review

### ESG Level Rules
- **None**: incomplete profile
- **Bronze**: basic profile complete (all required fields filled)
- **Silver**: >= 1 valid certification + renewable_energy_percent >= 20
- **Gold**: >= 2 valid certifications + renewable_energy_percent >= 50 + carbon_reporting = true
- **Platinum**: >= 3 valid certifications + renewable_energy_percent >= 70 + waste_recycling_percent >= 70 + carbon_reporting = true

### Certification Expiry
- Daily job checks for certifications expiring within 30/14/7 days
- Expired certifications trigger re-scoring of verification status and ESG level
- Automated email reminders at each threshold

### Profile Publication
- Profile goes live automatically unless flagged
- Flagged profiles are hidden from search until admin resolves
- Unpublished profiles are only visible to the supplier themselves

### Lead Protection
- Rate limiting on lead submissions (prevent spam)
- Captcha on anonymous inquiries
- Leads stored with status tracking (new → contacted → closed)

---

## Testing Strategy

### Frameworks
- **Backend:** xUnit + FluentAssertions + Moq
- **Frontend:** Vitest + React Testing Library + Playwright (E2E)
- **API:** Integration tests against real Azure SQL (test database)

### Coverage Targets
- **Services layer:** 90%+ (ESG scoring, verification — business rules are critical)
- **Controllers:** 80%+ (integration tests)
- **Frontend components:** 70%+
- **E2E critical paths:** supplier search, profile view, lead submission

### What to Prioritise
1. ESG scoring rules — must be correct, explainable, and auditable
2. Certification expiry logic — wrong dates = wrong verification status
3. Search and filter accuracy — buyers must find the right suppliers
4. Lead delivery — a lost lead is lost revenue
5. Profile publication rules — flagged suppliers must not appear in search

---

## Brand / Design System

**Green Suppliers has its own brand identity — it does NOT use the Agilus red/black palette.**

### Colour Palette
- **Primary Green:** `#16A34A` (green-600) — primary actions, verification badges, trust signals
- **Dark Green:** `#166534` (green-800) — headers, primary text, premium feel
- **Light Green:** `#F0FDF4` (green-50) — backgrounds, card highlights, selected states
- **Accent Emerald:** `#059669` (emerald-600) — secondary actions, links, hover states
- **Earth Brown:** `#78716C` (stone-500) — supporting text, borders, subtle elements
- **White:** `#FFFFFF` — page backgrounds, cards, content areas
- **Dark:** `#1C1917` (stone-900) — body text, dark sections
- **Brand personality:** Trustworthy, clean, natural, professional — green signals sustainability and growth, earth tones ground it in authenticity

### Tailwind Theme Extension
```js
colors: {
  brand: {
    green: { DEFAULT: '#16A34A', dark: '#166534', light: '#F0FDF4', hover: '#15803D' },
    emerald: { DEFAULT: '#059669', hover: '#047857' },
    earth: { DEFAULT: '#78716C', light: '#D6D3D1', dark: '#44403C' },
    dark: { DEFAULT: '#1C1917' },
  }
}
```

### shadcn/ui CSS Variable Overrides
```css
:root {
  --primary: 142 71% 45%;             /* green-600 #16A34A */
  --primary-foreground: 0 0% 100%;
  --secondary: 143 64% 24%;           /* green-800 */
  --secondary-foreground: 0 0% 100%;
  --ring: 142 71% 45%;                /* green-600 focus ring */
  --accent: 160 84% 39%;              /* emerald-600 */
  --accent-foreground: 0 0% 100%;
}
```

### Design Principles
- Clean, airy layouts with plenty of white space
- Nature-inspired imagery and iconography
- Trust indicators prominent (verification badges, ESG levels, certification logos)
- Mobile-first responsive design
- Accessibility: WCAG 2.1 AA minimum

---

## Deployment Target

- **API hosting:** Azure App Service
- **Frontend hosting:** Vercel (Next.js)
- **Database:** Azure SQL Database Standard S0 (minimum for full-text search)
- **Storage:** Azure Blob Storage
- **Secrets:** Azure Key Vault
- **Monitoring:** Azure Application Insights
- **Domain:** greensuppliers.co.za (existing, currently pointing to WordPress — keep live until cutover)
- **CI/CD:** GitHub Actions — build, test, lint, type-check, coverage gate (80%)

---

## Phase Roadmap

### Phase 1 — Public Directory (COMPLETE)
- Supplier profiles (admin-seeded 15 suppliers)
- Public "Get Listed" intake form (simple submission, admin enters data)
- Search + filters (country, industry, certification, ESG level) with SQL full-text search
- Lead capture forms (anonymous allowed, registered buyers get extras)
- Verification + ESG scoring engine
- Background jobs (cert expiry scanner, nightly rescore, email dispatch via Resend)
- Content pages (pillar guides)
- Admin UI (supplier CRUD, taxonomy management, flags, content editor, dashboard)
- 30 API endpoints, 19 database tables

### Phase 2 — Self-Service + Monetisation (IN PROGRESS)

**Sprint 0 — Tech Debt (COMPLETE)**
- CancellationToken threading, N+1 query fixes, rate limiting, security headers, health checks

**Sprint 1 — Auth Foundation (COMPLETE)**
- Resend email integration (replaced console stub)
- Supplier + buyer self-registration with email verification
- Password reset flow with timing-safe token generation
- JWT refresh tokens with access/refresh token type separation

**Sprint 2 — Supplier Self-Service (COMPLETE)**
- Supplier profile editor (self-service, editable fields only)
- Certification submission (always Pending, admin reviews)
- Document upload (metadata, blob upload TODO)
- Profile publication request (completeness >= 50%)
- Supplier dashboard (leads, certs, ESG score, completeness)

**Sprint 3 — Buyer Accounts + Supplier Leads (COMPLETE)**
- Buyer saved suppliers (bookmark/unbookmark)
- Buyer inquiry history
- Buyer dashboard (saved count, inquiry stats)
- Supplier leads inbox (paginated, filterable by status)
- Lead status transitions (New -> Contacted -> Closed) with IDOR protection

**Sprint 4 — Subscription Billing (NEXT)**
- PayFast integration (subscription creation, ITN webhooks, payment recording)
- Plan enforcement (Free / Pro / Premium feature gating)
- Subscription management (upgrade, downgrade, cancel)
- Billing dashboard

**Sprint 5 — Supplier Dashboard Enhancements (PLANNED)**
- Enhanced analytics (view counts, search impressions)
- SDG tagging

**Sprint 6 — Sponsored Placements + Analytics (PLANNED)**
- Sponsored placement management
- Enhanced analytics display

### Phase 3 — Pan-African Expansion
- Multi-country content and supplier profiles
- Multi-currency billing
- RFQ workflow + success fees
- Buyer subscriptions
- API access for enterprise procurement systems

---

## References
- Global standards: see `C:\Users\SiviCovenden\CLAUDE.md` for API conventions, coding standards, and architecture rules
- Product spec: `Green Suppliers.docx` in project root
- Architecture decision: [ADR-0001](docs/decisions/0001-architecture-tier-and-pattern.md)
- Phase 2 tier decision: [ADR-0002](docs/decisions/0002-phase2-tier-decision.md)
- Milestone 1 design: [design-milestone-01.md](docs/decisions/design-milestone-01.md)
- Milestone 2 design: [design-milestone-02.md](docs/decisions/design-milestone-02.md)
- Architecture report (initial): [architecture-report-2026-03-24.md](docs/decisions/architecture-report-2026-03-24.md)
- Architecture report (post-Sprint 3): [architecture-report-2026-03-26.md](docs/decisions/architecture-report-2026-03-26.md)
- Existing site: www.greensuppliers.co.za (WordPress — to be replaced, back up before cutover)
