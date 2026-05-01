using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Cinturon360.Domain.Entities.Travel;

namespace Cinturon360.Data.Configurations.Travel;

public sealed class DuffelOrgConfigurationConfiguration : IEntityTypeConfiguration<DuffelOrgConfiguration>
{
    public void Configure(EntityTypeBuilder<DuffelOrgConfiguration> builder)
    {
        builder.ToTable("duffel_org_configurations");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasMaxLength(50);

        builder.Property(x => x.OrgId)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.ApiBaseUrl)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(x => x.ApiToken)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(x => x.EnabledCapabilitiesCsv)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(x => x.EnabledSearchFunctionsCsv)
            .IsRequired()
            .HasMaxLength(4000);

        builder.Property(x => x.CorporateCodesCsv)
            .IsRequired()
            .HasMaxLength(4000);

        builder.Property(x => x.TourCodesCsv)
            .IsRequired()
            .HasMaxLength(4000);

        builder.Property(x => x.Notes).HasMaxLength(4000);
        builder.Property(x => x.UpdatedByUserId).HasMaxLength(50);

        builder.Property(x => x.IsEnabled).IsRequired();
        builder.Property(x => x.UseSandbox).IsRequired();
        builder.Property(x => x.AccessScope).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt).IsRequired();

        builder.HasIndex(x => x.OrgId).IsUnique();
        builder.HasIndex(x => x.IsEnabled);
    }
}
