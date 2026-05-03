using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Cinturon360.Domain.Entities.Billing;

namespace Cinturon360.Data.Configurations.Billing;

public sealed class OrgLicenseConfiguration : IEntityTypeConfiguration<OrgLicense>
{
    public void Configure(EntityTypeBuilder<OrgLicense> builder)
    {
        builder.ToTable("org_licenses");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasMaxLength(50);
        builder.Property(x => x.OrgId).IsRequired().HasMaxLength(50);
        builder.Property(x => x.LicenseType).HasConversion<int>();
        builder.Property(x => x.BillingCycle).HasConversion<int>();
        builder.Property(x => x.StripeSubscriptionId).HasMaxLength(200);
        builder.HasIndex(x => x.OrgId);
    }
}

public sealed class OrgBillingConfigConfiguration : IEntityTypeConfiguration<OrgBillingConfig>
{
    public void Configure(EntityTypeBuilder<OrgBillingConfig> builder)
    {
        builder.ToTable("org_billing_configs");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasMaxLength(50);
        builder.Property(x => x.OrgId).IsRequired().HasMaxLength(50);
        builder.Property(x => x.StripeCustomerId).HasMaxLength(200);
        builder.Property(x => x.BillingEmail).HasMaxLength(255);
        builder.Property(x => x.BillingName).HasMaxLength(200);
        builder.Property(x => x.CurrencyCode).HasMaxLength(3).HasDefaultValue("USD");
        builder.Property(x => x.VatNumber).HasMaxLength(50);
        builder.HasIndex(x => x.OrgId).IsUnique();
    }
}

public sealed class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.ToTable("invoices");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasMaxLength(50);
        builder.Property(x => x.OrgId).IsRequired().HasMaxLength(50);
        builder.Property(x => x.InvoiceNumber).IsRequired().HasMaxLength(50);
        builder.Property(x => x.Status).HasConversion<int>();
        builder.Property(x => x.SubtotalAmount).HasPrecision(18, 4);
        builder.Property(x => x.TaxAmount).HasPrecision(18, 4);
        builder.Property(x => x.TotalAmount).HasPrecision(18, 4);
        builder.Property(x => x.CurrencyCode).HasMaxLength(3).HasDefaultValue("USD");
        builder.Property(x => x.StripeInvoiceId).HasMaxLength(200);
        builder.Property(x => x.Notes).HasMaxLength(2000);
        builder.HasIndex(x => x.OrgId);
        builder.HasIndex(x => x.InvoiceNumber).IsUnique();
    }
}

public sealed class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("payments");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasMaxLength(50);
        builder.Property(x => x.OrgId).IsRequired().HasMaxLength(50);
        builder.Property(x => x.InvoiceId).HasMaxLength(50);
        builder.Property(x => x.Status).HasConversion<int>();
        builder.Property(x => x.Method).HasConversion<int>();
        builder.Property(x => x.Amount).HasPrecision(18, 4);
        builder.Property(x => x.CurrencyCode).HasMaxLength(3).HasDefaultValue("USD");
        builder.Property(x => x.StripePaymentIntentId).HasMaxLength(200);
        builder.Property(x => x.StripeChargeId).HasMaxLength(200);
        builder.Property(x => x.FailureReason).HasMaxLength(1000);
        builder.HasIndex(x => x.OrgId);
        builder.HasIndex(x => x.InvoiceId);
    }
}

public sealed class PrepaidBalanceConfiguration : IEntityTypeConfiguration<PrepaidBalance>
{
    public void Configure(EntityTypeBuilder<PrepaidBalance> builder)
    {
        builder.ToTable("prepaid_balances");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasMaxLength(50);
        builder.Property(x => x.OrgId).IsRequired().HasMaxLength(50);
        builder.Property(x => x.BalanceAmount).HasPrecision(18, 4);
        builder.Property(x => x.CurrencyCode).HasMaxLength(3).HasDefaultValue("USD");
        builder.HasIndex(x => x.OrgId).IsUnique();
    }
}

public sealed class PaymentProviderConnectionConfiguration : IEntityTypeConfiguration<PaymentProviderConnection>
{
    public void Configure(EntityTypeBuilder<PaymentProviderConnection> builder)
    {
        builder.ToTable("payment_provider_connections");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasMaxLength(50);
        builder.Property(x => x.OwnerOrganisationId).IsRequired().HasMaxLength(50);
        builder.Property(x => x.ProviderType).HasConversion<int>();
        builder.Property(x => x.DisplayName).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Status).HasConversion<int>();
        builder.Property(x => x.SecretBundleReference).IsRequired().HasMaxLength(200);
        builder.Property(x => x.ProviderAccountId).HasMaxLength(200);
        builder.Property(x => x.ProviderAccountName).HasMaxLength(200);
        builder.Property(x => x.WebhookEndpointId).HasMaxLength(200);
        builder.Property(x => x.WebhookSecretReference).HasMaxLength(200);
        builder.Property(x => x.UsageScope).HasConversion<int>();
        builder.HasIndex(x => new { x.OwnerOrganisationId, x.ProviderType, x.IsEnabled });
        // Partial unique index: only one primary verified+enabled connection per owner/provider/mode/scope (Q19)
        builder.HasIndex(x => new { x.OwnerOrganisationId, x.ProviderType, x.IsLiveMode, x.UsageScope })
            .HasFilter("is_primary = true AND is_enabled = true AND status = 2")
            .IsUnique()
            .HasDatabaseName("ux_payment_provider_connection_primary");
    }
}

public sealed class BillingRelationshipConfiguration : IEntityTypeConfiguration<BillingRelationship>
{
    public void Configure(EntityTypeBuilder<BillingRelationship> builder)
    {
        builder.ToTable("billing_relationships");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasMaxLength(50);
        builder.Property(x => x.SellerOrganisationId).IsRequired().HasMaxLength(50);
        builder.Property(x => x.BuyerOrganisationId).IsRequired().HasMaxLength(50);
        builder.Property(x => x.OrgLicenseId).HasMaxLength(50);
        builder.Property(x => x.PaymentProviderConnectionId).HasMaxLength(50);
        builder.Property(x => x.BillingMode).HasConversion<int>();
        builder.Property(x => x.BillingFrequency).HasConversion<int>();
        builder.Property(x => x.CollectionMode).HasConversion<int>();
        builder.Property(x => x.ChargeTreatment).HasConversion<int>();
        builder.Property(x => x.CommercialRiskOwner).HasConversion<int>();
        builder.Property(x => x.CurrencyCode).IsRequired().HasMaxLength(3).HasDefaultValue("USD");
        builder.Property(x => x.CreditLimitAmount).HasPrecision(18, 4);
        builder.Property(x => x.MaxSingleBookingAmount).HasPrecision(18, 4);
        builder.HasIndex(x => new { x.SellerOrganisationId, x.BuyerOrganisationId }).IsUnique();
    }
}

public sealed class ProviderCustomerConfiguration : IEntityTypeConfiguration<ProviderCustomer>
{
    public void Configure(EntityTypeBuilder<ProviderCustomer> builder)
    {
        builder.ToTable("provider_customers");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasMaxLength(50);
        builder.Property(x => x.PaymentProviderConnectionId).IsRequired().HasMaxLength(50);
        builder.Property(x => x.BuyerOrganisationId).IsRequired().HasMaxLength(50);
        builder.Property(x => x.ProviderCustomerId).IsRequired().HasMaxLength(200);
        builder.HasIndex(x => new { x.PaymentProviderConnectionId, x.BuyerOrganisationId }).IsUnique();
        builder.HasIndex(x => new { x.PaymentProviderConnectionId, x.ProviderCustomerId }).IsUnique();
    }
}

public sealed class ProviderPaymentMethodConfiguration : IEntityTypeConfiguration<ProviderPaymentMethod>
{
    public void Configure(EntityTypeBuilder<ProviderPaymentMethod> builder)
    {
        builder.ToTable("provider_payment_methods");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasMaxLength(50);
        builder.Property(x => x.ProviderCustomerId).IsRequired().HasMaxLength(50);
        builder.Property(x => x.ProviderType).HasConversion<int>();
        builder.Property(x => x.ProviderPaymentMethodId).IsRequired().HasMaxLength(200);
        builder.Property(x => x.ProviderSetupIntentId).HasMaxLength(200);
        builder.Property(x => x.Purpose).HasConversion<int>();
        builder.Property(x => x.DisplayName).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Brand).HasMaxLength(50);
        builder.Property(x => x.Last4).HasMaxLength(4);
        builder.Property(x => x.CardholderName).HasMaxLength(200);
        builder.Property(x => x.Fingerprint).HasMaxLength(200);
        builder.Property(x => x.BillingCountry).HasMaxLength(10);
        builder.Property(x => x.BillingEmail).HasMaxLength(255);
        builder.HasIndex(x => new { x.ProviderCustomerId, x.ProviderPaymentMethodId }).IsUnique();
    }
}

public sealed class OrganisationBillingProfileConfiguration : IEntityTypeConfiguration<OrganisationBillingProfile>
{
    public void Configure(EntityTypeBuilder<OrganisationBillingProfile> builder)
    {
        builder.ToTable("organisation_billing_profiles");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasMaxLength(50);
        builder.Property(x => x.OrganisationId).IsRequired().HasMaxLength(50);
        builder.Property(x => x.SellerOrganisationId).IsRequired().HasMaxLength(50);
        builder.Property(x => x.PaymentProviderConnectionId).IsRequired().HasMaxLength(50);
        builder.Property(x => x.DefaultProviderCustomerId).HasMaxLength(50);
        builder.Property(x => x.DefaultProviderPaymentMethodId).HasMaxLength(50);
        builder.Property(x => x.DefaultBillingMode).HasConversion<int>();
        builder.Property(x => x.DefaultBillingFrequency).HasConversion<int>();
        builder.Property(x => x.DefaultCollectionMode).HasConversion<int>();
        builder.Property(x => x.DefaultChargeTreatment).HasConversion<int>();
        builder.Property(x => x.CurrencyCode).IsRequired().HasMaxLength(3).HasDefaultValue("USD");
        builder.HasIndex(x => x.OrganisationId).IsUnique();
    }
}

public sealed class PolicyBillingRuleConfiguration : IEntityTypeConfiguration<PolicyBillingRule>
{
    public void Configure(EntityTypeBuilder<PolicyBillingRule> builder)
    {
        builder.ToTable("policy_billing_rules");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasMaxLength(50);
        builder.Property(x => x.OrganisationId).IsRequired().HasMaxLength(50);
        builder.Property(x => x.PolicyType).HasConversion<int>();
        builder.Property(x => x.PolicyId).IsRequired().HasMaxLength(50);
        builder.Property(x => x.ResolutionMode).HasConversion<int>();
        builder.Property(x => x.ProviderPaymentMethodId).HasMaxLength(50);
        builder.Property(x => x.PaymentProviderConnectionId).HasMaxLength(50);
        builder.Property(x => x.ProviderCustomerId).HasMaxLength(50);
        builder.Property(x => x.BillingModeOverride).HasConversion<int>();
        builder.Property(x => x.CollectionModeOverride).HasConversion<int>();
        builder.HasIndex(x => new { x.OrganisationId, x.PolicyType, x.PolicyId, x.IsEnabled });
    }
}

public sealed class ProviderWebhookEventConfiguration : IEntityTypeConfiguration<ProviderWebhookEvent>
{
    public void Configure(EntityTypeBuilder<ProviderWebhookEvent> builder)
    {
        builder.ToTable("provider_webhook_events");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasMaxLength(50);
        builder.Property(x => x.PaymentProviderConnectionId).IsRequired().HasMaxLength(50);
        builder.Property(x => x.ProviderType).HasConversion<int>();
        builder.Property(x => x.ProviderEventId).IsRequired().HasMaxLength(200);
        builder.Property(x => x.EventType).IsRequired().HasMaxLength(100);
        builder.Property(x => x.RawPayloadJson).IsRequired();
        builder.Property(x => x.Status).HasConversion<int>();
        builder.Property(x => x.ErrorMessage).HasMaxLength(1000);
        builder.HasIndex(x => new { x.PaymentProviderConnectionId, x.ProviderEventId }).IsUnique();
    }
}

// ─── Q16: License Agreement Configurations ──────────────────────────────────

public sealed class LicenseAgreementConfiguration : IEntityTypeConfiguration<LicenseAgreement>
{
    public void Configure(EntityTypeBuilder<LicenseAgreement> builder)
    {
        builder.ToTable("license_agreements");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasMaxLength(50);
        builder.Property(x => x.SellerOrgId).IsRequired().HasMaxLength(50);
        builder.Property(x => x.BuyerOrgId).IsRequired().HasMaxLength(50);
        builder.Property(x => x.BillingModel).HasConversion<int>();
        builder.Property(x => x.BillingPeriod).HasConversion<int>();
        builder.Property(x => x.CollectionMode).HasConversion<int>();
        builder.Property(x => x.CurrencyCode).IsRequired().HasMaxLength(3).HasDefaultValue("AUD");
        builder.Property(x => x.AccessPackageCode).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Status).HasConversion<int>();
        builder.Property(x => x.CreditLimitAmount).HasPrecision(18, 4);
        builder.Property(x => x.PreviousLicenseAgreementId).HasMaxLength(50);
        builder.Property(x => x.SupersededByLicenseAgreementId).HasMaxLength(50);
        builder.HasIndex(x => new { x.SellerOrgId, x.BuyerOrgId, x.Status });
    }
}

public sealed class LicenseAgreementEntitlementConfiguration : IEntityTypeConfiguration<LicenseAgreementEntitlement>
{
    public void Configure(EntityTypeBuilder<LicenseAgreementEntitlement> builder)
    {
        builder.ToTable("license_agreement_entitlements");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasMaxLength(50);
        builder.Property(x => x.LicenseAgreementId).IsRequired().HasMaxLength(50);
        builder.Property(x => x.Type).HasConversion<int>();
        builder.Property(x => x.ValueKind).HasConversion<int>();
        builder.Property(x => x.NumericValue).HasPrecision(18, 4);
        builder.Property(x => x.TextValue).HasMaxLength(500);
        builder.Property(x => x.Notes).HasMaxLength(1000);
        builder.HasIndex(x => new { x.LicenseAgreementId, x.Type, x.IsActive });
    }
}

public sealed class LicenseCollectionPolicyConfiguration : IEntityTypeConfiguration<LicenseCollectionPolicy>
{
    public void Configure(EntityTypeBuilder<LicenseCollectionPolicy> builder)
    {
        builder.ToTable("license_collection_policies");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasMaxLength(50);
        builder.Property(x => x.LicenseAgreementId).IsRequired().HasMaxLength(50);
        builder.Property(x => x.ActionAfterGrace).HasConversion<int>();
        builder.HasIndex(x => x.LicenseAgreementId).IsUnique();
    }
}

public sealed class BillingAccountConfiguration : IEntityTypeConfiguration<BillingAccount>
{
    public void Configure(EntityTypeBuilder<BillingAccount> builder)
    {
        builder.ToTable("billing_accounts");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasMaxLength(50);
        builder.Property(x => x.OwnerOrgId).IsRequired().HasMaxLength(50);
        builder.Property(x => x.LicenseAgreementId).HasMaxLength(50);
        builder.Property(x => x.BillingModel).HasConversion<int>();
        builder.Property(x => x.AccountStatus).HasConversion<int>();
        builder.Property(x => x.CurrencyCode).IsRequired().HasMaxLength(3).HasDefaultValue("AUD");
        builder.HasIndex(x => x.OwnerOrgId);
    }
}

public sealed class BillingLedgerEntryConfiguration : IEntityTypeConfiguration<BillingLedgerEntry>
{
    public void Configure(EntityTypeBuilder<BillingLedgerEntry> builder)
    {
        builder.ToTable("billing_ledger_entries");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasMaxLength(50);
        builder.Property(x => x.BillingAccountId).IsRequired().HasMaxLength(50);
        builder.Property(x => x.LicenseAgreementId).HasMaxLength(50);
        builder.Property(x => x.RelatedEntityId).HasMaxLength(50);
        builder.Property(x => x.RelatedEntityType).HasMaxLength(100);
        builder.Property(x => x.Amount).HasPrecision(18, 4);
        builder.Property(x => x.CurrencyCode).IsRequired().HasMaxLength(3).HasDefaultValue("AUD");
        builder.Property(x => x.Description).IsRequired().HasMaxLength(500);
        builder.HasIndex(x => x.BillingAccountId);
    }
}

public sealed class BillingInvoiceConfiguration : IEntityTypeConfiguration<BillingInvoice>
{
    public void Configure(EntityTypeBuilder<BillingInvoice> builder)
    {
        builder.ToTable("billing_invoices");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasMaxLength(50);
        builder.Property(x => x.SellerOrgId).IsRequired().HasMaxLength(50);
        builder.Property(x => x.BuyerOrgId).IsRequired().HasMaxLength(50);
        builder.Property(x => x.LicenseAgreementId).HasMaxLength(50);
        builder.Property(x => x.InvoiceNumber).IsRequired().HasMaxLength(50);
        builder.Property(x => x.Status).HasConversion<int>();
        builder.Property(x => x.SubtotalAmount).HasPrecision(18, 4);
        builder.Property(x => x.TaxAmount).HasPrecision(18, 4);
        builder.Property(x => x.TotalAmount).HasPrecision(18, 4);
        builder.Property(x => x.CurrencyCode).IsRequired().HasMaxLength(3).HasDefaultValue("AUD");
        builder.Property(x => x.Notes).HasMaxLength(2000);
        builder.HasIndex(x => x.InvoiceNumber).IsUnique();
        builder.HasIndex(x => new { x.SellerOrgId, x.BuyerOrgId, x.Status });
    }
}

public sealed class BillingInvoiceLineConfiguration : IEntityTypeConfiguration<BillingInvoiceLine>
{
    public void Configure(EntityTypeBuilder<BillingInvoiceLine> builder)
    {
        builder.ToTable("billing_invoice_lines");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasMaxLength(50);
        builder.Property(x => x.BillingInvoiceId).IsRequired().HasMaxLength(50);
        builder.Property(x => x.Description).IsRequired().HasMaxLength(500);
        builder.Property(x => x.UnitAmount).HasPrecision(18, 4);
        builder.Property(x => x.TaxAmount).HasPrecision(18, 4);
        builder.Property(x => x.LineTotal).HasPrecision(18, 4);
        builder.Property(x => x.RelatedEntityId).HasMaxLength(50);
        builder.Property(x => x.RelatedEntityType).HasMaxLength(100);
        builder.HasIndex(x => x.BillingInvoiceId);
    }
}

public sealed class PaymentAttemptConfiguration : IEntityTypeConfiguration<PaymentAttempt>
{
    public void Configure(EntityTypeBuilder<PaymentAttempt> builder)
    {
        builder.ToTable("payment_attempts");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasMaxLength(50);
        builder.Property(x => x.BillingInvoiceId).HasMaxLength(50);
        builder.Property(x => x.BillingAccountId).HasMaxLength(50);
        builder.Property(x => x.PaymentProviderConnectionId).HasMaxLength(50);
        builder.Property(x => x.ProviderPaymentIntentId).HasMaxLength(200);
        builder.Property(x => x.Amount).HasPrecision(18, 4);
        builder.Property(x => x.CurrencyCode).IsRequired().HasMaxLength(3).HasDefaultValue("AUD");
        builder.Property(x => x.Status).HasConversion<int>();
        builder.Property(x => x.FailureReason).HasMaxLength(1000);
        builder.HasIndex(x => x.BillingInvoiceId);
    }
}

public sealed class JournalEntryConfiguration : IEntityTypeConfiguration<JournalEntry>
{
    public void Configure(EntityTypeBuilder<JournalEntry> builder)
    {
        builder.ToTable("journal_entries");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasMaxLength(50);
        builder.Property(x => x.LicenseAgreementId).HasMaxLength(50);
        builder.Property(x => x.BillingInvoiceId).HasMaxLength(50);
        builder.Property(x => x.PaymentAttemptId).HasMaxLength(50);
        builder.Property(x => x.Description).IsRequired().HasMaxLength(500);
        builder.HasIndex(x => x.BillingInvoiceId);
    }
}

public sealed class JournalLineConfiguration : IEntityTypeConfiguration<JournalLine>
{
    public void Configure(EntityTypeBuilder<JournalLine> builder)
    {
        builder.ToTable("journal_lines");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasMaxLength(50);
        builder.Property(x => x.JournalEntryId).IsRequired().HasMaxLength(50);
        builder.Property(x => x.AccountCode).IsRequired().HasMaxLength(50);
        builder.Property(x => x.AccountName).IsRequired().HasMaxLength(200);
        builder.Property(x => x.DebitAmount).HasPrecision(18, 4);
        builder.Property(x => x.CreditAmount).HasPrecision(18, 4);
        builder.Property(x => x.CurrencyCode).IsRequired().HasMaxLength(3).HasDefaultValue("AUD");
        builder.HasIndex(x => x.JournalEntryId);
    }
}
