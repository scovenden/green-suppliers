# Architecture Report -- Green Suppliers -- 2026-03-24

## Complexity Assessment

| Dimension | Value | Score |
|-----------|-------|-------|
| Entities | 18 tables (14 Phase 1 + 4 Phase 2) | 2/4 |
| API endpoints | 28 Phase 1, ~45 total by Phase 2 | 2/4 |
| External integrations | 2 now (Blob Storage, email), 4 by Phase 2 (+PayFast, +Stripe) | 2/4 |
| Team size | 1-2 developers | 1/4 |
| Business rules | ESG scoring engine, verification state machine, cert expiry, lead protection -- beyond CRUD but bounded | 2/4 |
| **Total** | | **9/20** |

**Tier: 2 (MEDIUM) -- Feature-grouped service layer**

---

## Current Architecture vs Recommended

| | Current (CLAUDE.md) | Recommended |
|---|---|---|
| **Pattern** | Clean Architecture (4 layers, separate projects) | Service-layer monolith (single API project) |
| **Tier** | Tier 3 (Complex) | Tier 2 (Medium) |
| **Gap** | **Over-engineered** | |

The CLAUDE.md declares full Clean Architecture with Domain/Application/Infrastructure/API as separate layers. At a complexity score of 9/20 with 1-2 developers, this introduces unnecessary ceremony:
- Interfaces for every service (only needed where swappable: search)
- Separate Domain project with value objects and domain events (business rules fit in service classes)
- MediatR/CQRS patterns (not justified until team > 5 or write complexity increases)

**Recommendation:** Simplify to Tier 2 architecture per ADR-0001. The ESG scoring and verification logic justify a service layer, but not full Clean Architecture.

---

## Architecture Health

| Area | Status | Finding |
|------|--------|---------|
| Layer discipline | N/A | Greenfield -- no code exists yet. ADR-0001 defines the tier-appropriate structure. |
| API surface | DEFINED | 28 endpoints mapped for Phase 1. No gaps in auth or validation coverage. |
| DB schema quality | DEFINED | Full schema designed with: UUID PKs, soft delete, audit timestamps, filtered indexes, FTS preparation, check constraints on percentage fields, no floats for money. |
| CLAUDE.md accuracy | WARNING | Architecture section declares Clean Architecture but should be updated to Tier 2 per ADR-0001. Several sections need additions (see below). |
| ADR coverage | PARTIAL | ADR-0001 created. Need ADRs for: database choice, search strategy, auth approach. |
| Tech drift | N/A | No code exists to drift. |

---

## Critical Findings (fix before coding starts)

### 1. CLAUDE.md declares wrong architecture tier
**What:** The Architecture Pattern section says "Clean Architecture with strict layer boundaries" and shows 4-layer separation.
**Where:** CLAUDE.md lines 84-103
**Fix:** Update to reflect Tier 2 (service-layer monolith) per ADR-0001. Keep the upgrade triggers documented.

### 2. Five architectural decisions need resolution before coding
**What:** Open questions that affect project scaffold:
1. API hosting: Azure App Service vs Container Apps
2. Frontend hosting: Vercel vs Azure Static Web Apps
3. Email provider: SendGrid vs Azure Communication Services vs Resend
4. Admin auth approach in Phase 1
5. Azure SQL tier (need Standard S0+ for full-text search)

**Fix:** Make these decisions and record as ADRs before scaffolding.

---

## High Findings (fix this sprint)

### 1. No environment variables documented
**Where:** CLAUDE.md
**Fix:** Add a section listing all required env vars (connection strings, JWT secret, Blob Storage connection, email API key, etc.)

### 2. No folder structure documented
**Where:** CLAUDE.md
**Fix:** Add the actual folder structure from ADR-0001 (not the Clean Architecture aspirational structure).

### 3. No local development setup instructions
**Where:** CLAUDE.md
**Fix:** Add "How to run locally" section with database setup, migrations, seed data, and frontend dev server commands.

---

## Upgrade Triggers (monitor these)

Next tier upgrade (Tier 2 -> Tier 3 Clean Architecture) recommended when ANY of:
- [ ] Team grows past 4 active developers
- [ ] Entity count exceeds 25 tables
- [ ] Service classes regularly exceed 300 lines
- [ ] Circular dependencies appear between services
- [ ] Multiple bounded contexts emerge (e.g., billing becomes its own complex domain)
- [ ] Phase 3 pan-African expansion adds multi-currency, RFQ, and buyer subscriptions simultaneously

---

## CLAUDE.md Sections to Add/Update

Based on Tier 2 minimum requirements, the following sections need attention:

### Sections to UPDATE:
1. **Architecture Pattern** -- change from Clean Architecture to Tier 2 service-layer. Reference ADR-0001.

### Sections to ADD:
2. **Folder Structure** -- actual project structure (from ADR-0001), not aspirational
3. **Environment Variables** -- every env var documented with purpose and example
4. **How to Run Locally** -- step-by-step dev setup commands
5. **How to Deploy** -- deployment pipeline and target environments
6. **Layer Dependency Rules** -- what can import what (even in Tier 2, controllers must not contain business logic)
7. **Known Issues / Current Status** -- project phase tracking

### Sections that are COMPLETE and good:
- Product Overview
- ICP
- Tech Stack
- Key Domain Entities
- Critical Business Rules
- Testing Strategy
- Brand / Design System
- Deployment Target
- Phase Roadmap

---

## Actions Taken This Run

- **ADR-0001 created:** Architecture Tier and Pattern Selection (`docs/decisions/0001-architecture-tier-and-pattern.md`)
- **Design doc created:** Milestone 1 Technical Design with full DB schema, API surface map, service contracts, and test plan (`docs/decisions/design-milestone-01.md`)
- **Architecture report created:** This document (`docs/decisions/architecture-report-2026-03-24.md`)

## ADRs to Create (deferred -- need your input on open questions)

| ADR | Decision Needed |
|-----|----------------|
| ADR-0002 | Database and search strategy (Azure SQL tier, full-text setup, search abstraction) |
| ADR-0003 | Authentication approach for Phase 1 (seeded admin + JWT vs Entra ID) |
| ADR-0004 | Hosting decisions (App Service vs Container Apps, Vercel vs Azure SWA) |
| ADR-0005 | Email/notification provider |

---

## System Context Diagram

```
SYSTEM CONTEXT -- GREEN SUPPLIERS
==========================================================================

  [Buyer]                    [Supplier Admin]          [Platform Admin]
     |                            |                          |
     v                            v                          v
  +-----------------------------------------------------------------+
  |                     Next.js Frontend                             |
  |         (SSR public pages + admin dashboard)                     |
  +-----------------------------------------------------------------+
                              |
                              v
  +-----------------------------------------------------------------+
  |                   .NET API (single project)                      |
  |  Controllers/ -> Services/ -> DbContext                          |
  |  (Auth, Validation, ESG Scoring, Verification, Search, Leads)   |
  +-----------------------------------------------------------------+
        |              |              |              |
        v              v              v              v
  +-----------+  +-----------+  +-----------+  +-----------+
  | Azure SQL |  | Azure     |  | Email     |  | Azure     |
  | Database  |  | Blob      |  | Provider  |  | Key Vault |
  | (FTS)     |  | Storage   |  | (TBD)     |  | (secrets) |
  +-----------+  +-----------+  +-----------+  +-----------+

  +-----------------------------------------------------------------+
  |              .NET Worker Service (background jobs)               |
  |  CertExpiryScanner | NightlyRescore | EmailDispatch             |
  +-----------------------------------------------------------------+
        |              |
        v              v
  +-----------+  +-----------+
  | Azure SQL |  | Email     |
  | Database  |  | Provider  |
  +-----------+  +-----------+

==========================================================================
```

---

## Recommended Next Architect Run

**Trigger:** After Phase 1 Milestone 1 ships (public directory live), OR when Phase 2 design begins.

**Focus areas for next review:**
- Has the service layer stayed clean, or are services growing too large?
- Does adding self-service + billing push complexity to Tier 3?
- Are the upgrade triggers being approached?
