# ADR-0001: Architecture Tier and Pattern Selection

## Status
Accepted

## Date
2026-03-24

## Context
Green Suppliers is a new SaaS directory/marketplace for verified green suppliers, launching in South Africa with a pan-African data model. The project has been scored against the Agilus complexity framework:

| Dimension | Assessment | Score |
|-----------|-----------|-------|
| Entities | 14 total (9 Phase 1 + 5 Phase 2) | 2/4 |
| API endpoints | Estimated 30-50 across all phases | 2/4 |
| External integrations | 2 now (Blob, email), 4 by Phase 2 (+PayFast, Stripe) | 2/4 |
| Team size | 1-2 developers | 1/4 |
| Business rules | ESG scoring, verification state machine, cert expiry -- meaningful but bounded | 2/4 |
| **Total** | | **9/20** |

**Complexity Tier: 2 (MEDIUM)**

The initial CLAUDE.md specified full Clean Architecture (4-project .NET solution with Domain/Application/Infrastructure/API separation). At Tier 2 complexity, this is over-engineered for the current and near-term state of the project.

### Why Clean Architecture is premature here

1. **Team size is 1-2.** The ceremony of interfaces-for-everything, separate projects per layer, and MediatR/CQRS adds navigational overhead with no team-scaling benefit.
2. **Business rules are meaningful but bounded.** ESG scoring and verification are real domain logic, but they fit comfortably in a service class -- they do not require a rich domain model with aggregates, domain events, and value objects.
3. **14 entities is moderate.** There is no risk of a tangled domain graph requiring enforced layer isolation.
4. **Phase 1 is read-heavy.** The public directory is mostly search and display. The write-side (supplier self-service, billing) comes in Phase 2.

### Why not Tier 1 (flat/no service layer)?

The ESG scoring rules, verification state machine, and certification expiry logic are genuine business rules that do not belong in controllers. A service layer is justified.

## Decision

**Use Tier 2 (MEDIUM) architecture: feature-grouped service layer with EF Core direct.**

Concrete structure:

```
GreenSuppliers/
  src/
    GreenSuppliers.Api/           -- Single .NET project
      Controllers/                -- Thin controllers, delegate to services
      Services/                   -- Business logic (ESG scoring, verification, search, leads)
        ISupplierSearchService.cs -- Interface for search (abstracts SQL FTS now, Azure AI Search later)
        SupplierSearchService.cs
        EsgScoringService.cs
        VerificationService.cs
        CertificationExpiryService.cs
        LeadService.cs
      Models/
        Entities/                 -- EF Core entities (POCOs)
        DTOs/                     -- Request/response DTOs
        Enums/                    -- VerificationStatus, EsgLevel, LeadStatus, etc.
      Data/
        GreenSuppliersDbContext.cs
        Configurations/           -- EF Core entity type configurations
        Migrations/
      Middleware/                  -- Error handling, request logging
      Auth/                       -- JWT + API key auth
      BackgroundJobs/             -- IHostedService implementations (or separate Worker project)
      Validators/                 -- FluentValidation or manual validation
      Program.cs

    GreenSuppliers.Worker/        -- Separate .NET Worker Service project
      Jobs/
        CertExpiryScanner.cs
        NightlyRescore.cs
        SubscriptionSync.cs       -- Phase 2
        EmailDispatch.cs

  web/
    green-suppliers-web/          -- Next.js 14 App Router
      app/
        (public)/                 -- Public-facing pages (SSR)
          page.tsx                -- Homepage
          suppliers/
            page.tsx              -- Search results
            [slug]/page.tsx       -- Supplier profile
          industries/[slug]/page.tsx
          countries/[slug]/page.tsx
        (dashboard)/              -- Phase 2: authenticated supplier dashboard
        api/                      -- BFF proxy routes if needed
      components/
        ui/                       -- shadcn/ui components
        suppliers/                -- Supplier-specific components
        search/                   -- Search and filter components
        leads/                    -- Lead form components
        layout/                   -- Header, footer, navigation
      lib/
        api-client.ts             -- Typed API client
        types.ts                  -- Shared TypeScript types
        utils.ts
        validators.ts             -- Zod schemas

  docs/
    decisions/                    -- ADRs
  CLAUDE.md
```

### Key rules for this architecture

1. **Controllers are thin.** They validate input, call a service, return a response. No business logic in controllers.
2. **Services own business logic.** ESG scoring, verification state transitions, cert expiry -- all live in service classes.
3. **Services call DbContext directly.** No repository pattern. EF Core IS the repository/unit-of-work.
4. **Interfaces only where justified.** `ISupplierSearchService` gets an interface because we plan to swap implementations (SQL FTS now, Azure AI Search later). Other services do NOT need interfaces unless we need to mock them in tests.
5. **DTOs separate from entities.** API contracts (DTOs) are separate from EF Core entities, but they live in the same project.
6. **Worker is a separate project.** Background jobs run in a .NET Worker Service project, sharing the same Data/ and Services/ via a shared project or direct reference.

## Alternatives Considered

### Option A: Full Clean Architecture (4 projects)
- Pros: Maximum layer isolation, scales to large teams, aligns with global CLAUDE.md default
- Cons: Over-engineered for 1-2 devs and 14 entities; adds 3x the files for the same functionality; navigational overhead kills velocity at this scale
- Why rejected: Complexity score is 9/20 (Tier 2). Clean Architecture is recommended at 14+ (Tier 3).

### Option B: Flat / no service layer (Tier 1)
- Pros: Fastest to build, minimal ceremony
- Cons: ESG scoring and verification rules would end up in controllers, making them untestable and hard to maintain
- Why rejected: Business rules score is 2/4 -- enough to justify a service layer.

### Option C: CQRS with MediatR
- Pros: Clean separation of reads and writes
- Cons: Massive overhead for a 1-2 person team; Phase 1 is read-heavy with minimal writes; adds handler-per-endpoint ceremony
- Why rejected: Not justified until team > 5 or write complexity significantly increases.

## Consequences

### Positive
- Faster development velocity for a small team
- Business logic is testable (service classes) without over-abstraction
- Search abstraction (`ISupplierSearchService`) provides a clean upgrade path to Azure AI Search
- Easy to understand for any developer reading the codebase

### Negative / Trade-offs
- If the team grows to 5+ devs working simultaneously, the single-project structure may cause merge conflicts
- No enforced compile-time layer boundaries (discipline is by convention, not by project separation)

### Upgrade Path
Revisit this decision and consider upgrading to Tier 3 (Clean Architecture) when ANY of:
- Team grows past 4 developers
- Entity count exceeds 25
- Multiple bounded contexts emerge (e.g., billing becomes complex enough to warrant its own domain)
- Service classes exceed 300 lines regularly
- Circular dependencies appear between services

## References
- Global CLAUDE.md architecture conventions
- Agilus Architect Agent complexity scoring framework
