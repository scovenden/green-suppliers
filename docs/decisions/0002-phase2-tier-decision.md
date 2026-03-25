# ADR-0002: Phase 2 Complexity Tier Decision -- Remain at Tier 2

## Status
Accepted

## Date
2026-03-25

## Context

Phase 1 of Green Suppliers is complete and live. The project scored 9/20 in the initial complexity assessment (ADR-0001), placing it in Tier 2 (MEDIUM) with a service-layer monolith architecture.

Phase 2 adds significant functionality: supplier self-registration, email verification, profile wizard, subscription billing via PayFast, supplier dashboard, buyer accounts, sponsored placements, SDG tagging, and enhanced analytics.

The re-scored complexity after Phase 2:

| Dimension | Phase 1 | Phase 2 | Score |
|-----------|---------|---------|-------|
| Entities | 19 tables | ~27 tables (+8 new) | 2/4 |
| API endpoints | 30 | ~67 (+37 new) | 3/4 |
| External integrations | 2 (Blob, console email) | 4 (Blob, SendGrid, PayFast, Stripe later) | 2/4 |
| Team size | 1-2 devs | 1-2 devs | 1/4 |
| Business rules | ESG scoring, verification, cert expiry | + billing state machine, plan enforcement, analytics | 3/4 |
| **Total** | **9/20** | **11/20** | |

The entity count (27) triggers the "exceeds 25" upgrade threshold from ADR-0001. This requires an explicit decision.

## Decision

**Remain at Tier 2 (MEDIUM) architecture for Phase 2.**

At our current Tier 2 complexity (11/20), the service-layer monolith remains the appropriate level of sophistication because:

1. **Team size is still 1-2.** This is the strongest signal. Clean Architecture's layer isolation benefits multiple developers working in parallel. With 1-2 developers, the ceremony of interfaces-for-everything, separate projects per layer, and strict dependency inversion adds navigational overhead without proportional benefit.

2. **The entity count trigger (25+) is a soft trigger, not a hard gate.** The 27 tables include 4 reference/event tables (SdgGoals, PayFastWebhookEvents, ProfileViewEvents, SearchImpressionEvents) that are simple append-only or lookup tables. The core domain complexity has not materially increased -- it is still centered on SupplierProfile with supporting entities.

3. **No circular dependencies exist.** Services call each other in a clean dependency graph (SupplierService -> EsgScoringService, VerificationService, AuditService). Phase 2 adds new services (RegistrationService, SubscriptionService, PayFastService) that do not create cycles.

4. **Services remain under 300 lines.** The largest existing service (SupplierService) is ~380 lines including mapping code. Phase 2's largest projected services (SubscriptionService, SupplierSelfServiceService) are estimated at ~250 lines each.

5. **Billing is a distinct concern but not yet a bounded context.** PayFast integration is a single service with a webhook endpoint. It does not require its own domain model, aggregates, or event-driven architecture. If billing grows to include multiple providers, invoicing, tax calculation, and dunning, it would warrant a bounded context -- but that is Phase 3 scope.

## Alternatives Considered

### Option A: Upgrade to Tier 3 (Clean Architecture)
- Pros: Enforced layer boundaries via project separation, better for team scaling, clearer dependency rules
- Cons: Requires restructuring 108 existing source files into 4 projects (Domain, Application, Infrastructure, API), adding interfaces for all services, and configuring dependency injection across projects. Estimated 3-5 days of refactoring with zero feature value.
- Why rejected: Only 1 of 5 upgrade triggers is hit (entity count). The other 4 (team > 4, circular deps, 300+ line services, bounded contexts) are not.

### Option B: Partial Tier 3 (Extract Billing as Bounded Context)
- Pros: Isolates payment complexity from the core directory domain
- Cons: Premature. Phase 2 billing is PayFast-only with 3 tables and 1 webhook. The overhead of a separate project for 2 services is not justified.
- Why rejected: Billing is not yet complex enough to warrant isolation. Revisit if Stripe + multi-currency + invoicing are added in Phase 3.

## Consequences

### Positive
- Development velocity maintained for 1-2 developer team
- No refactoring cost -- jump straight into Phase 2 features
- New services follow the established pattern (service class -> DbContext direct)
- Consistent architecture across all features

### Negative / Trade-offs
- Entity count exceeds the documented upgrade trigger -- this decision consciously overrides it with rationale
- If team grows past 3 during Phase 2, the single-project structure will cause merge friction
- No compile-time enforcement of layer boundaries (discipline by convention)

### Mitigation
- Monitor the 5 upgrade triggers during Phase 2 implementation
- If any service exceeds 300 lines: extract into sub-services (e.g., split SubscriptionService into BillingService + PlanEnforcementService) rather than adding architectural layers
- If circular dependencies appear: resolve at the service level before considering Tier 3

### Upgrade Path
Revisit this decision and consider upgrading to Tier 3 when ANY of:
- Team grows past 4 active developers
- 3+ upgrade triggers are simultaneously active
- Phase 3 scope (multi-currency, Stripe, RFQ, buyer subscriptions) is confirmed and increases score to 14+
- Billing domain becomes complex enough to warrant its own bounded context

## References
- [ADR-0001: Architecture Tier and Pattern Selection](0001-architecture-tier-and-pattern.md)
- [Phase 2 Design: Milestone 2](design-milestone-02.md)
- [Architecture Report 2026-03-24](architecture-report-2026-03-24.md)
