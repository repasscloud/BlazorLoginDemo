using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Cinturon360.Common.Precision;
using Cinturon360.Domain.Entities.Identity;

namespace Cinturon360.Data.Configurations.Identity;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id).HasMaxLength(50);

        builder.Property(u => u.Email).IsRequired().HasMaxLength(256);
        builder.Property(u => u.FirstName).IsRequired().HasMaxLength(100);
        builder.Property(u => u.LastName).IsRequired().HasMaxLength(100);
        builder.Property(u => u.LanguageCode).IsRequired().HasMaxLength(10).HasDefaultValue("en");
        builder.Property(u => u.TimeZone).IsRequired().HasMaxLength(64).HasDefaultValue("UTC");
        builder.Property(u => u.CurrencyCode).IsRequired().HasMaxLength(3).HasDefaultValue("USD");
        builder.Property(u => u.AvatarStorageKey).HasMaxLength(500);
        builder.Property(u => u.HomeOrgId).HasMaxLength(50);
        builder.Property(u => u.ConcurrencyStamp).IsRequired().HasMaxLength(50);
        builder.Property(u => u.UserCategory).IsRequired();
        builder.Property(u => u.PlatformRole);

        builder.HasIndex(u => u.Email).IsUnique();
        builder.HasIndex(u => u.HomeOrgId);
        builder.HasIndex(u => u.UserCategory);
        builder.HasIndex(u => u.IsActive);

        builder.Property(u => u.CreatedAt).IsRequired();
        builder.Property(u => u.UpdatedAt).IsRequired();
        builder.Property(u => u.DeletedAt);
        builder.HasQueryFilter(u => u.DeletedAt == null);
    }
}
