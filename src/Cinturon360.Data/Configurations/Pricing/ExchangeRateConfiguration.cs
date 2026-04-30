using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Cinturon360.Domain.Entities.Pricing;

namespace Cinturon360.Data.Configurations.Pricing;

public sealed class ExchangeRateConfiguration : IEntityTypeConfiguration<ExchangeRate>
{
    public void Configure(EntityTypeBuilder<ExchangeRate> builder)
    {
        builder.ToTable("exchange_rates");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasMaxLength(50);

        builder.Property(x => x.CurrencyCode)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(x => x.CurrencyName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Rate)
            .IsRequired()
            .HasPrecision(18, 8);

        builder.Property(x => x.RateDate).IsRequired();
        builder.Property(x => x.FetchedAt).IsRequired();

        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt).IsRequired();

        // One row per currency — currency code must be unique
        builder.HasIndex(x => x.CurrencyCode).IsUnique();
        builder.HasIndex(x => x.RateDate);
    }
}
