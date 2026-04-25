using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Cinturon360.Domain.Entities.Identity;

namespace Cinturon360.Data.Configurations.Identity;

public sealed class UserProvisioningSourceConfiguration : IEntityTypeConfiguration<UserProvisioningSource>
{
    public void Configure(EntityTypeBuilder<UserProvisioningSource> builder)
    {
        builder.ToTable("user_provisioning_sources");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasMaxLength(50);

        builder.Property(x => x.UserId).IsRequired().HasMaxLength(50);
        builder.Property(x => x.Source).IsRequired();
        builder.Property(x => x.SourceIdentifier).HasMaxLength(500);
        builder.Property(x => x.BatchId).HasMaxLength(100);
        builder.Property(x => x.ProvisionedByUserId).HasMaxLength(50);

        builder.HasIndex(x => x.UserId);

        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt).IsRequired();
    }
}
