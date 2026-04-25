using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Cinturon360.Domain.Entities.Travel;

namespace Cinturon360.Data.Configurations.Travel;

public sealed class CountryConfiguration : IEntityTypeConfiguration<Country>
{
    public void Configure(EntityTypeBuilder<Country> builder)
    {
        builder.ToTable("countries");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasMaxLength(50);
        builder.Property(x => x.IsoCode2).IsRequired().HasMaxLength(2);
        builder.Property(x => x.IsoCode3).IsRequired().HasMaxLength(3);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
        builder.Property(x => x.PhonePrefix).HasMaxLength(10);
        builder.Property(x => x.CurrencyCode).HasMaxLength(3).HasDefaultValue("USD");
        builder.HasIndex(x => x.IsoCode2).IsUnique();
        builder.HasIndex(x => x.IsoCode3).IsUnique();
    }
}

public sealed class CityConfiguration : IEntityTypeConfiguration<City>
{
    public void Configure(EntityTypeBuilder<City> builder)
    {
        builder.ToTable("cities");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasMaxLength(50);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
        builder.Property(x => x.IataCode).HasMaxLength(10);
        builder.Property(x => x.CountryCode).IsRequired().HasMaxLength(3);
        builder.Property(x => x.TimeZone).HasMaxLength(100);
        builder.HasIndex(x => x.IataCode);
        builder.HasIndex(x => x.CountryCode);
    }
}

public sealed class AirportConfiguration : IEntityTypeConfiguration<Airport>
{
    public void Configure(EntityTypeBuilder<Airport> builder)
    {
        builder.ToTable("airports");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasMaxLength(50);
        builder.Property(x => x.IataCode).IsRequired().HasMaxLength(4);
        builder.Property(x => x.IcaoCode).HasMaxLength(5);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(300);
        builder.Property(x => x.CityCode).IsRequired().HasMaxLength(10);
        builder.Property(x => x.CountryCode).IsRequired().HasMaxLength(3);
        builder.Property(x => x.TimeZone).HasMaxLength(100);
        builder.Property(x => x.Latitude).HasPrecision(9, 6);
        builder.Property(x => x.Longitude).HasPrecision(9, 6);
        builder.HasIndex(x => x.IataCode).IsUnique();
        builder.HasIndex(x => x.CityCode);
        builder.HasIndex(x => x.CountryCode);
    }
}
