using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Cinturon360.Domain.Entities.Identity;

namespace Cinturon360.Data.Configurations.Identity;

public sealed class TravellerProfileConfiguration : IEntityTypeConfiguration<TravellerProfile>
{
    public void Configure(EntityTypeBuilder<TravellerProfile> builder)
    {
        builder.ToTable("traveller_profiles");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasMaxLength(50);
        builder.Property(x => x.UserId).IsRequired().HasMaxLength(50);
        builder.Property(x => x.PassportNumber).HasMaxLength(50);
        builder.Property(x => x.PassportCountry).HasMaxLength(3);
        builder.Property(x => x.Nationality).HasMaxLength(3);
        builder.Property(x => x.Gender).HasMaxLength(20);
        builder.Property(x => x.TsaPreCheckNumber).HasMaxLength(50);
        builder.Property(x => x.GlobalEntryNumber).HasMaxLength(50);
        builder.Property(x => x.RedressNumber).HasMaxLength(50);
        builder.Property(x => x.PreferredSeatType).HasMaxLength(50);
        builder.Property(x => x.PreferredMealType).HasMaxLength(50);
        builder.Property(x => x.VipLevel).HasMaxLength(50);
        builder.Property(x => x.DedicatedConsultantUserId).HasMaxLength(50);
        builder.HasIndex(x => x.UserId).IsUnique();
    }
}

public sealed class TravellerLoyaltyProgramConfiguration : IEntityTypeConfiguration<TravellerLoyaltyProgram>
{
    public void Configure(EntityTypeBuilder<TravellerLoyaltyProgram> builder)
    {
        builder.ToTable("traveller_loyalty_programs");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasMaxLength(50);
        builder.Property(x => x.UserId).IsRequired().HasMaxLength(50);
        builder.Property(x => x.ProgramCode).IsRequired().HasMaxLength(50);
        builder.Property(x => x.ProgramName).IsRequired().HasMaxLength(200);
        builder.Property(x => x.MembershipNumber).IsRequired().HasMaxLength(100);
        builder.Property(x => x.TierName).HasMaxLength(100);
        builder.HasIndex(x => new { x.UserId, x.ProgramCode }).IsUnique();
    }
}

public sealed class UserEmergencyContactConfiguration : IEntityTypeConfiguration<UserEmergencyContact>
{
    public void Configure(EntityTypeBuilder<UserEmergencyContact> builder)
    {
        builder.ToTable("user_emergency_contacts");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasMaxLength(50);
        builder.Property(x => x.UserId).IsRequired().HasMaxLength(50);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Relationship).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Phone).IsRequired().HasMaxLength(50);
        builder.Property(x => x.Email).HasMaxLength(255);
        builder.HasIndex(x => x.UserId);
    }
}

public sealed class UserAddressConfiguration : IEntityTypeConfiguration<UserAddress>
{
    public void Configure(EntityTypeBuilder<UserAddress> builder)
    {
        builder.ToTable("user_addresses");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasMaxLength(50);
        builder.Property(x => x.UserId).IsRequired().HasMaxLength(50);
        builder.Property(x => x.AddressType).IsRequired().HasMaxLength(50);
        builder.Property(x => x.Line1).IsRequired().HasMaxLength(300);
        builder.Property(x => x.Line2).HasMaxLength(300);
        builder.Property(x => x.City).IsRequired().HasMaxLength(100);
        builder.Property(x => x.StateProvince).HasMaxLength(100);
        builder.Property(x => x.PostalCode).IsRequired().HasMaxLength(20);
        builder.Property(x => x.CountryCode).IsRequired().HasMaxLength(3);
        builder.HasIndex(x => x.UserId);
    }
}

public sealed class UserPreferencesConfiguration : IEntityTypeConfiguration<UserPreferences>
{
    public void Configure(EntityTypeBuilder<UserPreferences> builder)
    {
        builder.ToTable("user_preferences");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasMaxLength(50);
        builder.Property(x => x.UserId).IsRequired().HasMaxLength(50);
        builder.Property(x => x.PreferredAirportCode).HasMaxLength(10);
        builder.Property(x => x.PreferredAirlineCode).HasMaxLength(10);
        builder.Property(x => x.PreferredHotelChain).HasMaxLength(100);
        builder.Property(x => x.PreferredCarRentalCompany).HasMaxLength(100);
        builder.HasIndex(x => x.UserId).IsUnique();
    }
}
