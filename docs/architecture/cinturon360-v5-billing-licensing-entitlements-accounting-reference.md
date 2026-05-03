# Cinturon360 v5 Billing, Licensing, Entitlements, Payments, and Accounting Reference

## Purpose

This document captures the preferred v5 design direction for Cinturon360 billing, licensing, entitlements, payment method handling, invoice generation, ledger records, and future accounting integration.

It is intended as an implementation reference for an AI coding agent or developer updating the Cinturon360 platform from the older v4 `LicenseAgreementUnified` model toward a cleaner v5 domain model.

The v4 `LicenseAgreementUnified` class attempted to hold too many responsibilities in a single entity:

- commercial licence agreement details;
- billing model configuration;
- access fees;
- thresholds;
- discounts;
- service fees;
- Stripe customer/payment method metadata;
- card cost profile;
- prepaid balance;
- payment status;
- late fee settings;
- capacity limits;
- audit fields.

For v5, these concerns should be separated into smaller domain objects.

---

# 1. Core Design Principles

## 1.1 Agreement is not the financial ledger

The licence agreement defines commercial terms. It must not become the source of truth for financial balances, payments, invoices, or accounting journals.

Use the agreement to answer:

- who the agreement is between;
- which organisation is licensed/billed;
- which organisation issued the agreement;
- when the agreement starts and ends;
- whether it auto-renews;
- whether it is active, suspended, expired, or superseded;
- what billing model applies;
- which recurring access fee applies;
- which fee rules apply;
- which entitlements/modules/limits apply;
- which collection and payment terms apply.

Do not use the agreement to answer:

- current prepaid balance;
- current outstanding amount;
- latest invoice status;
- latest payment failure;
- current card on file;
- Stripe fee amount;
- journal debit/credit truth;
- account receivable balance;
- revenue recognised.

Those belong to billing accounts, invoices, payment attempts, ledger records, and accounting journal entries.

## 1.2 Commercial state and accounting state must be separate

Commercial configuration controls what should happen.

Accounting records capture what did happen.

Example:

```text
Commercial rule:
- Agreement billing model is Postpaid.
- Access fee is AUD 500 monthly.
- Payment terms are Net14.

Accounting result:
- Invoice issued on 1 May 2026.
- DR Accounts Receivable AUD 550.
- CR Platform Revenue AUD 500.
- CR GST Payable AUD 50.
```

## 1.3 Payment provider details should not live on the licence agreement

The agreement should not directly contain Stripe fields such as:

- `StripeCustomerId`;
- `StripeAccountId`;
- `DefaultPaymentMethodId`;
- `CardLast4`;
- `CardFingerPrint`;
- card brand;
- card funding;
- high-cost card flags.

Those belong to organisation payment profiles and stored payment method records.

This keeps Cinturon360 flexible enough to support:

- Stripe;
- bank transfer;
- manual invoice;
- BSP settlement;
- supplier settlement;
- Airwallex;
- Adyen;
- Xero/MYOB/NetSuite exports later.

## 1.4 Entitlements should be grouped and expandable

Entitlements represent modules, limits, base capabilities, and future commercial packaging.

Use grouped numeric ranges so they are easy to read and expand:

```text
100-199     Organisation / tenant capacity
200-299     User / identity / access
300-399     Traveller profile / personal travel data
400-499     Booking / travel operations
500-599     Public / search / marketplace
600-699     API / integration / automation
700-799     Reporting / analytics / finance visibility
800-899     Support / service desk / operational tools
900-999     Security / compliance / audit
1000-1099   Billing / accountancy / settlement
1100-1199   AI / automation / assistant features
1200-1299   Data retention / storage / documents
9000-9999   Internal / experimental / migration
```

---

# 2. Recommended Domain Areas

Use a structure similar to this:

```text
Cinturon360.Domain/
  Billing/
    Agreements/
      LicenseAgreement.cs
      AgreementStatus.cs
      AgreementKind.cs
      AgreementBillingProfile.cs
      AgreementFeeRule.cs
      AgreementAdjustmentRule.cs
      AgreementCollectionPolicy.cs
      LicenseAgreementEntitlement.cs

    Accounts/
      BillingAccount.cs
      BillingAccountStatus.cs
      BillingLedgerEntry.cs
      BillingLedgerEntryType.cs
      BalanceMovementType.cs

    Invoicing/
      Invoice.cs
      InvoiceLine.cs
      InvoiceStatus.cs
      InvoicePaymentStatus.cs

    Payments/
      OrganizationPaymentProfile.cs
      OrganizationPaymentMethod.cs
      PaymentMethodAssignment.cs
      PaymentAttempt.cs
      PaymentProvider.cs
      PaymentMethodKind.cs

    Accounting/
      LedgerAccount.cs
      LedgerAccountType.cs
      JournalBatch.cs
      JournalEntry.cs
      JournalLine.cs
      DebitCredit.cs
      AccountingProvider.cs
      ExternalAccountingAccountMapping.cs

    Tax/
      AgreementTaxProfile.cs
      TaxTreatment.cs
```

---

# 3. Billing Model

## 3.1 BillingModel

`BillingModel` defines how financial consumption is authorised.

```csharp
public enum BillingModel
{
    Prepaid = 1,
    Postpaid = 2,
    PayAsYouGo = 3
}
```

### Prepaid

The organisation can only spend from a balance that has been loaded into the system.

Rules:

- spend must be authorised against available balance;
- the organisation cannot spend more than available balance;
- booking/payment flows should block if available balance is insufficient;
- available balance should be derived from ledger/balance movements, not stored directly on the agreement;
- prepaid funds are usually treated as a liability until consumed.

Example:

```text
Organisation balance: AUD 1,000
Booking/service charge: AUD 250
Allowed: yes
Remaining available balance after consumption/reservation: AUD 750
```

If balance is AUD 100 and charge is AUD 250:

```text
Allowed: no
Reason: insufficient prepaid balance
```

### Postpaid

The organisation can spend now and pay after invoice issue.

Rules:

- balance can be zero;
- billing can happen later according to billing period/frequency;
- payment terms define when payment becomes due;
- credit limit may optionally apply;
- account can be suspended if overdue beyond terms and grace period;
- outstanding amounts are tracked through invoices and receivables, not agreement fields.

Example:

```text
BillingModel: Postpaid
BillingPeriod: Monthly
PaymentTerms: Net14
GracePeriodDays: 3

Organisation books travel throughout May.
Invoice issued 1 June.
Due date 15 June.
Collection action can begin 18 June if unpaid.
```

### PayAsYouGo

The organisation is charged per usage/event.

Rules:

- usage creates a charge event;
- payment may be captured immediately or batched into a frequent invoice;
- suitable for lightweight clients, public search conversion, or low-commitment usage models;
- payment method resolution is important because different payment methods may apply to different operations.

Example:

```text
Flight booking service fee: AUD 25
Payment captured immediately at booking creation.
Invoice/receipt generated after successful payment.
```

---

# 4. Billing Account Status

`BillingAccountStatus` defines whether the account can currently transact.

Do not put `Suspended` inside `BillingModel`. Suspension is account/commercial state, not a billing model.

```csharp
public enum BillingAccountStatus
{
    Active = 1,
    Suspended = 2,
    Closed = 3,
    UnderReview = 4
}
```

This allows combinations such as:

```text
Prepaid + Active
Prepaid + Suspended
Postpaid + Active
Postpaid + Suspended
PayAsYouGo + UnderReview
```

## Status meaning

| Status | Meaning |
|---|---|
| `Active` | Account can transact normally. |
| `Suspended` | Account is blocked or restricted due to billing, risk, compliance, or admin action. |
| `Closed` | Account is no longer usable for billing. Historical records remain. |
| `UnderReview` | Account may be restricted pending finance/risk/admin review. |

---

# 5. Billing Period and Payment Terms

## 5.1 BillingPeriod

`BillingPeriod` defines how often recurring billing is actioned.

This applies to things like:

- platform access fee billing;
- subscription-style recurring charges;
- minimum spend checks;
- recurring entitlement charges;
- regular invoice generation.

```csharp
public enum BillingPeriod
{
    None = 0,
    Daily = 1,
    Weekly = 2,
    Fortnightly = 3,
    Monthly = 4,
    Quarterly = 5,
    BiAnnual = 6,
    Annual = 7,
    Manual = 8
}
```

Example:

```text
AccessFeeAmount = 500
AccessFeeCurrencyCode = AUD
AccessFeeBillingPeriod = Monthly

Meaning:
Invoice or charge AUD 500 per month for platform access.
```

## 5.2 PaymentTerms

`PaymentTerms` defines how many days after invoice issue the customer has to pay.

```csharp
public enum PaymentTerms
{
    DueImmediately = 0,
    Net7 = 7,
    Net14 = 14,
    Net30 = 30,
    Net45 = 45,
    Net60 = 60,
    Net90 = 90
}
```

Example:

```text
Invoice issued: 1 May 2026
PaymentTerms: Net14
Due date: 15 May 2026
GracePeriodDays: 3
Collection/suspension action can begin: 18 May 2026
```

## 5.3 Concept split

```text
BillingModel     = how spending is authorised
BillingPeriod    = how often recurring billing happens
PaymentTerms     = how long after invoice issue payment is allowed
GracePeriodDays  = extra collection/suspension buffer after due date
AccountStatus    = whether the account can currently transact
```

---

# 6. Licence Agreement Root

## 6.1 Purpose

The licence agreement represents the commercial agreement between an issuing organisation and a licensed organisation.

Examples:

```text
Platform/Vendor -> TMC
TMC -> Client
Vendor -> Child Vendor
Platform -> Vendor
```

The agreement should hold the core commercial lifecycle only.

## 6.2 Recommended entity

```csharp
using System.ComponentModel.DataAnnotations;

namespace Cinturon360.Domain.Billing.Agreements;

public sealed class LicenseAgreement
{
    [Key]
    [MaxLength(18)]
    public required string Id { get; init; }

    [Required]
    [MaxLength(18)]
    public required string LicensedOrganizationId { get; init; }

    [Required]
    [MaxLength(18)]
    public required string IssuedByOrganizationId { get; init; }

    public AgreementStatus Status { get; set; } = AgreementStatus.Draft;

    public AgreementKind AgreementKind { get; init; } = AgreementKind.StandardPlatformAccess;

    public int VersionNumber { get; init; } = 1;

    [MaxLength(18)]
    public string? PreviousAgreementId { get; init; }

    [MaxLength(18)]
    public string? SupersededByAgreementId { get; set; }

    public DateOnly StartsOn { get; init; }
    public DateOnly? EndsOn { get; set; }
    public DateOnly? RenewsOn { get; set; }

    public bool AutoRenew { get; set; }

    public DateTimeOffset? TrialEndsAtUtc { get; set; }

    public DateTimeOffset? AcceptedAtUtc { get; set; }

    [MaxLength(18)]
    public string? AcceptedByUserId { get; set; }

    [MaxLength(320)]
    public string? AcceptedByEmail { get; set; }

    [MaxLength(100)]
    public string? AcceptedFromIpAddress { get; set; }

    [MaxLength(500)]
    public string? AcceptedUserAgent { get; set; }

    public DateTimeOffset CreatedAtUtc { get; init; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAtUtc { get; set; } = DateTimeOffset.UtcNow;

    [MaxLength(18)]
    public string? CreatedByUserId { get; init; }

    [MaxLength(18)]
    public string? UpdatedByUserId { get; set; }

    public List<AgreementFeeRule> FeeRules { get; init; } = [];
    public List<AgreementAdjustmentRule> AdjustmentRules { get; init; } = [];
    public List<LicenseAgreementEntitlement> Entitlements { get; init; } = [];
}
```

---

# 7. Agreement Status

```csharp
public enum AgreementStatus
{
    Draft = 1,
    PendingAcceptance = 2,
    Active = 3,
    Suspended = 4,
    Expired = 5,
    Cancelled = 6,
    Terminated = 7,
    Superseded = 8
}
```

| Status | Meaning |
|---|---|
| `Draft` | Created internally but not issued or active. |
| `PendingAcceptance` | Issued to the organisation/user but not yet accepted. |
| `Active` | Commercially active. Billing and entitlements can apply. |
| `Suspended` | Agreement access or commercial activity is temporarily restricted. |
| `Expired` | End date has passed and agreement was not renewed. |
| `Cancelled` | Cancelled before or during the term by commercial process. |
| `Terminated` | Ended due to breach, admin action, contract termination, or commercial decision. |
| `Superseded` | Replaced by a newer agreement/version. |

---

# 8. Agreement Kind

```csharp
public enum AgreementKind
{
    StandardPlatformAccess = 1,
    VendorPlatformLicence = 2,
    TmcClientLicence = 3,
    MarketplaceSupplierAgreement = 4,
    InternalTestAgreement = 5
}
```

| Kind | Meaning |
|---|---|
| `StandardPlatformAccess` | General platform access agreement. |
| `VendorPlatformLicence` | Agreement between platform and vendor. |
| `TmcClientLicence` | Agreement between TMC and client organisation. |
| `MarketplaceSupplierAgreement` | Future supplier/marketplace agreement. |
| `InternalTestAgreement` | Internal or non-production agreement. |

---

# 9. Agreement Billing Profile

## 9.1 Purpose

The billing profile stores the commercial billing configuration for the agreement.

It should not contain invoice status, actual balance, card data, or payment attempt details.

## 9.2 Recommended entity

```csharp
using System.ComponentModel.DataAnnotations;

namespace Cinturon360.Domain.Billing.Agreements;

public sealed class AgreementBillingProfile
{
    [Key]
    [MaxLength(18)]
    public required string Id { get; init; }

    [Required]
    [MaxLength(18)]
    public required string AgreementId { get; init; }

    public BillingModel BillingModel { get; init; }

    public BillingPeriod AccessFeeBillingPeriod { get; init; }

    public PaymentTerms PaymentTerms { get; init; }

    public int GracePeriodDays { get; init; }

    public decimal AccessFeeAmount { get; init; }

    [MaxLength(3)]
    public string AccessFeeCurrencyCode { get; init; } = "AUD";

    public decimal? MinimumSpendAmount { get; init; }

    [MaxLength(3)]
    public string? MinimumSpendCurrencyCode { get; init; }

    public BillingPeriod? MinimumSpendPeriod { get; init; }

    public decimal? PostpaidCreditLimitAmount { get; init; }

    [MaxLength(3)]
    public string? PostpaidCreditLimitCurrencyCode { get; init; }

    public bool RequiresPositiveBalanceForBooking =>
        BillingModel == BillingModel.Prepaid;

    public bool AllowsZeroOrNegativeOperationalBalance =>
        BillingModel == BillingModel.Postpaid;

    public bool RequiresImmediatePayment =>
        BillingModel == BillingModel.PayAsYouGo;

    public DateTimeOffset CreatedAtUtc { get; init; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
}
```

## 9.3 Behaviour notes

### Prepaid

- Check available balance before allowing bookings or billable operations.
- Block spend when balance is insufficient.
- Use billing ledger/balance records to calculate balance.
- Treat customer funds as a liability until consumed.

### Postpaid

- Allow spending even where balance is zero.
- Optionally enforce credit limit.
- Generate invoices on billing period.
- Payment terms and grace period control overdue/suspension logic.

### PayAsYouGo

- Charge per event/usage.
- Payment can be immediate or batched.
- Best for lightweight usage, casual clients, or low-commitment clients.

---

# 10. Entitlement Grouping

## 10.1 Group ranges

Use the following grouped numeric ranges:

```text
100-199     Organisation / tenant capacity
200-299     User / identity / access
300-399     Traveller profile / personal travel data
400-499     Booking / travel operations
500-599     Public / search / marketplace
600-699     API / integration / automation
700-799     Reporting / analytics / finance visibility
800-899     Support / service desk / operational tools
900-999     Security / compliance / audit
1000-1099   Billing / accountancy / settlement
1100-1199   AI / automation / assistant features
1200-1299   Data retention / storage / documents
9000-9999   Internal / experimental / migration
```

## 10.2 Recommended enum

```csharp
public enum EntitlementType
{
    // ---------------------------------------------------------------------
    // 100-199: Organisation / tenant capacity
    // ---------------------------------------------------------------------
    ClientOrganizations = 100,
    ChildVendorOrganizations = 101,
    TmcOrganizations = 102,
    ActiveOrganizations = 103,
    OrganisationHierarchyDepth = 104,

    // ---------------------------------------------------------------------
    // 200-299: User / identity / access
    // ---------------------------------------------------------------------
    UserAccounts = 200,
    ActiveUsers = 201,
    AdminUsers = 202,
    ConcurrentSessions = 203,
    SsoEnabled = 220,
    SamlEnabled = 221,
    OidcEnabled = 222,
    ScimEnabled = 223,
    MfaRequired = 224,
    PersonalAccessTokens = 225,
    ServiceAccounts = 226,
    QrDeviceLogin = 227,

    // ---------------------------------------------------------------------
    // 300-399: Traveller profile / personal travel data
    // ---------------------------------------------------------------------
    Travellers = 300,
    TravellerProfiles = 301,
    PassportProfiles = 302,
    LoyaltyPrograms = 303,
    EmergencyContacts = 304,
    TravelPreferences = 305,

    // ---------------------------------------------------------------------
    // 400-499: Booking / travel operations
    // ---------------------------------------------------------------------
    MonthlyBookings = 400,
    AnnualBookings = 401,
    FlightBookings = 410,
    HotelBookings = 411,
    CarBookings = 412,
    RailBookings = 413,
    TransferBookings = 414,
    ActivityBookings = 415,
    OfflineBookings = 420,
    AgentAssistedBookings = 421,
    BookingApprovals = 430,
    MultiLevelApprovals = 431,
    DelegatedApprovals = 432,
    TravelPolicies = 440,
    RegionalTravelPolicies = 441,
    CostCentreAllocation = 442,

    // ---------------------------------------------------------------------
    // 500-599: Public / search / marketplace
    // ---------------------------------------------------------------------
    PublicSearchAccess = 500,
    PublicFlightSearch = 501,
    PublicHotelSearch = 502,
    PublicLeadCapture = 503,
    MarketplaceAccess = 520,
    SupplierMarketplace = 521,

    // ---------------------------------------------------------------------
    // 600-699: API / integration / automation
    // ---------------------------------------------------------------------
    ApiAccess = 600,
    ApiRequestsPerMonth = 601,
    Webhooks = 602,
    ScheduledImports = 603,
    BatchImports = 604,
    DataFeedAccess = 605,
    ExternalBookingImport = 620,
    GdsIntegration = 630,
    NdcIntegration = 631,
    PaymentProviderIntegration = 640,
    AccountingSystemExport = 650,

    // ---------------------------------------------------------------------
    // 700-799: Reporting / analytics / finance visibility
    // ---------------------------------------------------------------------
    BasicReporting = 700,
    AdvancedReporting = 701,
    CustomReports = 702,
    ScheduledReports = 703,
    FinanceReporting = 720,
    InvoiceReporting = 721,
    PaymentReporting = 722,
    LedgerReporting = 723,
    BspReporting = 724,
    AdmAcmReporting = 725,
    RefundReporting = 726,

    // ---------------------------------------------------------------------
    // 800-899: Support / service desk / operational tools
    // ---------------------------------------------------------------------
    SupportDeskAccess = 800,
    TicketingAccess = 801,
    EscalationWorkflows = 802,
    TmcSupportQueue = 803,
    VendorSupportQueue = 804,
    EmergencySupport = 820,
    AfterHoursSupport = 821,

    // ---------------------------------------------------------------------
    // 900-999: Security / compliance / audit
    // ---------------------------------------------------------------------
    AuditLogAccess = 900,
    ExtendedAuditRetention = 901,
    ComplianceExports = 902,
    SecurityReports = 903,
    TenantIsolationReports = 904,
    DataProcessingReports = 905,

    // ---------------------------------------------------------------------
    // 1000-1099: Billing / accountancy / settlement
    // ---------------------------------------------------------------------
    BillingModule = 1000,
    InvoicingModule = 1001,
    PaymentCollection = 1002,
    PrepaidWallet = 1003,
    PostpaidCreditAccount = 1004,
    GeneralLedgerExport = 1010,
    DoubleEntryLedger = 1011,
    JournalExport = 1012,
    TaxReporting = 1020,
    BspSettlement = 1030,
    SupplierSettlement = 1031,

    // ---------------------------------------------------------------------
    // 1100-1199: AI / automation / assistant features
    // ---------------------------------------------------------------------
    AiAssistant = 1100,
    AiTripSummary = 1101,
    AiPolicyCheck = 1102,
    AiInvoiceReconciliation = 1103,
    AiSupportTriage = 1104,

    // ---------------------------------------------------------------------
    // 1200-1299: Data retention / storage / documents
    // ---------------------------------------------------------------------
    DocumentStorage = 1200,
    DocumentRetentionYears = 1201,
    AttachmentStorageGb = 1202,
    ExportArchive = 1203,

    // ---------------------------------------------------------------------
    // 9000-9999: Internal / experimental / migration
    // ---------------------------------------------------------------------
    InternalTesting = 9000,
    LegacyMigrationAccess = 9001,
    ExperimentalFeatures = 9002
}
```

---

# 11. Entitlement Metadata

## 11.1 Why metadata is needed

The enum is useful for code, but the platform should also have a catalogue of entitlement definitions.

This allows the admin UI, billing engine, navigation engine, and API authorisation system to understand:

- display name;
- description;
- grouping;
- whether the entitlement is boolean or quantity-based;
- whether it is billable;
- whether it is a module;
- whether it is a usage limit;
- whether it is internal only;
- whether it can inherit down the organisation tree;
- whether it affects navigation;
- whether it affects API access.

## 11.2 EntitlementGroup

```csharp
public enum EntitlementGroup
{
    Organisation = 100,
    IdentityAccess = 200,
    TravellerProfile = 300,
    BookingOperations = 400,
    PublicSearchMarketplace = 500,
    ApiIntegration = 600,
    ReportingAnalytics = 700,
    SupportOperations = 800,
    SecurityComplianceAudit = 900,
    BillingAccountingSettlement = 1000,
    AiAutomation = 1100,
    DataRetentionStorage = 1200,
    Internal = 9000
}
```

## 11.3 EntitlementValueKind

```csharp
public enum EntitlementValueKind
{
    Boolean = 1,
    Quantity = 2,
    Money = 3,
    Percentage = 4,
    DurationDays = 5,
    DurationMonths = 6,
    StorageGb = 7,
    RequestsPerPeriod = 8
}
```

## 11.4 EntitlementCommercialKind

```csharp
public enum EntitlementCommercialKind
{
    BaseComponent = 1,
    PaidModule = 2,
    UsageLimit = 3,
    InternalCapability = 4,
    ComplianceFeature = 5
}
```

## 11.5 EntitlementDefinition

```csharp
using System.ComponentModel.DataAnnotations;

namespace Cinturon360.Domain.Billing.Agreements;

public sealed class EntitlementDefinition
{
    [Key]
    public EntitlementType Type { get; init; }

    public EntitlementGroup Group { get; init; }

    public EntitlementValueKind ValueKind { get; init; }

    public EntitlementCommercialKind CommercialKind { get; init; }

    [MaxLength(120)]
    public required string Code { get; init; }

    [MaxLength(200)]
    public required string DisplayName { get; init; }

    [MaxLength(1000)]
    public string? Description { get; init; }

    public bool IsBillable { get; init; }

    public bool IsModule { get; init; }

    public bool IsLimit { get; init; }

    public bool IsInternalOnly { get; init; }

    public bool CanInheritToChildOrganizations { get; init; }

    public bool AffectsNavigation { get; init; }

    public bool AffectsApiAccess { get; init; }
}
```

## 11.6 Example definitions

```csharp
public static class EntitlementDefinitions
{
    public static readonly EntitlementDefinition UserAccounts = new()
    {
        Type = EntitlementType.UserAccounts,
        Group = EntitlementGroup.IdentityAccess,
        ValueKind = EntitlementValueKind.Quantity,
        CommercialKind = EntitlementCommercialKind.UsageLimit,
        Code = "identity.users",
        DisplayName = "User accounts",
        Description = "Maximum number of user accounts allowed under the agreement.",
        IsBillable = true,
        IsModule = false,
        IsLimit = true,
        IsInternalOnly = false,
        CanInheritToChildOrganizations = false,
        AffectsNavigation = false,
        AffectsApiAccess = false
    };

    public static readonly EntitlementDefinition SsoEnabled = new()
    {
        Type = EntitlementType.SsoEnabled,
        Group = EntitlementGroup.IdentityAccess,
        ValueKind = EntitlementValueKind.Boolean,
        CommercialKind = EntitlementCommercialKind.PaidModule,
        Code = "identity.sso",
        DisplayName = "Single sign-on",
        Description = "Allows the organisation to configure SSO using OIDC or SAML.",
        IsBillable = true,
        IsModule = true,
        IsLimit = false,
        IsInternalOnly = false,
        CanInheritToChildOrganizations = false,
        AffectsNavigation = true,
        AffectsApiAccess = false
    };

    public static readonly EntitlementDefinition ApiRequestsPerMonth = new()
    {
        Type = EntitlementType.ApiRequestsPerMonth,
        Group = EntitlementGroup.ApiIntegration,
        ValueKind = EntitlementValueKind.RequestsPerPeriod,
        CommercialKind = EntitlementCommercialKind.UsageLimit,
        Code = "api.requests.monthly",
        DisplayName = "API requests per month",
        Description = "Maximum number of API requests allowed per calendar month or billing period.",
        IsBillable = true,
        IsModule = false,
        IsLimit = true,
        IsInternalOnly = false,
        CanInheritToChildOrganizations = false,
        AffectsNavigation = false,
        AffectsApiAccess = true
    };
}
```

---

# 12. Licence Agreement Entitlement

## 12.1 Purpose

`LicenseAgreementEntitlement` stores the actual entitlement value granted by a specific agreement.

It supports boolean modules, numeric limits, storage limits, duration limits, and future text/config values.

## 12.2 Entity

```csharp
using System.ComponentModel.DataAnnotations;

namespace Cinturon360.Domain.Billing.Agreements;

public sealed class LicenseAgreementEntitlement
{
    [Key]
    [MaxLength(18)]
    public required string Id { get; init; }

    [Required]
    [MaxLength(18)]
    public required string AgreementId { get; init; }

    public EntitlementType Type { get; init; }

    public EntitlementValueKind ValueKind { get; init; }

    public bool? BooleanValue { get; init; }

    public decimal? NumericValue { get; init; }

    [MaxLength(1000)]
    public string? TextValue { get; init; }

    public bool IsUnlimited { get; init; }

    public DateOnly EffectiveFrom { get; init; }

    public DateOnly? EffectiveTo { get; init; }

    public bool IsActive { get; set; } = true;

    [MaxLength(1000)]
    public string? Notes { get; init; }
}
```

## 12.3 Examples

```csharp
var ssoEnabled = new LicenseAgreementEntitlement
{
    Id = "ent_000000000001",
    AgreementId = "lic_000000000001",
    Type = EntitlementType.SsoEnabled,
    ValueKind = EntitlementValueKind.Boolean,
    BooleanValue = true,
    EffectiveFrom = new DateOnly(2026, 1, 1)
};

var userLimit = new LicenseAgreementEntitlement
{
    Id = "ent_000000000002",
    AgreementId = "lic_000000000001",
    Type = EntitlementType.UserAccounts,
    ValueKind = EntitlementValueKind.Quantity,
    NumericValue = 250,
    EffectiveFrom = new DateOnly(2026, 1, 1)
};

var apiLimit = new LicenseAgreementEntitlement
{
    Id = "ent_000000000003",
    AgreementId = "lic_000000000001",
    Type = EntitlementType.ApiRequestsPerMonth,
    ValueKind = EntitlementValueKind.RequestsPerPeriod,
    NumericValue = 100_000,
    EffectiveFrom = new DateOnly(2026, 1, 1)
};

var unlimitedTravellers = new LicenseAgreementEntitlement
{
    Id = "ent_000000000004",
    AgreementId = "lic_000000000001",
    Type = EntitlementType.Travellers,
    ValueKind = EntitlementValueKind.Quantity,
    IsUnlimited = true,
    EffectiveFrom = new DateOnly(2026, 1, 1)
};
```

---

# 13. Base Components vs Modules

## 13.1 Base components

Base components are core platform capabilities that usually exist for most organisations but may have limits.

Examples:

- organisation hierarchy;
- users;
- travellers;
- bookings;
- basic reporting;
- billing account;
- invoices;
- audit log.

## 13.2 Modules

Modules are product areas that can be enabled, disabled, sold, trialled, or restricted.

Examples:

- SSO;
- SCIM;
- advanced reporting;
- finance reporting;
- support desk;
- public search;
- API access;
- GDS integration;
- BSP reporting;
- AI assistant;
- general ledger export.

## 13.3 Usage limits

Usage limits are numeric controls.

Examples:

- user account count;
- monthly booking count;
- API requests per month;
- attachment storage GB;
- document retention years.

---

# 14. Fee Rules

## 14.1 Why v4 fee columns should be replaced

The v4 agreement had many product-specific fields:

```csharp
public decimal PnrCreationFee { get; set; }
public decimal PnrChangeFee { get; set; }

public decimal FlightMarkupPercent { get; set; }
public decimal FlightPerItemFee { get; set; }
public ServiceFeeType FlightFeeType { get; set; }

public decimal HotelMarkupPercent { get; set; }
public decimal HotelPerItemFee { get; set; }
public ServiceFeeType HotelFeeType { get; set; }
```

This does not scale.

Future fees may include:

- visa processing;
- insurance;
- lounge;
- baggage;
- seat selection;
- meal selection;
- ticketing;
- refund;
- exchange;
- after-hours support;
- emergency support;
- agent-assisted booking;
- online booking;
- offline booking.

Use a row-based `AgreementFeeRule` model instead.

## 14.2 Fee enums

```csharp
public enum TravelProductType
{
    Unknown = 0,
    Flight = 1,
    Hotel = 2,
    Car = 3,
    Rail = 4,
    Transfer = 5,
    Activity = 6,
    Insurance = 7,
    Visa = 8,
    Package = 9,
    GenericTravel = 99
}

public enum FeeTrigger
{
    BookingCreated = 1,
    BookingChanged = 2,
    BookingCancelled = 3,
    TicketIssued = 4,
    TicketRefunded = 5,
    TicketExchanged = 6,
    PnrCreated = 7,
    PnrChanged = 8,
    ManualService = 9,
    AfterHoursSupport = 10,
    EmergencySupport = 11
}

public enum ServiceFeeType
{
    None = 0,
    FixedAmount = 1,
    Percentage = 2,
    FixedAmountPlusPercentage = 3,
    Included = 4,
    Waived = 5
}
```

## 14.3 AgreementFeeRule entity

```csharp
using System.ComponentModel.DataAnnotations;

namespace Cinturon360.Domain.Billing.Agreements;

public sealed class AgreementFeeRule
{
    [Key]
    [MaxLength(18)]
    public required string Id { get; init; }

    [Required]
    [MaxLength(18)]
    public required string AgreementId { get; init; }

    public TravelProductType ProductType { get; init; }

    public FeeTrigger Trigger { get; init; }

    public ServiceFeeType FeeType { get; init; }

    public decimal? FixedAmount { get; init; }

    [MaxLength(3)]
    public string? FixedAmountCurrencyCode { get; init; }

    public decimal? Percent { get; init; }

    public bool AppliesPerTraveller { get; init; }

    public bool AppliesPerBooking { get; init; }

    public bool AppliesPerSegment { get; init; }

    public bool AppliesPerNight { get; init; }

    public DateOnly EffectiveFrom { get; init; }

    public DateOnly? EffectiveTo { get; init; }

    public bool IsActive { get; set; } = true;

    [MaxLength(500)]
    public string? Description { get; init; }
}
```

## 14.4 Fee rule examples

```csharp
var flightBookingFee = new AgreementFeeRule
{
    Id = "fee_000000000001",
    AgreementId = "lic_000000000001",
    ProductType = TravelProductType.Flight,
    Trigger = FeeTrigger.BookingCreated,
    FeeType = ServiceFeeType.FixedAmount,
    FixedAmount = 25.00m,
    FixedAmountCurrencyCode = "AUD",
    AppliesPerBooking = true,
    EffectiveFrom = new DateOnly(2026, 1, 1),
    Description = "Standard flight booking service fee."
};

var hotelMarkup = new AgreementFeeRule
{
    Id = "fee_000000000002",
    AgreementId = "lic_000000000001",
    ProductType = TravelProductType.Hotel,
    Trigger = FeeTrigger.BookingCreated,
    FeeType = ServiceFeeType.Percentage,
    Percent = 3.5m,
    AppliesPerBooking = true,
    EffectiveFrom = new DateOnly(2026, 1, 1),
    Description = "Hotel booking markup."
};

var emergencySupportFee = new AgreementFeeRule
{
    Id = "fee_000000000003",
    AgreementId = "lic_000000000001",
    ProductType = TravelProductType.GenericTravel,
    Trigger = FeeTrigger.EmergencySupport,
    FeeType = ServiceFeeType.FixedAmount,
    FixedAmount = 75.00m,
    FixedAmountCurrencyCode = "AUD",
    AppliesPerBooking = false,
    EffectiveFrom = new DateOnly(2026, 1, 1),
    Description = "Emergency support fee."
};
```

---

# 15. Discounts and Adjustments

## 15.1 Why v4 discount fields should be replaced

The v4 model used two fixed discount slots:

```csharp
public PeriodScopedFlatDiscount? DiscountA { get; set; }
public PeriodScopedPercentDiscount? DiscountB { get; set; }
```

Problems:

- only two discounts;
- field names have no business meaning;
- no stacking rules;
- no priority;
- no applies-to category;
- no effective start date;
- fixed amount discount has no currency;
- cannot naturally handle credits, waivers, or promotional adjustments.

## 15.2 Adjustment enums

```csharp
public enum AdjustmentKind
{
    Discount = 1,
    Credit = 2,
    PromotionalCredit = 3,
    ManualAdjustment = 4,
    Waiver = 5
}

public enum AdjustmentCalculationType
{
    FixedAmount = 1,
    Percentage = 2
}

public enum BillingChargeCategory
{
    Any = 0,
    AccessFee = 1,
    ServiceFee = 2,
    BookingFee = 3,
    LateFee = 4,
    Tax = 5,
    ManualCharge = 6
}
```

## 15.3 AgreementAdjustmentRule entity

```csharp
using System.ComponentModel.DataAnnotations;

namespace Cinturon360.Domain.Billing.Agreements;

public sealed class AgreementAdjustmentRule
{
    [Key]
    [MaxLength(18)]
    public required string Id { get; init; }

    [Required]
    [MaxLength(18)]
    public required string AgreementId { get; init; }

    public AdjustmentKind Kind { get; init; }

    public AdjustmentCalculationType CalculationType { get; init; }

    public decimal Value { get; init; }

    [MaxLength(3)]
    public string? CurrencyCode { get; init; }

    public BillingChargeCategory AppliesTo { get; init; }

    public DateOnly EffectiveFrom { get; init; }

    public DateOnly? EffectiveTo { get; init; }

    public int Priority { get; init; } = 100;

    public bool CanStackWithOtherAdjustments { get; init; }

    [MaxLength(500)]
    public string? Description { get; init; }
}
```

## 15.4 Example

```csharp
var launchDiscount = new AgreementAdjustmentRule
{
    Id = "adj_000000000001",
    AgreementId = "lic_000000000001",
    Kind = AdjustmentKind.Discount,
    CalculationType = AdjustmentCalculationType.Percentage,
    Value = 20m,
    AppliesTo = BillingChargeCategory.AccessFee,
    EffectiveFrom = new DateOnly(2026, 1, 1),
    EffectiveTo = new DateOnly(2026, 6, 30),
    CanStackWithOtherAdjustments = false,
    Description = "Launch discount for first six months."
};
```

---

# 16. Collection Policy and Late Fees

## 16.1 Purpose

The collection policy controls what happens after invoice issue and non-payment.

It should hold:

- payment terms;
- grace period;
- late fee behaviour;
- maximum late fee cap;
- post-grace action;
- suspension logic.

Do not duplicate grace period fields in multiple places.

## 16.2 Enums

```csharp
public enum LateFeeCalculationType
{
    None = 0,
    FixedAmount = 1,
    PercentageOfOutstanding = 2,
    FixedAmountPlusPercentage = 3
}

public enum CollectionActionAfterGrace
{
    NotifyOnly = 1,
    SuspendNewBookings = 2,
    SuspendAllAccess = 3,
    RequireManualReview = 4
}
```

## 16.3 Entity

```csharp
using System.ComponentModel.DataAnnotations;

namespace Cinturon360.Domain.Billing.Agreements;

public sealed class AgreementCollectionPolicy
{
    [Key]
    [MaxLength(18)]
    public required string Id { get; init; }

    [Required]
    [MaxLength(18)]
    public required string AgreementId { get; init; }

    public PaymentTerms PaymentTerms { get; init; } = PaymentTerms.DueImmediately;

    public int GracePeriodDays { get; init; }

    public bool ApplyLateFees { get; init; }

    public LateFeeCalculationType LateFeeCalculationType { get; init; }

    public decimal? FixedLateFeeAmount { get; init; }

    [MaxLength(3)]
    public string? FixedLateFeeCurrencyCode { get; init; }

    public decimal? LateFeePercentOfOutstandingAmount { get; init; }

    public decimal? MaximumLateFeeAmount { get; init; }

    [MaxLength(3)]
    public string? MaximumLateFeeCurrencyCode { get; init; }

    public CollectionActionAfterGrace ActionAfterGrace { get; init; } = CollectionActionAfterGrace.NotifyOnly;
}
```

---

# 17. Payments and Payment Method Assignment

## 17.1 Key requirement

Cinturon360 must support different payment methods for different operations.

Examples:

```text
APAC Travel Policy -> Corporate Visa ending 1234
EMEA Travel Policy -> Amex ending 9876
Default platform access fee -> Mastercard ending 1111
Manual service fee invoice -> Bank transfer
BSP air settlement -> BSP settlement account
```

Do not store a single `DefaultPaymentMethodId` on the licence agreement as the only model.

## 17.2 PaymentProvider

```csharp
public enum PaymentProvider
{
    None = 0,
    Stripe = 1,
    BankTransfer = 2,
    ManualInvoice = 3,
    BspSettlement = 4,
    Airwallex = 5,
    Adyen = 6
}
```

## 17.3 PaymentMethodKind

```csharp
public enum PaymentMethodKind
{
    Unknown = 0,
    Card = 1,
    BankAccount = 2,
    InvoiceOnly = 3,
    Wallet = 4,
    BspSettlement = 5
}
```

## 17.4 PaymentOperationType

```csharp
public enum PaymentOperationType
{
    PlatformAccessFee = 1,
    BookingPayment = 2,
    ServiceFee = 3,
    CancellationFee = 4,
    RefundAdjustment = 5,
    CreditTopUp = 6,
    ManualInvoice = 7,
    SupplierSettlement = 8,
    BspSettlement = 9
}
```

## 17.5 OrganizationPaymentProfile

```csharp
using System.ComponentModel.DataAnnotations;

namespace Cinturon360.Domain.Billing.Payments;

public sealed class OrganizationPaymentProfile
{
    [Key]
    [MaxLength(18)]
    public required string Id { get; init; }

    [Required]
    [MaxLength(18)]
    public required string OrganizationId { get; init; }

    public PaymentProvider Provider { get; init; }

    [MaxLength(200)]
    public required string ProviderCustomerId { get; init; }

    public bool IsActive { get; set; } = true;

    public DateTimeOffset CreatedAtUtc { get; init; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
}
```

## 17.6 OrganizationPaymentMethod

```csharp
using System.ComponentModel.DataAnnotations;

namespace Cinturon360.Domain.Billing.Payments;

public sealed class OrganizationPaymentMethod
{
    [Key]
    [MaxLength(18)]
    public required string Id { get; init; }

    [Required]
    [MaxLength(18)]
    public required string OrganizationId { get; init; }

    [Required]
    [MaxLength(18)]
    public required string PaymentProfileId { get; init; }

    public PaymentProvider Provider { get; init; }

    [MaxLength(200)]
    public required string ProviderPaymentMethodId { get; init; }

    public PaymentMethodKind Kind { get; init; }

    public CardBrand CardBrand { get; init; } = CardBrand.Unset;

    [MaxLength(2)]
    public string? CardCountryIso2 { get; init; }

    public CardFunding CardFunding { get; init; } = CardFunding.Unset;

    [MaxLength(4)]
    public string? CardLast4 { get; init; }

    public bool IsDomestic { get; init; }

    public bool IsHighCost { get; init; }

    public bool IsAmexLike { get; init; }

    public BillingFeeTier FeeTier { get; init; } = BillingFeeTier.Unset;

    public bool IsActive { get; set; } = true;

    public DateTimeOffset CreatedAtUtc { get; init; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
}
```

Note: `CardBrand`, `CardFunding`, and `BillingFeeTier` can reuse existing v4 enums if suitable. Keep provider/card fields internal and do not expose sensitive provider metadata unnecessarily.

## 17.7 PaymentMethodAssignment

```csharp
using System.ComponentModel.DataAnnotations;

namespace Cinturon360.Domain.Billing.Payments;

public sealed class PaymentMethodAssignment
{
    [Key]
    [MaxLength(18)]
    public required string Id { get; init; }

    [Required]
    [MaxLength(18)]
    public required string OrganizationId { get; init; }

    [Required]
    [MaxLength(18)]
    public required string PaymentMethodId { get; init; }

    public PaymentOperationType OperationType { get; init; }

    [MaxLength(18)]
    public string? AgreementId { get; init; }

    [MaxLength(18)]
    public string? TravelPolicyId { get; init; }

    [MaxLength(18)]
    public string? CostCentreId { get; init; }

    [MaxLength(10)]
    public string? RegionCode { get; init; }

    public bool IsDefaultForScope { get; init; }

    public DateTimeOffset EffectiveFromUtc { get; init; }

    public DateTimeOffset? EffectiveToUtc { get; init; }
}
```

## 17.8 Payment method resolution order

When selecting a payment method for a charge, resolve from most specific to least specific:

```text
1. Exact travel policy + operation type
2. Cost centre + operation type
3. Region + operation type
4. Agreement + operation type
5. Organisation default for operation type
6. Organisation default general payment method
7. No payment method found -> fail or require manual invoice depending on billing model
```

---

# 18. Billing Account and Ledger

## 18.1 Why balance should not be on the agreement

The v4 class had:

```csharp
public decimal PrepaidBalance { get; set; }
```

This should be removed.

Prepaid balance is financial state. It should be derived from ledger entries or balance movements.

## 18.2 BillingAccount

```csharp
using System.ComponentModel.DataAnnotations;

namespace Cinturon360.Domain.Billing.Accounts;

public sealed class BillingAccount
{
    [Key]
    [MaxLength(18)]
    public required string Id { get; init; }

    [Required]
    [MaxLength(18)]
    public required string OrganizationId { get; init; }

    [MaxLength(3)]
    public string CurrencyCode { get; init; } = "AUD";

    public BillingAccountStatus Status { get; set; } = BillingAccountStatus.Active;

    public DateTimeOffset CreatedAtUtc { get; init; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
}
```

## 18.3 BillingLedgerEntry

```csharp
using System.ComponentModel.DataAnnotations;

namespace Cinturon360.Domain.Billing.Accounts;

public sealed class BillingLedgerEntry
{
    [Key]
    [MaxLength(18)]
    public required string Id { get; init; }

    [Required]
    [MaxLength(18)]
    public required string BillingAccountId { get; init; }

    public BillingLedgerEntryType Type { get; init; }

    public decimal Amount { get; init; }

    [MaxLength(3)]
    public string CurrencyCode { get; init; } = "AUD";

    [MaxLength(18)]
    public string? InvoiceId { get; init; }

    [MaxLength(18)]
    public string? PaymentAttemptId { get; init; }

    [MaxLength(500)]
    public string? Description { get; init; }

    public DateTimeOffset PostedAtUtc { get; init; } = DateTimeOffset.UtcNow;
}
```

## 18.4 Ledger entry type

```csharp
public enum BillingLedgerEntryType
{
    CreditAdded = 1,
    InvoiceIssued = 2,
    PaymentReceived = 3,
    RefundIssued = 4,
    AdjustmentCredit = 5,
    AdjustmentDebit = 6,
    WriteOff = 7
}
```

## 18.5 Balance movement type

For prepaid/reservation flows:

```csharp
public enum BalanceMovementType
{
    CreditAdded = 1,
    CreditReserved = 2,
    CreditReservationReleased = 3,
    CreditConsumed = 4,
    CreditRefunded = 5,
    ManualAdjustment = 6
}
```

Prepaid systems may need:

```text
Available balance
Reserved balance
Consumed balance
```

This matters because a travel booking may reserve funds before ticketing or final supplier confirmation.

---

# 19. Invoices

## 19.1 Invoice status should not be on agreement

The v4 class had:

```csharp
public PaymentStatus PaymentStatus { get; set; }
```

This should move to invoice/payment objects.

## 19.2 InvoiceStatus

```csharp
public enum InvoiceStatus
{
    Draft = 1,
    Issued = 2,
    Cancelled = 3,
    Voided = 4
}
```

## 19.3 InvoicePaymentStatus

```csharp
public enum InvoicePaymentStatus
{
    NotPaid = 1,
    Pending = 2,
    PartiallyPaid = 3,
    Paid = 4,
    Failed = 5,
    Overdue = 6,
    WrittenOff = 7,
    Refunded = 8,
    PartiallyRefunded = 9
}
```

## 19.4 Invoice

```csharp
using System.ComponentModel.DataAnnotations;

namespace Cinturon360.Domain.Billing.Invoicing;

public sealed class Invoice
{
    [Key]
    [MaxLength(18)]
    public required string Id { get; init; }

    [Required]
    [MaxLength(18)]
    public required string OrganizationId { get; init; }

    [MaxLength(18)]
    public string? AgreementId { get; init; }

    public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft;

    public InvoicePaymentStatus PaymentStatus { get; set; } = InvoicePaymentStatus.NotPaid;

    public DateOnly IssueDate { get; init; }

    public DateOnly DueDate { get; init; }

    public decimal SubtotalAmount { get; init; }

    public decimal TaxAmount { get; init; }

    public decimal TotalAmount { get; init; }

    [MaxLength(3)]
    public string CurrencyCode { get; init; } = "AUD";

    public DateTimeOffset CreatedAtUtc { get; init; } = DateTimeOffset.UtcNow;
}
```

## 19.5 InvoiceLine

```csharp
using System.ComponentModel.DataAnnotations;

namespace Cinturon360.Domain.Billing.Invoicing;

public sealed class InvoiceLine
{
    [Key]
    [MaxLength(18)]
    public required string Id { get; init; }

    [Required]
    [MaxLength(18)]
    public required string InvoiceId { get; init; }

    [MaxLength(18)]
    public string? AgreementFeeRuleId { get; init; }

    public BillingChargeCategory ChargeCategory { get; init; }

    [MaxLength(500)]
    public required string Description { get; init; }

    public decimal Quantity { get; init; } = 1m;

    public decimal UnitAmount { get; init; }

    public decimal SubtotalAmount { get; init; }

    public decimal TaxRatePercent { get; init; }

    public decimal TaxAmount { get; init; }

    public decimal TotalAmount { get; init; }

    [MaxLength(3)]
    public string CurrencyCode { get; init; } = "AUD";
}
```

---

# 20. Tax

## 20.1 Why `TaxRate` should not be a single field on agreement

The v4 class had:

```csharp
public decimal TaxRate { get; set; }
```

This is too simple.

Tax depends on:

- country;
- region;
- GST/VAT rules;
- reverse charge rules;
- tax registration;
- taxable/exempt/out-of-scope treatment;
- invoice date;
- tax jurisdiction;
- line item type.

Use a tax profile and snapshot calculated tax onto invoice lines.

## 20.2 TaxTreatment

```csharp
public enum TaxTreatment
{
    Unknown = 0,
    Taxable = 1,
    GstIncluded = 2,
    GstExclusive = 3,
    VatIncluded = 4,
    VatExclusive = 5,
    ReverseCharge = 6,
    Exempt = 7,
    OutOfScope = 8
}
```

## 20.3 AgreementTaxProfile

```csharp
using System.ComponentModel.DataAnnotations;

namespace Cinturon360.Domain.Billing.Tax;

public sealed class AgreementTaxProfile
{
    [Key]
    [MaxLength(18)]
    public required string Id { get; init; }

    [Required]
    [MaxLength(18)]
    public required string AgreementId { get; init; }

    [MaxLength(2)]
    public required string CountryCode { get; init; }

    [MaxLength(50)]
    public string? TaxRegistrationNumber { get; init; }

    public TaxTreatment TaxTreatment { get; init; }

    public decimal DefaultTaxRatePercent { get; init; }

    public DateOnly EffectiveFrom { get; init; }

    public DateOnly? EffectiveTo { get; init; }
}
```

---

# 21. Accounting Compatibility

## 21.1 Key accounting rule

Billing must be designed so that every financial movement can eventually produce debit and credit records.

The sequence should be:

```text
Agreement
  -> Billing run / usage event
  -> Invoice
  -> Invoice lines
  -> Journal entries
  -> Payment attempt / receipt
  -> Settlement / reconciliation
  -> Export to accounting system
```

## 21.2 Postpaid invoice accounting

Example invoice:

```text
Invoice total: AUD 110.00
Revenue: AUD 100.00
GST: AUD 10.00
```

Journal on invoice issue:

| Account | DR | CR |
|---|---:|---:|
| Accounts Receivable | 110.00 | |
| Platform Revenue | | 100.00 |
| GST Payable | | 10.00 |

Payment received by card:

| Account | DR | CR |
|---|---:|---:|
| Stripe Clearing | 110.00 | |
| Accounts Receivable | | 110.00 |

Stripe fee charged, AUD 2.00:

| Account | DR | CR |
|---|---:|---:|
| Merchant Fees Expense | 2.00 | |
| Stripe Clearing | | 2.00 |

Stripe payout to bank, AUD 108.00:

| Account | DR | CR |
|---|---:|---:|
| Bank | 108.00 | |
| Stripe Clearing | | 108.00 |

## 21.3 Prepaid accounting

When customer loads balance:

| Account | DR | CR |
|---|---:|---:|
| Bank / Stripe Clearing | 1,100.00 | |
| Customer Credits Liability | | 1,100.00 |

When customer consumes AUD 110 including GST:

| Account | DR | CR |
|---|---:|---:|
| Customer Credits Liability | 110.00 | |
| Platform Revenue | | 100.00 |
| GST Payable | | 10.00 |

Important: prepaid funds are generally not revenue until consumed. They are a customer credit/deferred revenue/liability until the service is provided.

## 21.4 PayAsYouGo accounting

Immediate charge example:

| Account | DR | CR |
|---|---:|---:|
| Stripe Clearing / Bank | 110.00 | |
| Platform Revenue | | 100.00 |
| GST Payable | | 10.00 |

If the PAYG transaction is invoiced first and collected after, treat like postpaid invoice/payment flow.

---

# 22. Accounting Domain Model

## 22.1 LedgerAccountType

```csharp
public enum LedgerAccountType
{
    Asset = 1,
    Liability = 2,
    Equity = 3,
    Revenue = 4,
    Expense = 5
}
```

## 22.2 LedgerAccount

```csharp
using System.ComponentModel.DataAnnotations;

namespace Cinturon360.Domain.Billing.Accounting;

public sealed class LedgerAccount
{
    [Key]
    [MaxLength(18)]
    public required string Id { get; init; }

    [MaxLength(50)]
    public required string Code { get; init; }

    [MaxLength(200)]
    public required string Name { get; init; }

    public LedgerAccountType Type { get; init; }

    public bool IsActive { get; set; } = true;
}
```

Example chart of accounts:

```text
1000 Bank
1010 Stripe Clearing
1100 Accounts Receivable
2100 Customer Credits Liability
2200 GST Payable
4000 Platform Access Revenue
4010 Booking Service Fee Revenue
4020 Markup Revenue
5000 Merchant Fees Expense
```

## 22.3 DebitCredit

```csharp
public enum DebitCredit
{
    Debit = 1,
    Credit = 2
}
```

## 22.4 JournalEntryStatus

```csharp
public enum JournalEntryStatus
{
    Draft = 1,
    Posted = 2,
    Reversed = 3,
    Exported = 4
}
```

## 22.5 JournalEntry

```csharp
using System.ComponentModel.DataAnnotations;

namespace Cinturon360.Domain.Billing.Accounting;

public sealed class JournalEntry
{
    [Key]
    [MaxLength(18)]
    public required string Id { get; init; }

    [Required]
    [MaxLength(18)]
    public required string JournalBatchId { get; init; }

    public DateOnly AccountingDate { get; init; }

    [MaxLength(100)]
    public required string SourceSystem { get; init; }

    [MaxLength(100)]
    public required string SourceRecordType { get; init; }

    [MaxLength(18)]
    public required string SourceRecordId { get; init; }

    public JournalEntryStatus Status { get; set; } = JournalEntryStatus.Draft;

    [MaxLength(1000)]
    public string? Description { get; init; }

    public DateTimeOffset CreatedAtUtc { get; init; } = DateTimeOffset.UtcNow;

    public List<JournalLine> Lines { get; init; } = [];
}
```

## 22.6 JournalLine

```csharp
using System.ComponentModel.DataAnnotations;

namespace Cinturon360.Domain.Billing.Accounting;

public sealed class JournalLine
{
    [Key]
    [MaxLength(18)]
    public required string Id { get; init; }

    [Required]
    [MaxLength(18)]
    public required string JournalEntryId { get; init; }

    [Required]
    [MaxLength(18)]
    public required string LedgerAccountId { get; init; }

    public DebitCredit DebitCredit { get; init; }

    public decimal Amount { get; init; }

    [MaxLength(3)]
    public required string CurrencyCode { get; init; }

    [MaxLength(18)]
    public string? OrganizationId { get; init; }

    [MaxLength(18)]
    public string? AgreementId { get; init; }

    [MaxLength(18)]
    public string? InvoiceId { get; init; }

    [MaxLength(18)]
    public string? PaymentId { get; init; }

    [MaxLength(50)]
    public string? TaxCode { get; init; }

    [MaxLength(1000)]
    public string? Description { get; init; }
}
```

## 22.7 Balance validation

```csharp
public static class JournalEntryValidator
{
    public static bool IsBalanced(IEnumerable<JournalLine> lines, int decimalPlaces = 2)
    {
        var debitTotal = lines
            .Where(x => x.DebitCredit == DebitCredit.Debit)
            .Sum(x => Math.Round(x.Amount, decimalPlaces));

        var creditTotal = lines
            .Where(x => x.DebitCredit == DebitCredit.Credit)
            .Sum(x => Math.Round(x.Amount, decimalPlaces));

        return debitTotal == creditTotal;
    }
}
```

---

# 23. Accounting Export Mapping

## 23.1 Purpose

Do not hardcode external accounting system account codes into invoices or billing records.

Use mapping tables.

## 23.2 AccountingProvider

```csharp
public enum AccountingProvider
{
    GenericCsv = 1,
    Xero = 2,
    Myob = 3,
    QuickBooks = 4,
    NetSuite = 5,
    Dynamics365BusinessCentral = 6
}
```

## 23.3 ExternalAccountingAccountMapping

```csharp
using System.ComponentModel.DataAnnotations;

namespace Cinturon360.Domain.Billing.Accounting;

public sealed class ExternalAccountingAccountMapping
{
    [Key]
    [MaxLength(18)]
    public required string Id { get; init; }

    [Required]
    [MaxLength(18)]
    public required string OrganizationId { get; init; }

    public AccountingProvider Provider { get; init; }

    [Required]
    [MaxLength(18)]
    public required string InternalLedgerAccountId { get; init; }

    [MaxLength(100)]
    public required string ExternalAccountCode { get; init; }

    [MaxLength(100)]
    public string? ExternalTaxCode { get; init; }

    public bool IsActive { get; set; } = true;
}
```

Example:

```text
Internal account: 4000 Platform Access Revenue
Xero account: 200
MYOB account: 4-1000
NetSuite account: 4010
```

---

# 24. Financial Precision Rules

All financial decimal fields must have explicit database precision.

Recommended precision:

| Use | Precision |
|---|---:|
| Money | `decimal(18,4)` |
| Percentage | `decimal(9,6)` |
| Exchange rate | `decimal(19,10)` |
| Quantity | `decimal(18,4)` |
| Tax rate | `decimal(9,6)` |

Do not rely on provider defaults.

## EF examples

```csharp
builder.Property(x => x.AccessFeeAmount)
    .HasPrecision(18, 4);

builder.Property(x => x.PostpaidCreditLimitAmount)
    .HasPrecision(18, 4);

builder.Property(x => x.Percent)
    .HasPrecision(9, 6);

builder.Property(x => x.TaxRatePercent)
    .HasPrecision(9, 6);
```

---

# 25. EF Core Configuration Examples

## 25.1 LicenseAgreementConfiguration

```csharp
using Cinturon360.Domain.Billing.Agreements;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cinturon360.Infrastructure.Persistence.Configurations.Billing;

public sealed class LicenseAgreementConfiguration : IEntityTypeConfiguration<LicenseAgreement>
{
    public void Configure(EntityTypeBuilder<LicenseAgreement> builder)
    {
        builder.ToTable("license_agreements", "billing");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasMaxLength(18)
            .IsRequired();

        builder.Property(x => x.LicensedOrganizationId)
            .HasMaxLength(18)
            .IsRequired();

        builder.Property(x => x.IssuedByOrganizationId)
            .HasMaxLength(18)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(40)
            .IsRequired();

        builder.Property(x => x.AgreementKind)
            .HasConversion<string>()
            .HasMaxLength(60)
            .IsRequired();

        builder.Property(x => x.PreviousAgreementId)
            .HasMaxLength(18);

        builder.Property(x => x.SupersededByAgreementId)
            .HasMaxLength(18);

        builder.Property(x => x.AcceptedByUserId)
            .HasMaxLength(18);

        builder.Property(x => x.AcceptedByEmail)
            .HasMaxLength(320);

        builder.Property(x => x.AcceptedFromIpAddress)
            .HasMaxLength(100);

        builder.Property(x => x.AcceptedUserAgent)
            .HasMaxLength(500);

        builder.Property(x => x.CreatedByUserId)
            .HasMaxLength(18);

        builder.Property(x => x.UpdatedByUserId)
            .HasMaxLength(18);

        builder.HasMany(x => x.FeeRules)
            .WithOne()
            .HasForeignKey(x => x.AgreementId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.AdjustmentRules)
            .WithOne()
            .HasForeignKey(x => x.AgreementId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Entitlements)
            .WithOne()
            .HasForeignKey(x => x.AgreementId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
```

## 25.2 AgreementBillingProfileConfiguration

```csharp
using Cinturon360.Domain.Billing.Agreements;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cinturon360.Infrastructure.Persistence.Configurations.Billing;

public sealed class AgreementBillingProfileConfiguration : IEntityTypeConfiguration<AgreementBillingProfile>
{
    public void Configure(EntityTypeBuilder<AgreementBillingProfile> builder)
    {
        builder.ToTable("agreement_billing_profiles", "billing");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasMaxLength(18)
            .IsRequired();

        builder.Property(x => x.AgreementId)
            .HasMaxLength(18)
            .IsRequired();

        builder.Property(x => x.BillingModel)
            .HasConversion<string>()
            .HasMaxLength(40)
            .IsRequired();

        builder.Property(x => x.AccessFeeBillingPeriod)
            .HasConversion<string>()
            .HasMaxLength(40)
            .IsRequired();

        builder.Property(x => x.PaymentTerms)
            .HasConversion<string>()
            .HasMaxLength(40)
            .IsRequired();

        builder.Property(x => x.AccessFeeAmount)
            .HasPrecision(18, 4);

        builder.Property(x => x.AccessFeeCurrencyCode)
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(x => x.MinimumSpendAmount)
            .HasPrecision(18, 4);

        builder.Property(x => x.MinimumSpendCurrencyCode)
            .HasMaxLength(3);

        builder.Property(x => x.PostpaidCreditLimitAmount)
            .HasPrecision(18, 4);

        builder.Property(x => x.PostpaidCreditLimitCurrencyCode)
            .HasMaxLength(3);
    }
}
```

## 25.3 AgreementFeeRuleConfiguration

```csharp
using Cinturon360.Domain.Billing.Agreements;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cinturon360.Infrastructure.Persistence.Configurations.Billing;

public sealed class AgreementFeeRuleConfiguration : IEntityTypeConfiguration<AgreementFeeRule>
{
    public void Configure(EntityTypeBuilder<AgreementFeeRule> builder)
    {
        builder.ToTable("agreement_fee_rules", "billing");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasMaxLength(18)
            .IsRequired();

        builder.Property(x => x.AgreementId)
            .HasMaxLength(18)
            .IsRequired();

        builder.Property(x => x.ProductType)
            .HasConversion<string>()
            .HasMaxLength(60)
            .IsRequired();

        builder.Property(x => x.Trigger)
            .HasConversion<string>()
            .HasMaxLength(80)
            .IsRequired();

        builder.Property(x => x.FeeType)
            .HasConversion<string>()
            .HasMaxLength(60)
            .IsRequired();

        builder.Property(x => x.FixedAmount)
            .HasPrecision(18, 4);

        builder.Property(x => x.FixedAmountCurrencyCode)
            .HasMaxLength(3);

        builder.Property(x => x.Percent)
            .HasPrecision(9, 6);

        builder.Property(x => x.Description)
            .HasMaxLength(500);
    }
}
```

---

# 26. Migration from v4 LicenseAgreementUnified

## 26.1 v4 field mapping

| v4 field | v5 destination |
|---|---|
| `Id` | `LicenseAgreement.Id` |
| `OrganizationUnifiedId` | `LicenseAgreement.LicensedOrganizationId` |
| `CreatedByOrganizationUnifiedId` | `LicenseAgreement.IssuedByOrganizationId` |
| `StartDate` | `LicenseAgreement.StartsOn` |
| `ExpiryDate` | `LicenseAgreement.EndsOn` |
| `RenewalDate` | `LicenseAgreement.RenewsOn` |
| `AutoRenew` | `LicenseAgreement.AutoRenew` |
| `TrialEndsOnUtc` | `LicenseAgreement.TrialEndsAtUtc` |
| `BillingType` | `AgreementBillingProfile.BillingModel` |
| `BillingFrequency` | `AgreementBillingProfile.AccessFeeBillingPeriod` or invoice frequency equivalent |
| `PaymentTerms` | `AgreementBillingProfile.PaymentTerms` / `AgreementCollectionPolicy.PaymentTerms` |
| `AccessFee` | `AgreementBillingProfile.AccessFeeAmount` |
| `AccessFeeScope` | `AgreementBillingProfile.AccessFeeBillingPeriod` |
| `AccountThreshold` | `AgreementBillingProfile.PostpaidCreditLimitAmount` or threshold profile |
| `ThresholdScope` | threshold period if retained |
| `MinimumMonthlySpend` | `AgreementBillingProfile.MinimumSpendAmount` |
| `GracePeriodDays` | `AgreementCollectionPolicy.GracePeriodDays` |
| `TaxRate` | `AgreementTaxProfile.DefaultTaxRatePercent` and invoice line tax snapshots |
| `PaymentStatus` | `Invoice.PaymentStatus`, not agreement |
| `PrepaidBalance` | billing ledger/balance records, not agreement |

## 26.2 Fee field mapping

| v4 field(s) | v5 destination |
|---|---|
| `PnrCreationFee` | `AgreementFeeRule` with `Trigger = PnrCreated` |
| `PnrChangeFee` | `AgreementFeeRule` with `Trigger = PnrChanged` |
| `FlightMarkupPercent`, `FlightPerItemFee`, `FlightFeeType` | `AgreementFeeRule` with `ProductType = Flight` |
| `HotelMarkupPercent`, `HotelPerItemFee`, `HotelFeeType` | `AgreementFeeRule` with `ProductType = Hotel` |
| `CarMarkupPercent`, `CarPerItemFee`, `CarFeeType` | `AgreementFeeRule` with `ProductType = Car` |
| `RailMarkupPercent`, `RailPerItemFee`, `RailFeeType` | `AgreementFeeRule` with `ProductType = Rail` |
| `TransferMarkupPercent`, `TransferPerItemFee`, `TransferFeeType` | `AgreementFeeRule` with `ProductType = Transfer` |
| `ActivityMarkupPercent`, `ActivityPerItemFee`, `ActivityFeeType` | `AgreementFeeRule` with `ProductType = Activity` |
| `TravelMarkupPercent`, `TravelPerItemFee`, `TravelFeeType` | `AgreementFeeRule` with `ProductType = GenericTravel` |

## 26.3 Payment field mapping

| v4 field | v5 destination |
|---|---|
| `StripeCustomerId` | `OrganizationPaymentProfile.ProviderCustomerId` |
| `StripeAccountId` | payment provider/account configuration, not licence agreement |
| `DefaultPaymentMethodId` | `OrganizationPaymentMethod.ProviderPaymentMethodId` and `PaymentMethodAssignment` |
| `PaymentCardBrand` | `OrganizationPaymentMethod.CardBrand` |
| `CardCountry` | `OrganizationPaymentMethod.CardCountryIso2` |
| `PaymentCardFunding` | `OrganizationPaymentMethod.CardFunding` |
| `IsDomesticCard` | `OrganizationPaymentMethod.IsDomestic` |
| `IsHighCostCard` | `OrganizationPaymentMethod.IsHighCost` |
| `IsAmexLikeCard` | `OrganizationPaymentMethod.IsAmexLike` |
| `PaymentBillingFeeTier` | `OrganizationPaymentMethod.FeeTier` |
| `CardLast4` | `OrganizationPaymentMethod.CardLast4` |
| `CardFingerPrint` | secure provider fingerprint reference if needed |
| `CardUpdatedAtUtc` | `OrganizationPaymentMethod.UpdatedAtUtc` |

---

# 27. Required Build Instructions for AI Agent

## 27.1 Replace v4 unified model gradually

Do not attempt to keep expanding `LicenseAgreementUnified`.

Instead:

1. Create new v5 billing domain models.
2. Map old fields into new models.
3. Keep compatibility code only where required.
4. Build billing/invoice/payment logic against new models.
5. Avoid using the agreement as a balance store.

## 27.2 Build order

Recommended implementation order:

```text
1. Create enums
2. Create LicenseAgreement root
3. Create AgreementBillingProfile
4. Create EntitlementType and entitlement metadata
5. Create LicenseAgreementEntitlement
6. Create AgreementFeeRule
7. Create AgreementAdjustmentRule
8. Create AgreementCollectionPolicy
9. Create OrganizationPaymentProfile
10. Create OrganizationPaymentMethod
11. Create PaymentMethodAssignment
12. Create BillingAccount
13. Create BillingLedgerEntry
14. Create Invoice and InvoiceLine
15. Create basic accounting journal models
16. Add EF Core configurations
17. Add seed data for entitlement definitions
18. Add migration logic from v4 model if required
```

## 27.3 Non-negotiable rules

- Do not store current prepaid balance on the agreement.
- Do not store current invoice payment status on the agreement.
- Do not store card/provider data directly on the agreement.
- Do not create one column per travel product fee type.
- Do not hardcode Xero/MYOB/NetSuite account codes into invoice lines.
- Do not create accounting exports directly from agreement rows.
- Use explicit decimal precision for money, percentages, rates, and quantities.
- Use `DateOnly` for commercial dates.
- Use `DateTimeOffset` for timestamps/events.
- Keep enums PascalCase, not uppercase snake case.
- Store currency codes as ISO-style 3-character strings such as `AUD`, `USD`, `NZD`, `EUR`, `GBP`.

## 27.4 Expected future support

This design should support future features including:

- prepaid wallet;
- postpaid credit account;
- PAYG charging;
- multiple payment methods per organisation;
- payment method assignment by travel policy/region/cost centre;
- Stripe integration;
- bank transfer/manual invoice;
- BSP settlement;
- supplier settlement;
- invoice generation;
- invoice payment status;
- payment reconciliation;
- GST/VAT/tax snapshots;
- DR/CR accounting journals;
- Stripe clearing account;
- merchant fee expense;
- customer credits liability;
- export to Xero/MYOB/NetSuite/Business Central;
- audit-safe financial history;
- entitlement-based navigation;
- entitlement-based API access;
- module packaging;
- enterprise reporting.

---

# 28. Summary

The preferred v5 split is:

```csharp
public enum BillingModel
{
    Prepaid = 1,
    Postpaid = 2,
    PayAsYouGo = 3
}

public enum BillingAccountStatus
{
    Active = 1,
    Suspended = 2,
    Closed = 3,
    UnderReview = 4
}
```

Use grouped entitlements:

```text
100-199     Organisation / tenant capacity
200-299     User / identity / access
300-399     Traveller profile / personal travel data
400-499     Booking / travel operations
500-599     Public / search / marketplace
600-699     API / integration / automation
700-799     Reporting / analytics / finance visibility
800-899     Support / service desk / operational tools
900-999     Security / compliance / audit
1000-1099   Billing / accountancy / settlement
1100-1199   AI / automation / assistant features
1200-1299   Data retention / storage / documents
9000-9999   Internal / experimental / migration
```

Use the agreement for commercial rules.

Use invoices, payments, ledgers, and journals for financial truth.

Use payment profiles and payment method assignments for Stripe/payment provider details.

Use entitlement definitions and agreement entitlement rows for modules, capabilities, and limits.

Use accounting journal entries for future DR/CR compatible financial exports.

