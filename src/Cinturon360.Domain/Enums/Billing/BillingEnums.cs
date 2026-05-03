namespace Cinturon360.Domain.Enums.Billing;

public enum LicenseType
{
    Free        = 1,
    Starter     = 2,
    Business    = 3,
    Enterprise  = 4
}

public enum BillingCycle
{
    Monthly  = 1,
    Annual   = 2
}

public enum InvoiceStatus
{
    Draft      = 1,
    Sent       = 2,
    Paid       = 3,
    Void       = 4,
    Overdue    = 5,
    Disputed   = 6
}

public enum PaymentStatus
{
    Pending    = 1,
    Processing = 2,
    Succeeded  = 3,
    Failed     = 4,
    Refunded   = 5
}

public enum PaymentMethod
{
    Card           = 1,
    BankTransfer   = 2,
    PrepaidBalance = 3,
    Crypto         = 4
}

public enum BillingMode
{
    NoCharge = 0,
    PrepaidBalance = 1,
    ImmediatePayment = 2,
    PeriodicInvoice = 3,
    ManualInvoice = 4,
    ExternalBilling = 5,
    PartnerManaged = 6
}

/// <summary>Canonical collection mode (Q16).</summary>
public enum CollectionMode
{
    None = 0,
    Manual = 1,
    Automatic = 2,
    ExternalReferenceOnly = 3
}

public enum ChargeTreatment
{
    Billable = 0,
    ShadowOnly = 1,
    Waived = 2
}

public enum BillingFrequency
{
    None = 0,
    PerTransaction = 1,
    Daily = 2,
    Weekly = 3,
    Fortnightly = 4,
    Monthly = 5,
    CustomCron = 6
}

public enum PaymentProviderType
{
    None = 0,
    Stripe = 1,
    Airwallex = 2,
    Braintree = 3,
    PayPal = 4,
    Square = 5,
    Manual = 6,
    External = 7
}

public enum ProviderConnectionStatus
{
    PendingSetup = 0,
    PendingVerification = 1,
    Verified = 2,
    FailedVerification = 3,
    Disabled = 4
}

public enum ProviderUsageScope
{
    OwnerOnly = 0,
    DirectChildren = 1,
    Descendants = 2
}

public enum CommercialRiskOwner
{
    Platform = 0,
    SellerOrganisation = 1,
    External = 2
}

public enum PaymentMethodPurpose
{
    General = 0,
    AccountFees = 1,
    TravelBookings = 2,
    TravelPolicy = 3,
    EmergencyFallback = 4
}

public enum BillingResolutionMode
{
    Default = 0,
    SpecificPaymentMethod = 1,
    SpecificBillingProfile = 2,
    Manual = 3,
    External = 4,
    NoCharge = 5
}

public enum WebhookProcessingStatus
{
    Pending = 0,
    Processed = 1,
    Failed = 2,
    Ignored = 3
}

// ─── Q16 Canonical Enums ────────────────────────────────────────────────────

/// <summary>
/// Canonical billing model for LicenseAgreement and new billing entities (Q16).
/// Controls how spend is authorised.
/// </summary>
public enum BillingModel
{
    Prepaid    = 1,
    Postpaid   = 2,
    PayAsYouGo = 3
}

/// <summary>
/// Billing account transactional state. Orthogonal to BillingModel.
/// </summary>
public enum BillingAccountStatus
{
    Active      = 1,
    Suspended   = 2,
    Closed      = 3,
    UnderReview = 4
}

/// <summary>
/// Canonical billing period for LicenseAgreement (Q16). Controls recurring billing cadence.
/// </summary>
public enum BillingPeriod
{
    None           = 0,
    PerTransaction = 1,
    Daily          = 2,
    Weekly         = 3,
    Fortnightly    = 4,
    Monthly        = 5,
    Quarterly      = 6,
    BiAnnual       = 7,
    Annual         = 8,
    Manual         = 9
}

/// <summary>
/// Policy domain type used as discriminator in PolicyBillingRule (Q18).
/// </summary>
public enum PolicyType
{
    Travel        = 1,
    Expense       = 2,
    Procurement   = 3,
    CorporateCard = 4,
    Reimbursement = 5
}

/// <summary>
/// LicenseAgreement lifecycle status (Q16).
/// </summary>
public enum LicenseAgreementStatus
{
    Draft      = 1,
    Active     = 2,
    Suspended  = 3,
    Expired    = 4,
    Superseded = 5,
    Cancelled  = 6
}

/// <summary>
/// Action taken after a buyer's grace period expires (Q16).
/// </summary>
public enum CollectionActionAfterGrace
{
    None               = 0,
    SendOverdueNotice  = 1,
    BlockBookings      = 2,
    Suspend            = 3,
    Escalate           = 4
}

/// <summary>
/// How a resolved billing instruction was sourced (Q16/Q18).
/// </summary>
public enum BillingResolutionSource
{
    OrganisationDefault = 0,
    PolicyOverride      = 1,
    TransactionOverride = 2
}

/// <summary>
/// Value type for LicenseAgreementEntitlement quantities (Q16).
/// </summary>
public enum EntitlementValueKind
{
    Boolean           = 1,
    Quantity          = 2,
    Money             = 3,
    Percentage        = 4,
    DurationDays      = 5,
    DurationMonths    = 6,
    StorageGb         = 7,
    RequestsPerPeriod = 8
}

/// <summary>
/// Entitlement type catalogue grouped by domain range (Q16).
/// Ranges: 100–199 Org/Tenant, 200–299 User/Identity, 300–399 Traveller,
/// 400–499 Booking, 500–599 Public/Search, 600–699 API/Integration,
/// 700–799 Reporting, 800–899 Support, 900–999 Security,
/// 1000–1099 Billing, 1100–1199 AI, 1200–1299 Documents, 9000–9999 Internal.
/// </summary>
public enum EntitlementType
{
    // Organisation / Tenant capacity (100–199)
    MaxOrganisations   = 100,
    MaxUsersPerOrg     = 101,
    MaxTravellersPerOrg = 102,

    // User / Identity / Access (200–299)
    SsoEnabled        = 200,
    ScimEnabled       = 201,
    MfaRequired       = 202,
    ApiAccessEnabled  = 203,
    PatEnabled        = 204,

    // Traveller Profile (300–399)
    TravellerProfileEnabled = 300,
    LoyaltyProgramsEnabled  = 301,

    // Booking / Travel Operations (400–499)
    MaxBookingsPerMonth  = 400,
    FlightBookingEnabled = 401,
    HotelBookingEnabled  = 402,
    CarBookingEnabled    = 403,
    RailBookingEnabled   = 404,
    ApprovalsEnabled     = 405,

    // Public / Search / Marketplace (500–599)
    PublicSearchEnabled  = 500,

    // API / Integration / Automation (600–699)
    WebhooksEnabled       = 600,
    ThirdPartyIntegrations = 601,
    MaxApiRequestsPerDay  = 602,

    // Reporting / Analytics / Finance (700–799)
    FinanceReportingEnabled = 700,
    AuditLogEnabled         = 701,
    ExportEnabled           = 702,

    // Support / Service Desk (800–899)
    SupportDeskEnabled    = 800,
    DedicatedConsultant   = 801,

    // Security / Compliance / Audit (900–999)
    ComplianceReportingEnabled = 900,
    DataResidencyControl       = 901,

    // Billing / Accountancy (1000–1099)
    PrepaidBalanceEnabled = 1000,
    PostpaidEnabled       = 1001,
    InvoicingEnabled      = 1002,
    CreditNotesEnabled    = 1003,
    MaxCreditLimitAud     = 1004,

    // AI / Automation / Assistant (1100–1199)
    AiAssistantEnabled    = 1100,
    AutoApprovalEnabled   = 1101,

    // Data Retention / Storage / Documents (1200–1299)
    DocumentStorageGb       = 1200,
    RetentionPeriodMonths   = 1201,
    SignedUrlDownloadEnabled = 1202,

    // Internal / Experimental / Migration (9000–9999)
    InternalTestFeature = 9000
}
