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
