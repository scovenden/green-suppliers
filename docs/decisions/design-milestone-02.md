# Technical Design: Milestone 2 -- Self-Service + Monetisation

**Date:** 2026-03-25
**Project Complexity Tier:** 2 (MEDIUM) -- upper boundary, see re-score below
**Prerequisite:** Phase 1 complete and live at www.greensuppliers.co.za

---

## Summary

Milestone 2 transforms Green Suppliers from an admin-seeded directory into a self-service marketplace. Suppliers register, verify their email, build their own profiles via a wizard, subscribe to paid plans (Free/Pro/Premium) via PayFast, and access a dashboard showing leads, analytics, and billing. Buyers get free accounts for saved suppliers and lead history. The platform earns revenue through subscriptions and sponsored placements. Real email delivery replaces the Phase 1 console stub.

---

## Phase 2 Complexity Re-Score

| Dimension | Phase 1 Value | Phase 2 Value | Phase 2 Score |
|-----------|--------------|--------------|---------------|
| Entities | 19 tables | ~27 tables (+8 new/modified) | 2/4 (just under 30) |
| API endpoints | 30 | ~60 (+30 new) | 3/4 |
| External integrations | 2 (Blob, console email) | 4 (Blob, SendGrid, PayFast, Stripe) | 2/4 |
| Team size | 1-2 | 1-2 | 1/4 |
| Business rules | ESG scoring, verification, cert expiry | + billing state machine, plan enforcement, sponsored scheduling, analytics aggregation | 3/4 |
| **Total** | **9/20** | **11/20** | |

**Verdict: Still Tier 2 (MEDIUM), but at the upper boundary (11/20 vs 9-13 range).**

The upgrade trigger "entity count exceeds 25" from ADR-0001 is being approached (27 tables projected). However, the other triggers (team > 4, circular dependencies, 300+ line services) are NOT hit. At 1-2 developers, upgrading to Clean Architecture would slow down Phase 2 delivery without proportional benefit.

**Recommendation:** Stay at Tier 2. Monitor service complexity during implementation. If SubscriptionService or PayFastService exceed 300 lines, extract into focused sub-services rather than adding architectural layers.

**Create ADR-0002** to record this decision (see Architectural Decisions section below).

---

## Implementation Order -- Dependency Graph

Features have hard dependencies. Building in the wrong order creates throwaway scaffolding or blocks parallel work. This is the critical path.

```
DEPENDENCY GRAPH
===============================================================================

Layer 0 -- Infrastructure (no feature dependencies)
  [8] Real email integration (SendGrid)
      Reason: EVERYTHING needs email -- registration, password reset, billing
              receipts, cert reminders. This is the foundation layer.
      Blocks: Features 1, 3, 7, and improves existing cert expiry reminders.

Layer 1 -- Auth Foundation (depends on Layer 0)
  [1] Supplier self-registration + email verification
      Depends on: [8] real email for verification emails
      Blocks: Features 2, 3, 4, 5, 6
  [7] Password reset flow
      Depends on: [8] real email for reset emails
      Blocks: nothing, but users need it immediately after registration exists

Layer 2 -- Profile + Accounts (depends on Layer 1)
  [2] Profile wizard (self-service)
      Depends on: [1] registered supplier account to own the profile
      Blocks: Features 3, 4 (no point billing for an incomplete profile)
  [9] Buyer accounts (free)
      Depends on: [1] registration infrastructure (shared auth code)
      Blocks: Feature 10 (analytics need buyer tracking)

Layer 3 -- Monetisation (depends on Layer 2)
  [3] Subscription billing (PayFast)
      Depends on: [1] registered org, [2] profile exists to subscribe for
      Blocks: Features 5 (sponsored placements need active subscription)
  [6] Structured SDG tagging
      Depends on: [2] profile wizard (SDG tags are a wizard step)
      No blockers -- can be built in parallel with [3]

Layer 4 -- Revenue Features (depends on Layer 3)
  [4] Supplier dashboard (leads, analytics, billing)
      Depends on: [1] auth, [2] profile, [3] billing data to display
  [5] Sponsored placements
      Depends on: [3] active subscription (Pro/Premium only), [4] dashboard to manage
  [10] Enhanced analytics (view counts, search analytics)
       Depends on: [9] buyer tracking, [4] dashboard to display

===============================================================================
```

### Recommended Build Sequence

| Sprint | Features | Why This Order |
|--------|----------|---------------|
| **2.1** | [8] Real email (SendGrid) | Foundation. Replace ConsoleEmailSender. Every subsequent feature needs it. |
| **2.2** | [1] Supplier registration + email verification, [7] Password reset | Auth foundation. Shared registration/reset infrastructure. |
| **2.3** | [2] Profile wizard, [9] Buyer accounts | Profile creation enables billing. Buyer accounts reuse registration. |
| **2.4** | [3] Subscription billing (PayFast), [6] SDG tagging | Monetisation. SDG tagging is independent, fits the same sprint. |
| **2.5** | [4] Supplier dashboard | Central hub. Requires billing + leads + profile data. |
| **2.6** | [5] Sponsored placements, [10] Enhanced analytics | Revenue optimization. Last because they depend on everything else. |

---

## Feature Designs

### Feature [8]: Real Email Integration (SendGrid)

**Sprint:** 2.1
**Rationale:** The `IEmailSender` interface and `EmailQueueItem` entity already exist. The `EmailDispatch` Worker job already processes the queue. This feature replaces `ConsoleEmailSender` with a real `SendGridEmailSender` implementation. Minimal code, maximum unblocking value.

#### Changes Required

**New files:**
- `src/GreenSuppliers.Worker/Services/SendGridEmailSender.cs` -- implements `IEmailSender`

**Modified files:**
- `src/GreenSuppliers.Worker/Program.cs` -- register `SendGridEmailSender` instead of `ConsoleEmailSender` (based on config)
- `src/GreenSuppliers.Worker/appsettings.json` -- add SendGrid config section

**New environment variables:**
- `SENDGRID_API_KEY` -- SendGrid API key
- `SENDGRID_FROM_EMAIL` -- sender email (e.g. noreply@greensuppliers.co.za)
- `SENDGRID_FROM_NAME` -- sender name (e.g. Green Suppliers)

**New package:**
- `SendGrid` NuGet package (Worker project only)

**Email templates to create (SendGrid dynamic templates):**
- `email_verification` -- Welcome + verify your email
- `password_reset` -- Password reset link
- `lead_notification` -- New lead received (to supplier)
- `get_listed_notification` -- New listing request (to admin)
- `cert_reminder` -- Certification expiring (30/14/7 days)
- `cert_expired` -- Certification has expired
- `subscription_welcome` -- Welcome to [Plan] plan
- `subscription_payment_receipt` -- Payment received
- `subscription_payment_failed` -- Payment failed, update card
- `subscription_cancelled` -- Subscription cancelled

**Architecture note:** Keep using the email queue pattern. All features write to EmailQueue; the Worker sends them. This decouples email delivery from request handling and provides retry on failure.

**No new API endpoints.** No new tables. No migration.

---

### Feature [1]: Supplier Self-Registration + Email Verification

**Sprint:** 2.2
**Depends on:** [8] Real email

#### New/Modified Entities

**New table: `EmailVerificationTokens`**
```sql
CREATE TABLE EmailVerificationTokens (
    Id              UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
    UserId          UNIQUEIDENTIFIER NOT NULL,
    Token           NVARCHAR(200)    NOT NULL,     -- secure random token
    ExpiresAt       DATETIME2        NOT NULL,
    UsedAt          DATETIME2        NULL,
    CreatedAt       DATETIME2        NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_EmailVerificationTokens PRIMARY KEY (Id),
    CONSTRAINT FK_EVT_User FOREIGN KEY (UserId) REFERENCES Users(Id),
    CONSTRAINT UQ_EVT_Token UNIQUE (Token)
);
CREATE INDEX IX_EVT_Token ON EmailVerificationTokens (Token) WHERE UsedAt IS NULL;
CREATE INDEX IX_EVT_UserId ON EmailVerificationTokens (UserId);
```

**New table: `PasswordResetTokens`** (shared with Feature [7])
```sql
CREATE TABLE PasswordResetTokens (
    Id              UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
    UserId          UNIQUEIDENTIFIER NOT NULL,
    Token           NVARCHAR(200)    NOT NULL,
    ExpiresAt       DATETIME2        NOT NULL,
    UsedAt          DATETIME2        NULL,
    CreatedAt       DATETIME2        NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_PasswordResetTokens PRIMARY KEY (Id),
    CONSTRAINT FK_PRT_User FOREIGN KEY (UserId) REFERENCES Users(Id),
    CONSTRAINT UQ_PRT_Token UNIQUE (Token)
);
CREATE INDEX IX_PRT_Token ON PasswordResetTokens (Token) WHERE UsedAt IS NULL;
```

**Modified entity: `User`**
- Add `EmailVerificationToken` navigation property (no schema change -- User already has `EmailVerified` field)

#### New API Endpoints

```
METHOD  PATH                                    AUTH     VALIDATED  NOTES
POST    /api/v1/auth/register                   No       Yes        Supplier self-registration
POST    /api/v1/auth/verify-email               No       Yes        Verify email with token
POST    /api/v1/auth/resend-verification        No       Yes        Resend verification email (rate limited)
```

#### New Service: `RegistrationService`

```csharp
public class RegistrationService
{
    // Register: create Organization + User + SupplierProfile (draft)
    // Send verification email via EmailQueue
    // Verify: mark user as verified, activate profile
    // Resend: generate new token, invalidate old, send email

    public async Task<RegisterResult> RegisterSupplierAsync(RegisterRequest request);
    public async Task<bool> VerifyEmailAsync(string token);
    public async Task<bool> ResendVerificationAsync(string email);
}
```

#### Registration Flow

```
1. POST /api/v1/auth/register
   Input: email, password, firstName, lastName, companyName, countryCode
   Actions:
     a. Validate (email unique, password strength, required fields)
     b. Create Organization (type: supplier)
     c. Create User (role: supplier_admin, emailVerified: false)
     d. Create SupplierProfile (isPublished: false, verificationStatus: unverified)
     e. Generate EmailVerificationToken (expires in 24 hours)
     f. Queue verification email
     g. Return success (do NOT return JWT -- must verify email first)

2. POST /api/v1/auth/verify-email
   Input: token
   Actions:
     a. Find token, validate not expired, not used
     b. Mark token as used
     c. Set user.EmailVerified = true
     d. Return JWT tokens (user is now logged in)

3. POST /api/v1/auth/resend-verification
   Input: email
   Actions:
     a. Find user by email (must not already be verified)
     b. Rate limit: max 3 per hour per email
     c. Invalidate existing tokens for this user
     d. Generate new token
     e. Queue verification email
```

#### Business Rules
- Password minimum 8 characters, 1 uppercase, 1 number
- Email must be unique (case-insensitive)
- Verification token expires in 24 hours
- Unverified accounts cannot log in (except to resend verification)
- Unverified profiles are NOT visible in search (isPublished = false)
- After verification, profile remains unpublished until wizard is completed

#### Validators
- `RegisterRequestValidator` (FluentValidation)

#### Frontend Pages
- `/register` -- Registration form
- `/verify-email?token=xxx` -- Email verification landing page
- `/register/check-email` -- "Check your email" confirmation page

---

### Feature [7]: Password Reset Flow

**Sprint:** 2.2
**Depends on:** [8] Real email

#### API Endpoints

```
METHOD  PATH                                    AUTH     VALIDATED  NOTES
POST    /api/v1/auth/forgot-password            No       Yes        Request password reset
POST    /api/v1/auth/reset-password             No       Yes        Reset with token + new password
```

#### Service: Add to `RegistrationService` (same auth service)

```csharp
public async Task<bool> RequestPasswordResetAsync(string email);
public async Task<bool> ResetPasswordAsync(string token, string newPassword);
```

#### Flow

```
1. POST /api/v1/auth/forgot-password
   Input: email
   Actions:
     a. Find user by email (silently succeed even if not found -- prevent enumeration)
     b. Rate limit: max 3 per hour per email
     c. Generate PasswordResetToken (expires in 1 hour)
     d. Queue reset email with link
     e. Always return 200 (prevent email enumeration)

2. POST /api/v1/auth/reset-password
   Input: token, newPassword
   Actions:
     a. Find token, validate not expired, not used
     b. Mark token as used
     c. Hash new password, update user
     d. Invalidate all existing refresh tokens (log out everywhere)
     e. Return success
```

#### Business Rules
- Reset token expires in 1 hour (shorter than email verification)
- Always return success on forgot-password (prevent email enumeration)
- Password reset invalidates all sessions (security)
- Rate limited to prevent abuse

#### Frontend Pages
- `/forgot-password` -- Email input form
- `/reset-password?token=xxx` -- New password form
- `/forgot-password/check-email` -- Confirmation page

---

### Feature [2]: Profile Wizard (Self-Service)

**Sprint:** 2.3
**Depends on:** [1] Registration

The profile wizard lets newly registered suppliers complete their profile in steps. The profile was created in draft state during registration; the wizard populates it.

#### Wizard Steps

```
Step 1: Company Details
  - Trading name, description, short description
  - Year founded, employee count range
  - BEE level (SA-specific, optional)
  - Logo upload, banner upload

Step 2: Location
  - Country (pre-filled from registration)
  - City, province
  - Website (pre-filled from org)

Step 3: Industries + Services
  - Select industries (multi-select from taxonomy)
  - Add service tags (multi-select + freeform)

Step 4: Sustainability
  - Renewable energy percentage
  - Waste recycling percentage
  - Carbon reporting (yes/no)
  - Water management (yes/no)
  - Sustainable packaging (yes/no)
  - SDG tags (see Feature [6])

Step 5: Certifications
  - Upload certifications (file + type + number + dates)
  - Each cert starts as 'pending' until admin review

Step 6: Review + Publish
  - Preview profile as buyers will see it
  - Click "Publish" to go live (isPublished = true)
  - ESG scoring runs on publish
```

#### New API Endpoints

```
METHOD  PATH                                        AUTH         VALIDATED  NOTES
GET     /api/v1/supplier/profile                    Supplier     No         Get own profile (for wizard)
PUT     /api/v1/supplier/profile                    Supplier     Yes        Update own profile
PUT     /api/v1/supplier/profile/step/{stepNumber}  Supplier     Yes        Save individual wizard step
POST    /api/v1/supplier/profile/publish             Supplier     No         Publish profile
POST    /api/v1/supplier/certifications             Supplier     Yes        Upload a certification
DELETE  /api/v1/supplier/certifications/{id}        Supplier     No         Remove own cert
POST    /api/v1/supplier/documents/upload           Supplier     Yes        Upload logo/banner/doc
DELETE  /api/v1/supplier/documents/{id}             Supplier     No         Remove own document
GET     /api/v1/supplier/profile/preview            Supplier     No         Preview as public
```

#### New Service: `SupplierSelfServiceService`

Separate from the admin `SupplierService`. Self-service has different authorization rules (can only edit own profile) and different validation (admin can set any field; suppliers have restricted fields like verificationStatus).

```csharp
public class SupplierSelfServiceService
{
    // Profile CRUD scoped to the authenticated supplier's organization
    public async Task<SupplierProfileDto?> GetOwnProfileAsync(Guid organizationId);
    public async Task<SupplierProfileDto> UpdateProfileAsync(Guid organizationId, UpdateOwnProfileRequest request);
    public async Task<SupplierProfileDto> SaveWizardStepAsync(Guid organizationId, int step, JsonElement data);
    public async Task<bool> PublishProfileAsync(Guid organizationId);
    public async Task<SupplierCertificationDto> AddCertificationAsync(Guid organizationId, AddCertificationRequest request);
    public async Task<bool> RemoveCertificationAsync(Guid organizationId, Guid certId);
}
```

#### Modified Entity: `SupplierProfile`

Add field to track wizard completion:
```sql
ALTER TABLE SupplierProfiles ADD WizardCompleted BIT NOT NULL DEFAULT 0;
ALTER TABLE SupplierProfiles ADD WizardStep INT NOT NULL DEFAULT 0;
```

#### Business Rules
- Suppliers can only edit their OWN profile (scoped by OrganizationId from JWT claims)
- Profile cannot be published until WizardCompleted = true (all required fields filled)
- Publishing triggers ESG scoring + verification evaluation
- Certifications uploaded by suppliers start as 'pending' (admin must accept)
- Logo/banner uploads go to Azure Blob Storage (existing DocumentService)
- Admin can still edit any profile via the existing admin endpoints

#### Frontend Pages
- `/dashboard/profile/wizard` -- Multi-step wizard
- `/dashboard/profile/preview` -- Preview mode

---

### Feature [9]: Buyer Accounts (Free)

**Sprint:** 2.3
**Depends on:** [1] Registration infrastructure

Buyers get free accounts to save suppliers and track their lead history. This reuses the registration infrastructure.

#### New/Modified Entities

**New table: `SavedSuppliers`**
```sql
CREATE TABLE SavedSuppliers (
    Id                  UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
    BuyerUserId         UNIQUEIDENTIFIER NOT NULL,
    SupplierProfileId   UNIQUEIDENTIFIER NOT NULL,
    Notes               NVARCHAR(500)    NULL,
    CreatedAt           DATETIME2        NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_SavedSuppliers PRIMARY KEY (Id),
    CONSTRAINT FK_SS_Buyer FOREIGN KEY (BuyerUserId) REFERENCES Users(Id),
    CONSTRAINT FK_SS_Supplier FOREIGN KEY (SupplierProfileId) REFERENCES SupplierProfiles(Id),
    CONSTRAINT UQ_SavedSupplier UNIQUE (BuyerUserId, SupplierProfileId)
);
CREATE INDEX IX_SS_BuyerUserId ON SavedSuppliers (BuyerUserId);
```

**Modified entity: `Lead`**
- Leads now link to BuyerUserId when submitted by authenticated buyers (already supported -- BuyerUserId column exists)

#### New API Endpoints

```
METHOD  PATH                                    AUTH     VALIDATED  NOTES
POST    /api/v1/auth/register-buyer             No       Yes        Buyer registration (simpler than supplier)
GET     /api/v1/buyer/saved-suppliers            Buyer    Yes        List saved suppliers
POST    /api/v1/buyer/saved-suppliers            Buyer    Yes        Save a supplier
DELETE  /api/v1/buyer/saved-suppliers/{id}       Buyer    No         Unsave a supplier
GET     /api/v1/buyer/leads                      Buyer    Yes        View own lead history
```

#### New Service: `BuyerService`

```csharp
public class BuyerService
{
    public async Task<PagedResult<SupplierSummaryDto>> GetSavedSuppliersAsync(Guid userId, int page, int pageSize);
    public async Task<SavedSupplierDto> SaveSupplierAsync(Guid userId, Guid supplierProfileId, string? notes);
    public async Task<bool> UnsaveSupplierAsync(Guid userId, Guid savedSupplierId);
    public async Task<PagedResult<LeadDto>> GetOwnLeadsAsync(Guid userId, int page, int pageSize);
}
```

#### Registration Flow (Buyer)

Same infrastructure as supplier registration, but:
- Creates Organization (type: buyer) -- or optionally just a User if they provide a company name
- Creates User (role: buyer)
- No SupplierProfile created
- No wizard needed -- buyer can start using immediately after email verification

#### Frontend Pages
- `/register/buyer` -- Buyer registration
- `/buyer/saved` -- Saved suppliers list
- `/buyer/leads` -- Lead history

---

### Feature [3]: Subscription Billing (PayFast)

**Sprint:** 2.4
**Depends on:** [1] Registration, [2] Profile wizard

#### Plan Definitions

```
FREE PLAN (default)
  - Basic profile listing
  - Up to 5 leads/month
  - No analytics
  - No featured listing
  - No document uploads
  - Price: R0

PRO PLAN
  - Enhanced profile (priority in search)
  - Up to 50 leads/month
  - Basic analytics (views, lead conversion)
  - 10 document uploads
  - Price: R499/month or R4,999/year (save 17%)

PREMIUM PLAN
  - Featured profile (top of search, badge)
  - Unlimited leads
  - Full analytics
  - Unlimited documents
  - Sponsored placement eligibility
  - Price: R999/month or R9,999/year (save 17%)
```

#### Modified Entities

The `Plans`, `Subscriptions`, and `Payments` tables already exist in the database. Minor modifications needed:

**Modified table: `Subscriptions`**
```sql
-- Add fields for PayFast integration
ALTER TABLE Subscriptions ADD PayFastToken NVARCHAR(200) NULL;  -- PayFast subscription token
ALTER TABLE Subscriptions ADD TrialEndsAt DATETIME2 NULL;       -- Optional trial period
ALTER TABLE Subscriptions ADD GracePeriodEndsAt DATETIME2 NULL; -- Grace period after failed payment
```

**Modified table: `Payments`**
```sql
-- Add invoice reference
ALTER TABLE Payments ADD InvoiceNumber NVARCHAR(50) NULL;
ALTER TABLE Payments ADD Description NVARCHAR(200) NULL;
```

**New table: `PayFastWebhookEvents`** (idempotency + audit trail)
```sql
CREATE TABLE PayFastWebhookEvents (
    Id              UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
    PayFastPaymentId NVARCHAR(200)   NOT NULL,
    EventType       NVARCHAR(50)     NOT NULL,  -- COMPLETE, CANCEL, etc.
    RawPayload      NVARCHAR(MAX)    NOT NULL,
    Processed       BIT              NOT NULL DEFAULT 0,
    ProcessedAt     DATETIME2        NULL,
    ErrorMessage    NVARCHAR(2000)   NULL,
    CreatedAt       DATETIME2        NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_PayFastWebhookEvents PRIMARY KEY (Id)
);
CREATE INDEX IX_PFWE_PaymentId ON PayFastWebhookEvents (PayFastPaymentId);
CREATE INDEX IX_PFWE_Processed ON PayFastWebhookEvents (Processed) WHERE Processed = 0;
```

#### New API Endpoints

```
METHOD  PATH                                        AUTH         VALIDATED  NOTES
GET     /api/v1/plans                               No           No         List available plans
GET     /api/v1/supplier/subscription                Supplier     No         Get own subscription
POST    /api/v1/supplier/subscription/checkout       Supplier     Yes        Initiate PayFast checkout
POST    /api/v1/supplier/subscription/cancel         Supplier     Yes        Cancel subscription
GET     /api/v1/supplier/billing/invoices            Supplier     Yes        List payment history
POST    /api/v1/webhooks/payfast                     No*          Yes        PayFast ITN callback (*signature validated)
```

*The PayFast webhook endpoint has no JWT auth but validates the PayFast signature and source IP.*

#### New Services

**`SubscriptionService`**
```csharp
public class SubscriptionService
{
    // Core billing logic
    public async Task<SubscriptionDto?> GetCurrentAsync(Guid organizationId);
    public async Task<CheckoutResult> InitiateCheckoutAsync(Guid organizationId, CheckoutRequest request);
    public async Task<bool> CancelSubscriptionAsync(Guid organizationId);
    public async Task<bool> CanAccessFeatureAsync(Guid organizationId, string feature);
    public async Task<PagedResult<PaymentDto>> GetPaymentHistoryAsync(Guid organizationId, int page, int pageSize);

    // Plan enforcement
    public async Task<int> GetRemainingLeadsAsync(Guid organizationId);
    public async Task<bool> IncrementLeadCountAsync(Guid organizationId);
}
```

**`PayFastService`**
```csharp
public class PayFastService
{
    // PayFast integration
    public PayFastCheckoutData BuildCheckoutData(Plan plan, Organization org, string billingCycle);
    public bool ValidateItnSignature(Dictionary<string, string> payload, string passphraseHash);
    public async Task ProcessItnAsync(Dictionary<string, string> payload);
}
```

#### PayFast Integration Flow

```
CHECKOUT FLOW:
1. Supplier clicks "Upgrade to Pro" in dashboard
2. Frontend calls POST /api/v1/supplier/subscription/checkout { planId, billingCycle }
3. API creates pending Subscription + returns PayFast redirect URL with signature
4. Supplier redirected to PayFast hosted payment page
5. On success: PayFast redirects to /dashboard/billing?status=success
6. PayFast sends ITN (Instant Transaction Notification) to /api/v1/webhooks/payfast
7. API validates ITN signature, updates Subscription status to 'active', creates Payment record
8. Queue confirmation email

RECURRING PAYMENT FLOW:
1. PayFast sends ITN for each recurring charge
2. API creates Payment record, extends Subscription period
3. Queue receipt email

FAILED PAYMENT FLOW:
1. PayFast sends ITN with failure status
2. API marks Subscription as 'past_due', sets GracePeriodEndsAt (7 days)
3. Queue "payment failed" email
4. Worker job: after grace period expires, downgrade to Free plan

CANCELLATION FLOW:
1. Supplier clicks "Cancel" in dashboard
2. API calls PayFast cancel subscription API
3. Subscription stays active until CurrentPeriodEnd
4. At period end, downgrade to Free plan
```

#### Business Rules
- Every new supplier starts on the Free plan (auto-created during registration)
- Plan changes take effect immediately for upgrades, at period end for downgrades
- Lead limits are enforced per calendar month (reset on 1st of each month)
- Existing leads are not deleted on downgrade -- only new leads are gated
- PayFast webhook must be idempotent (PayFastWebhookEvents table for dedup)
- All money stored as `DECIMAL(10,2)` with currency code (already in schema)
- No Stripe in Phase 2 -- PayFast only (SA market). Stripe deferred to Phase 3 for international.

#### Plan Enforcement Points

These are the places in existing code that must check the supplier's plan:

| Feature | Free | Pro | Premium |
|---------|------|-----|---------|
| Profile listing in search | Yes | Priority sort boost | Featured badge + top placement |
| Leads received per month | 5 | 50 | Unlimited |
| Document uploads | 0 | 10 | Unlimited |
| Analytics access | No | Basic | Full |
| Sponsored placements | No | No | Eligible |

#### Worker Job: `SubscriptionSync`

Add to `GreenSuppliers.Worker`:
```
New job: SubscriptionSync (runs hourly)
- Check for past_due subscriptions past grace period -> downgrade to Free
- Check for cancelled subscriptions past CurrentPeriodEnd -> downgrade to Free
- Check for trial periods that have expired -> convert to paid or downgrade
```

#### Frontend Pages
- `/pricing` -- Public pricing page
- `/dashboard/billing` -- Subscription management
- `/dashboard/billing/checkout` -- PayFast redirect handler
- `/dashboard/billing/invoices` -- Payment history

---

### Feature [6]: Structured SDG Tagging

**Sprint:** 2.4
**Depends on:** [2] Profile wizard

#### New Entities

**New table: `SdgGoals`** (reference data -- seeded)
```sql
CREATE TABLE SdgGoals (
    Id          INT             NOT NULL,           -- SDG 1-17
    Name        NVARCHAR(200)   NOT NULL,           -- e.g. "No Poverty"
    Description NVARCHAR(500)   NULL,
    IconUrl     NVARCHAR(500)   NULL,               -- UN SDG icon
    IsActive    BIT             NOT NULL DEFAULT 1,
    CONSTRAINT PK_SdgGoals PRIMARY KEY (Id)
);
```

**New table: `SupplierSdgGoals`** (many-to-many)
```sql
CREATE TABLE SupplierSdgGoals (
    SupplierProfileId UNIQUEIDENTIFIER NOT NULL,
    SdgGoalId         INT              NOT NULL,
    Description       NVARCHAR(500)    NULL,       -- Supplier's description of how they contribute
    CONSTRAINT PK_SupplierSdgGoals PRIMARY KEY (SupplierProfileId, SdgGoalId),
    CONSTRAINT FK_SSG_Supplier FOREIGN KEY (SupplierProfileId) REFERENCES SupplierProfiles(Id),
    CONSTRAINT FK_SSG_Sdg FOREIGN KEY (SdgGoalId) REFERENCES SdgGoals(Id)
);
```

#### New API Endpoints

```
METHOD  PATH                                    AUTH         VALIDATED  NOTES
GET     /api/v1/sdg-goals                       No           No         List all 17 SDGs
PUT     /api/v1/supplier/profile/sdg-goals      Supplier     Yes        Set SDG goals for own profile
GET     /api/v1/suppliers?sdgGoalId=7            No           Yes        Filter by SDG goal (add to existing search)
```

#### Changes to Existing Code
- `SupplierSearchQuery` -- add `SdgGoalId` filter parameter
- `SqlFullTextSearchService` -- add SDG join to search query
- `SupplierProfileDto` -- add `SdgGoals` collection
- `SupplierService.MapToDto` -- include SDG goals in mapping
- Profile wizard Step 4 (Sustainability) -- add SDG goal selector

#### Seed Data
Seed all 17 UN SDGs with official names and icon URLs.

---

### Feature [4]: Supplier Dashboard

**Sprint:** 2.5
**Depends on:** [1], [2], [3]

The supplier dashboard is the central authenticated area for suppliers.

#### New API Endpoints

```
METHOD  PATH                                        AUTH         VALIDATED  NOTES
GET     /api/v1/supplier/dashboard/stats             Supplier     No         Summary stats
GET     /api/v1/supplier/leads                       Supplier     Yes        View leads for own profile
PATCH   /api/v1/supplier/leads/{id}/status           Supplier     Yes        Update lead status (own only)
GET     /api/v1/supplier/analytics/views             Supplier     Yes        Profile view counts (Pro+)
GET     /api/v1/supplier/analytics/search-appearances Supplier    Yes        Search appearance count (Pro+)
GET     /api/v1/supplier/analytics/lead-conversion   Supplier     Yes        Lead metrics (Premium)
```

#### New Service: `SupplierDashboardService`

```csharp
public class SupplierDashboardService
{
    public async Task<DashboardStatsDto> GetStatsAsync(Guid organizationId);
    public async Task<PagedResult<LeadDto>> GetLeadsAsync(Guid organizationId, int page, int pageSize, string? status);
    public async Task<bool> UpdateLeadStatusAsync(Guid organizationId, Guid leadId, string status);
    public async Task<AnalyticsDto> GetAnalyticsAsync(Guid organizationId, DateRange range);
}
```

#### Dashboard Stats DTO
```
{
  profileViews: 142,          // last 30 days
  searchAppearances: 583,     // last 30 days
  totalLeads: 12,             // all time
  newLeads: 3,                // unread
  esgLevel: "Gold",
  verificationStatus: "Verified",
  planName: "Pro",
  planExpiresAt: "2026-04-24T00:00:00Z",
  leadsUsedThisMonth: 8,
  leadsLimitThisMonth: 50
}
```

#### Business Rules
- Suppliers can only see leads for their own profile (scoped by OrganizationId)
- Analytics data gated by plan (Free: no analytics, Pro: basic, Premium: full)
- Lead status updates scoped to own leads only

#### Frontend Pages
- `/dashboard` -- Dashboard home (stats overview)
- `/dashboard/leads` -- Lead management table
- `/dashboard/analytics` -- Analytics charts (plan-gated)
- `/dashboard/profile` -- Profile editor (post-wizard)
- `/dashboard/billing` -- Subscription + payments (from Feature [3])

---

### Feature [5]: Sponsored Placements

**Sprint:** 2.6
**Depends on:** [3], [4]

#### Modified Entity: `SponsoredPlacements` (already exists)

Add fields:
```sql
ALTER TABLE SponsoredPlacements ADD PaidAmount DECIMAL(10,2) NULL;
ALTER TABLE SponsoredPlacements ADD Currency CHAR(3) NOT NULL DEFAULT 'ZAR';
ALTER TABLE SponsoredPlacements ADD Status NVARCHAR(20) NOT NULL DEFAULT 'pending';
  -- pending | active | expired | cancelled
```

#### New API Endpoints

```
METHOD  PATH                                            AUTH         VALIDATED  NOTES
GET     /api/v1/supplier/placements                     Supplier     Yes        List own placements
POST    /api/v1/supplier/placements                     Supplier     Yes        Request a new placement
GET     /api/v1/admin/placements                        Admin        Yes        List all placements
PATCH   /api/v1/admin/placements/{id}                   Admin        Yes        Approve/reject/modify
GET     /api/v1/suppliers/featured                      No           No         Get featured suppliers for homepage
POST    /api/v1/suppliers/{slug}/impression              No           No         Track impression (fire-and-forget)
POST    /api/v1/suppliers/{slug}/click                   No           No         Track click (fire-and-forget)
```

#### New Service: `SponsoredPlacementService`

```csharp
public class SponsoredPlacementService
{
    public async Task<List<SponsoredPlacementDto>> GetActiveAsync(string placementType);
    public async Task<SponsoredPlacementDto> RequestPlacementAsync(Guid organizationId, PlacementRequest request);
    public async Task RecordImpressionAsync(Guid supplierProfileId, string placementType);
    public async Task RecordClickAsync(Guid supplierProfileId, string placementType);
}
```

#### Business Rules
- Only Premium subscribers can request sponsored placements
- Placements require admin approval (manual pricing in Phase 2)
- Impressions and clicks tracked for ROI reporting
- Active placements boost search ranking (separate from ESG score)
- Homepage shows max 6 featured suppliers (round-robin if more active)
- Fire-and-forget tracking (don't slow page loads)

---

### Feature [10]: Enhanced Analytics

**Sprint:** 2.6
**Depends on:** [4], [9]

#### New Entities

**New table: `ProfileViewEvents`**
```sql
CREATE TABLE ProfileViewEvents (
    Id                  UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
    SupplierProfileId   UNIQUEIDENTIFIER NOT NULL,
    ViewerUserId        UNIQUEIDENTIFIER NULL,          -- NULL if anonymous
    ViewerIpAddress     NVARCHAR(45)     NULL,
    Referrer            NVARCHAR(500)    NULL,
    CreatedAt           DATETIME2        NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_ProfileViewEvents PRIMARY KEY (Id),
    CONSTRAINT FK_PVE_Supplier FOREIGN KEY (SupplierProfileId) REFERENCES SupplierProfiles(Id)
);
CREATE INDEX IX_PVE_Supplier_Date ON ProfileViewEvents (SupplierProfileId, CreatedAt DESC);
```

**New table: `SearchImpressionEvents`**
```sql
CREATE TABLE SearchImpressionEvents (
    Id                  UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
    SupplierProfileId   UNIQUEIDENTIFIER NOT NULL,
    SearchQuery         NVARCHAR(500)    NULL,
    Position            INT              NULL,           -- rank position in results
    CreatedAt           DATETIME2        NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_SIE PRIMARY KEY (Id),
    CONSTRAINT FK_SIE_Supplier FOREIGN KEY (SupplierProfileId) REFERENCES SupplierProfiles(Id)
);
CREATE INDEX IX_SIE_Supplier_Date ON SearchImpressionEvents (SupplierProfileId, CreatedAt DESC);
```

#### New API Endpoints

(Already defined in Feature [4] dashboard -- the analytics endpoints.)

Additional public-side tracking:
```
METHOD  PATH                                    AUTH     VALIDATED  NOTES
POST    /api/v1/tracking/profile-view           No       Yes        Track profile view (fire-and-forget)
POST    /api/v1/tracking/search-impression      No       Yes        Track search appearance (batch)
```

#### New Service: `AnalyticsService`

```csharp
public class AnalyticsService
{
    // Write side (fire-and-forget, used by public pages)
    public Task RecordProfileViewAsync(Guid supplierProfileId, Guid? viewerUserId, string? ipAddress, string? referrer);
    public Task RecordSearchImpressionsAsync(List<Guid> supplierProfileIds, string? searchQuery);

    // Read side (used by supplier dashboard)
    public Task<ProfileViewAnalytics> GetProfileViewsAsync(Guid supplierProfileId, DateRange range);
    public Task<SearchAnalytics> GetSearchAppearancesAsync(Guid supplierProfileId, DateRange range);
    public Task<LeadConversionAnalytics> GetLeadConversionAsync(Guid supplierProfileId, DateRange range);
}
```

#### Business Rules
- Tracking endpoints are fire-and-forget (return 202 Accepted immediately, process async)
- Deduplicate profile views per IP+supplier per hour (prevent refresh spam)
- Analytics data visible based on plan tier (see Feature [3] plan enforcement)
- Analytics tables are append-only, high-write -- consider table partitioning if volume warrants

#### Worker Job: `AnalyticsAggregation` (future optimization)

Not needed for Phase 2 launch. If analytics queries become slow, add a nightly aggregation job that pre-computes daily rollups. Document this as a known optimization path.

---

## Complete New/Modified Table Summary

| Table | Change | Migration | Sprint |
|-------|--------|-----------|--------|
| EmailVerificationTokens | NEW | Yes | 2.2 |
| PasswordResetTokens | NEW | Yes | 2.2 |
| SupplierProfiles | ADD WizardCompleted, WizardStep | Yes | 2.3 |
| SavedSuppliers | NEW | Yes | 2.3 |
| Subscriptions | ADD PayFastToken, TrialEndsAt, GracePeriodEndsAt | Yes | 2.4 |
| Payments | ADD InvoiceNumber, Description | Yes | 2.4 |
| PayFastWebhookEvents | NEW | Yes | 2.4 |
| SdgGoals | NEW (seeded) | Yes | 2.4 |
| SupplierSdgGoals | NEW | Yes | 2.4 |
| SponsoredPlacements | ADD PaidAmount, Currency, Status | Yes | 2.6 |
| ProfileViewEvents | NEW | Yes | 2.6 |
| SearchImpressionEvents | NEW | Yes | 2.6 |

**Total after Phase 2: ~27 tables** (19 existing + 8 new)

---

## Complete New API Endpoint Summary

```
API SURFACE MAP -- PHASE 2 NEW ENDPOINTS
============================================================================
METHOD  PATH                                            AUTH         SPRINT
----------------------------------------------------------------------------
AUTH (public)
POST    /api/v1/auth/register                           No           2.2
POST    /api/v1/auth/verify-email                       No           2.2
POST    /api/v1/auth/resend-verification                No           2.2
POST    /api/v1/auth/register-buyer                     No           2.3
POST    /api/v1/auth/forgot-password                    No           2.2
POST    /api/v1/auth/reset-password                     No           2.2

SUPPLIER SELF-SERVICE (supplier_admin or supplier_user role)
GET     /api/v1/supplier/profile                        Supplier     2.3
PUT     /api/v1/supplier/profile                        Supplier     2.3
PUT     /api/v1/supplier/profile/step/{stepNumber}      Supplier     2.3
POST    /api/v1/supplier/profile/publish                Supplier     2.3
GET     /api/v1/supplier/profile/preview                Supplier     2.3
POST    /api/v1/supplier/certifications                 Supplier     2.3
DELETE  /api/v1/supplier/certifications/{id}            Supplier     2.3
POST    /api/v1/supplier/documents/upload               Supplier     2.3
DELETE  /api/v1/supplier/documents/{id}                 Supplier     2.3
PUT     /api/v1/supplier/profile/sdg-goals              Supplier     2.4
GET     /api/v1/supplier/subscription                   Supplier     2.4
POST    /api/v1/supplier/subscription/checkout          Supplier     2.4
POST    /api/v1/supplier/subscription/cancel            Supplier     2.4
GET     /api/v1/supplier/billing/invoices               Supplier     2.4
GET     /api/v1/supplier/dashboard/stats                Supplier     2.5
GET     /api/v1/supplier/leads                          Supplier     2.5
PATCH   /api/v1/supplier/leads/{id}/status              Supplier     2.5
GET     /api/v1/supplier/analytics/views                Supplier     2.5
GET     /api/v1/supplier/analytics/search-appearances   Supplier     2.5
GET     /api/v1/supplier/analytics/lead-conversion      Supplier     2.5
GET     /api/v1/supplier/placements                     Supplier     2.6
POST    /api/v1/supplier/placements                     Supplier     2.6

BUYER (buyer role)
GET     /api/v1/buyer/saved-suppliers                   Buyer        2.3
POST    /api/v1/buyer/saved-suppliers                   Buyer        2.3
DELETE  /api/v1/buyer/saved-suppliers/{id}              Buyer        2.3
GET     /api/v1/buyer/leads                             Buyer        2.3

PUBLIC (additions)
GET     /api/v1/plans                                   No           2.4
GET     /api/v1/sdg-goals                               No           2.4
GET     /api/v1/suppliers/featured                      No           2.6
POST    /api/v1/suppliers/{slug}/impression              No           2.6
POST    /api/v1/suppliers/{slug}/click                   No           2.6
POST    /api/v1/tracking/profile-view                   No           2.6
POST    /api/v1/tracking/search-impression              No           2.6

WEBHOOKS (external services)
POST    /api/v1/webhooks/payfast                        Signature    2.4

ADMIN (additions)
GET     /api/v1/admin/placements                        Admin        2.6
PATCH   /api/v1/admin/placements/{id}                   Admin        2.6
----------------------------------------------------------------------------
TOTAL NEW: ~37 endpoints | Total after Phase 2: ~67 endpoints
============================================================================
```

---

## New Services Summary

| Service | Sprint | Responsibility | Lines Est. |
|---------|--------|---------------|------------|
| `SendGridEmailSender` | 2.1 | Send emails via SendGrid API | ~60 |
| `RegistrationService` | 2.2 | Register, verify email, password reset | ~200 |
| `SupplierSelfServiceService` | 2.3 | Profile wizard, self-service CRUD | ~250 |
| `BuyerService` | 2.3 | Saved suppliers, lead history | ~100 |
| `SubscriptionService` | 2.4 | Billing state machine, plan enforcement | ~250 |
| `PayFastService` | 2.4 | PayFast checkout + ITN processing | ~200 |
| `SponsoredPlacementService` | 2.6 | Placement CRUD, impression/click tracking | ~150 |
| `AnalyticsService` | 2.6 | Event recording, aggregation queries | ~200 |
| `SupplierDashboardService` | 2.5 | Dashboard stats, supplier lead management | ~150 |

**Monitoring note:** `SubscriptionService` and `SupplierSelfServiceService` are the most likely to approach the 300-line threshold. If they do, split into focused sub-services (e.g., `PlanEnforcementService`, `WizardStepValidator`) rather than adding architectural layers.

---

## New Frontend Pages Summary

| Route | Sprint | Auth | Notes |
|-------|--------|------|-------|
| `/register` | 2.2 | No | Supplier registration |
| `/register/buyer` | 2.3 | No | Buyer registration |
| `/register/check-email` | 2.2 | No | Post-registration confirmation |
| `/verify-email` | 2.2 | No | Email verification handler |
| `/forgot-password` | 2.2 | No | Request password reset |
| `/forgot-password/check-email` | 2.2 | No | Post-request confirmation |
| `/reset-password` | 2.2 | No | New password form |
| `/pricing` | 2.4 | No | Public pricing page |
| `/dashboard` | 2.5 | Supplier | Dashboard home |
| `/dashboard/profile/wizard` | 2.3 | Supplier | Multi-step profile wizard |
| `/dashboard/profile/preview` | 2.3 | Supplier | Profile preview |
| `/dashboard/profile` | 2.5 | Supplier | Profile editor (post-wizard) |
| `/dashboard/leads` | 2.5 | Supplier | Lead management |
| `/dashboard/analytics` | 2.5 | Supplier | Analytics (plan-gated) |
| `/dashboard/billing` | 2.4 | Supplier | Subscription management |
| `/dashboard/billing/invoices` | 2.4 | Supplier | Payment history |
| `/dashboard/placements` | 2.6 | Supplier | Sponsored placements |
| `/buyer/saved` | 2.3 | Buyer | Saved suppliers |
| `/buyer/leads` | 2.3 | Buyer | Lead history |

**Total new frontend pages: ~19**

---

## Architecture Changes

### Auth System Expansion

Phase 1 auth only supports admin login. Phase 2 needs:
- **Multi-role auth:** Supplier, Buyer, Admin (roles already exist in UserRole enum)
- **Route protection:** `/dashboard/*` requires Supplier role, `/buyer/*` requires Buyer role, `/admin/*` requires Admin role
- **API authorization:** New `[Authorize(Roles = "SupplierAdmin,SupplierUser")]` attributes on supplier endpoints
- **Organization scoping:** All supplier self-service operations MUST scope by OrganizationId from the JWT claims -- never trust client-provided IDs

The existing `JwtTokenService` already includes `role` and `organizationId` claims. The auth infrastructure is sufficient -- just needs new middleware/attribute for role-based route guards.

### New Middleware

**`PlanEnforcementMiddleware`** (or attribute-based):
Check the supplier's active plan before allowing access to gated features. Return 403 with `PLAN_UPGRADE_REQUIRED` error code.

### Worker Service Additions

Add to `GreenSuppliers.Worker`:
- `SubscriptionSync` job (hourly) -- handle expired trials, grace periods, end-of-term cancellations
- `AnalyticsAggregation` job (deferred -- only if queries become slow)

### Shared Code Between API and Worker

The Worker currently references the API's `Data/` and `Services/` folders (shared project or direct reference). Phase 2 adds:
- `SubscriptionService` -- needed by both API (billing) and Worker (subscription sync)
- `EsgScoringService` / `VerificationService` -- already shared

This is fine for Tier 2. If sharing becomes painful (circular dependencies, conflicting lifetimes), it is an upgrade trigger for Tier 3.

### Frontend Architecture

The Next.js app needs a new route group for authenticated supplier pages:

```
web/green-suppliers-web/
  app/
    (public)/           -- existing public pages
    (auth)/             -- new: registration, verification, password reset
      register/
      verify-email/
      forgot-password/
      reset-password/
    (dashboard)/        -- new: supplier dashboard (protected)
      layout.tsx        -- auth guard + sidebar nav
      page.tsx          -- dashboard home
      profile/
      leads/
      analytics/
      billing/
      placements/
    (buyer)/            -- new: buyer pages (protected)
      layout.tsx        -- auth guard
      saved/
      leads/
    admin/              -- existing admin pages
```

### New Frontend Infrastructure
- `lib/auth.ts` -- Token storage, refresh logic, auth guards
- `lib/hooks/use-auth.ts` -- Auth hook for components
- `lib/hooks/use-subscription.ts` -- Plan-gating hook
- `components/dashboard/` -- Dashboard-specific components (sidebar, stats cards, charts)
- `components/wizard/` -- Profile wizard step components

---

## Shared Infrastructure Map

Several features share infrastructure. Build once, use everywhere:

```
EMAIL INFRASTRUCTURE (Sprint 2.1)
  SendGridEmailSender -> used by ALL features that send email
  EmailQueueItem entity -> already exists
  EmailDispatch worker -> already exists
  Consumers: registration, verification, password reset, billing receipts,
             cert reminders (existing), lead notifications (existing)

AUTH INFRASTRUCTURE (Sprint 2.2)
  RegistrationService -> used by supplier AND buyer registration
  JwtTokenService -> already exists, shared
  Token generation (email verification, password reset) -> shared pattern
  Consumers: supplier registration, buyer registration, password reset

PLAN ENFORCEMENT (Sprint 2.4)
  SubscriptionService.CanAccessFeatureAsync() -> used by ALL plan-gated features
  Consumers: lead limit checks, analytics access, document upload limits,
             sponsored placement eligibility, featured listing
```

---

## Architectural Decisions Needed Before Coding

### ADR-0002: Phase 2 Complexity Tier Decision (Required)

**Decision needed:** Confirm staying at Tier 2 despite entity count approaching 25+ trigger.
**Recommendation:** Stay Tier 2. 1-2 devs, no circular dependencies, services under 300 lines.
**Record as:** `docs/decisions/0002-phase2-tier-decision.md`

### ADR-0003: Email Provider Selection (Required)

**Decision needed:** SendGrid vs Resend vs Azure Communication Services
**Recommendation:** SendGrid.
- Most mature, best deliverability for transactional email
- Free tier: 100 emails/day (sufficient for early Phase 2)
- Dynamic templates with handlebars (store template IDs, not HTML, in code)
- South Africa deliverability is well-supported
- Upgrade path: just change API key for higher tier
**Risk with alternatives:**
- Resend: newer, less proven SA deliverability, smaller template system
- Azure Communication Services: more complex setup, overkill for transactional email
**Record as:** `docs/decisions/0003-email-provider.md`

### ADR-0004: PayFast Integration Pattern (Required)

**Decision needed:** PayFast hosted checkout vs custom integration vs PayFast Onsite Payments
**Recommendation:** PayFast hosted checkout (redirect flow).
- Simplest integration (redirect to PayFast, ITN callback)
- PCI compliance handled by PayFast (no card data touches our servers)
- Recurring subscriptions supported via PayFast subscription API
- Well-documented for South African payment flows
**Defer:** Stripe integration to Phase 3 when expanding internationally
**Record as:** `docs/decisions/0004-payment-provider.md`

### ADR-0005: Analytics Event Storage Strategy (Decision before Sprint 2.6)

**Decision needed:** Direct DB insert vs queue-based ingestion for analytics events
**Recommendation:** Direct DB insert for Phase 2.
- ProfileViewEvents and SearchImpressionEvents are simple append-only tables
- At 15-30 suppliers, write volume is negligible
- If volume exceeds 10K events/day, migrate to Azure Service Bus + Worker pattern
- Deduplication done at query time (per IP+supplier per hour)
**Upgrade trigger:** When event tables exceed 1M rows or insert latency affects page loads
**Record as:** `docs/decisions/0005-analytics-storage.md` (defer to Sprint 2.6)

---

## Security Considerations

### Authentication
- Registration: password hashing with BCrypt (already in use), minimum 8 chars + 1 upper + 1 number
- Email verification: cryptographically random tokens (not sequential), expire in 24h
- Password reset: tokens expire in 1h, shorter window for security
- Session invalidation: password reset invalidates all refresh tokens
- Rate limiting on auth endpoints: 5 login attempts per 15 min per IP, 3 verification resends per hour per email

### Authorization
- All `/supplier/*` endpoints scoped by OrganizationId from JWT -- NEVER trust client-provided org IDs
- All `/buyer/*` endpoints scoped by UserId from JWT
- PayFast webhook: validate signature + source IP whitelist (PayFast provides IP ranges)
- Sponsored placement requests: validate Premium plan before accepting

### Data Protection
- PayFast tokens stored but no card data on our servers
- Email verification tokens hashed in DB (store hash, send plaintext in email)
- Password reset tokens: same pattern -- hash in DB
- Analytics events: IP addresses stored but anonymized after 90 days (POPI compliance)

### Input Validation
- All new endpoints get FluentValidation validators
- Registration: email format, password strength, company name length
- Wizard steps: field-level validation per step
- PayFast ITN: signature validation before any processing

---

## Testing Strategy for Phase 2

### Unit Tests (Priority Order)
1. `RegistrationService` -- registration flow, email verification, duplicate detection
2. `SubscriptionService` -- plan state machine, lead counting, feature gating
3. `PayFastService` -- signature validation, ITN processing, idempotency
4. `SupplierSelfServiceService` -- wizard step validation, authorization scoping
5. `AnalyticsService` -- deduplication, aggregation queries

### Integration Tests
6. Registration + verification flow (full API round-trip)
7. PayFast checkout + ITN webhook flow
8. Supplier self-service profile update (auth + scoping)
9. Plan enforcement (try to access Pro feature on Free plan)
10. Buyer saved suppliers CRUD

### E2E Tests (Playwright)
11. Full registration -> verify email -> complete wizard -> publish profile
12. Upgrade to Pro plan (mock PayFast redirect)
13. Buyer: register -> search -> save supplier -> submit lead -> view lead history

---

## Migration Strategy

Each sprint produces one EF Core migration. Migrations are additive (new tables, new columns) -- no destructive changes to existing data.

```
Sprint 2.1: No migration (code-only: SendGridEmailSender)
Sprint 2.2: Migration_AddAuthTokenTables (EmailVerificationTokens, PasswordResetTokens)
Sprint 2.3: Migration_AddSelfServiceFields (SupplierProfiles.WizardCompleted/WizardStep, SavedSuppliers)
Sprint 2.4: Migration_AddBillingAndSdg (Subscriptions cols, Payments cols, PayFastWebhookEvents, SdgGoals, SupplierSdgGoals)
Sprint 2.5: No migration (dashboard uses existing data)
Sprint 2.6: Migration_AddAnalyticsAndPlacements (ProfileViewEvents, SearchImpressionEvents, SponsoredPlacements cols)
```

---

## Environment Variables (New for Phase 2)

| Variable | Sprint | Purpose |
|----------|--------|---------|
| `SENDGRID_API_KEY` | 2.1 | SendGrid API key |
| `SENDGRID_FROM_EMAIL` | 2.1 | Sender email address |
| `SENDGRID_FROM_NAME` | 2.1 | Sender display name |
| `SENDGRID_TEMPLATE_EMAIL_VERIFICATION` | 2.1 | Template ID for verification email |
| `SENDGRID_TEMPLATE_PASSWORD_RESET` | 2.1 | Template ID for password reset |
| `SENDGRID_TEMPLATE_WELCOME` | 2.1 | Template ID for welcome email |
| `PAYFAST_MERCHANT_ID` | 2.4 | PayFast merchant ID |
| `PAYFAST_MERCHANT_KEY` | 2.4 | PayFast merchant key |
| `PAYFAST_PASSPHRASE` | 2.4 | PayFast passphrase for signature validation |
| `PAYFAST_SANDBOX` | 2.4 | true/false -- use PayFast sandbox |
| `PAYFAST_RETURN_URL` | 2.4 | URL to redirect after successful payment |
| `PAYFAST_CANCEL_URL` | 2.4 | URL to redirect after cancelled payment |
| `PAYFAST_NOTIFY_URL` | 2.4 | URL for PayFast ITN callbacks |
| `FRONTEND_URL` | 2.2 | Base URL for email links (e.g. https://www.greensuppliers.co.za) |

---

## Out of Scope for Milestone 2

- Stripe integration (Phase 3 -- international expansion)
- Multi-currency billing (Phase 3)
- RFQ workflows (Phase 3)
- API access for enterprise procurement (Phase 3)
- Azure AI Search (keep SQL FTS, swap when needed)
- Microsoft Entra ID (keep JWT, evaluate for Phase 3)
- Automated supplier verification (manual admin review continues)
- Team management (multiple users per supplier org) -- schema supports it but UI deferred
- Mobile app or PWA

---

## Upgrade Triggers to Monitor During Phase 2

These are the triggers from ADR-0001 to watch:

- [ ] Entity count exceeds 25 -- **WILL HIT (27 projected)**. Acceptable because other triggers are not hit.
- [ ] Service classes exceed 300 lines -- Watch `SubscriptionService`, `SupplierSelfServiceService`
- [ ] Circular dependencies between services -- Watch for SubscriptionService <-> SupplierService cycles
- [ ] Team grows past 4 developers -- Not expected for Phase 2
- [ ] Multiple bounded contexts emerge -- Billing is the candidate. If PayFast integration pulls in 5+ services, consider isolating.

If 3+ of these triggers fire during Phase 2 implementation, stop and create ADR for Tier 3 upgrade before continuing.

---

## Open Questions

1. **PayFast sandbox access:** Do we have PayFast sandbox credentials? Need them before Sprint 2.4.
2. **SendGrid account:** Is the SendGrid account created with domain verification for greensuppliers.co.za? Need SPF/DKIM before Sprint 2.1.
3. **Plan pricing:** The R499/R999 pricing is a starting point. Should we validate with market research before coding the pricing page?
4. **Trial period:** Should new Pro/Premium signups get a 14-day free trial? This affects `Subscriptions` schema and `SubscriptionService` logic.
5. **Buyer registration fields:** Minimal (email + password + name) or include company details? Recommendation: minimal, collect company via profile later.
6. **Analytics retention:** How long to keep raw analytics events? Recommendation: 90 days raw, then aggregate to daily rollups and purge. Need to confirm POPI compliance requirements.
