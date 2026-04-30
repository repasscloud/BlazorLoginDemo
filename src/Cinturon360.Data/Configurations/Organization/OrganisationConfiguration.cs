using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Cinturon360.Domain.Entities.Organization;

namespace Cinturon360.Data.Configurations.Organization;

public sealed class OrganisationConfiguration : IEntityTypeConfiguration<Organisation>
{
    public void Configure(EntityTypeBuilder<Organisation> builder)
    {
        builder.ToTable("organisations");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasMaxLength(50);

        builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Slug).IsRequired().HasMaxLength(100);
        builder.Property(x => x.OrgType).IsRequired();
        builder.Property(x => x.ParentOrgId).HasMaxLength(50);
        builder.Property(x => x.LogoStorageKey).HasMaxLength(500);
        builder.Property(x => x.PrimaryEmail).HasMaxLength(255);
        builder.Property(x => x.PrimaryPhone).HasMaxLength(50);
        builder.Property(x => x.Website).HasMaxLength(255);
        builder.Property(x => x.LanguageCode).IsRequired().HasMaxLength(10).HasDefaultValue("en");
        builder.Property(x => x.TimeZone).IsRequired().HasMaxLength(100).HasDefaultValue("UTC");
        builder.Property(x => x.CurrencyCode).IsRequired().HasMaxLength(3).HasDefaultValue("USD");
        builder.Property(x => x.ExternalRef).HasMaxLength(200);

        // TMC chain / group codes
        builder.Property(x => x.ChainCode).HasMaxLength(50);
        builder.Property(x => x.ChainName).HasMaxLength(200);
        builder.Property(x => x.BranchCode).HasMaxLength(50);

        // Support team display name (shown on ticket replies)
        builder.Property(x => x.SupportTeamName).HasMaxLength(200);

        builder.HasIndex(x => x.ChainCode);

        builder.HasIndex(x => x.Slug).IsUnique();
        builder.HasIndex(x => x.OrgType);
        builder.HasIndex(x => x.ParentOrgId);
        builder.HasIndex(x => x.IsActive);

        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt).IsRequired();
        builder.Property(x => x.DeletedAt);
        builder.HasQueryFilter(x => x.DeletedAt == null);
    }
}
