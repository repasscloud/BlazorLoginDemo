using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Cinturon360.Domain.Entities.Organization;

namespace Cinturon360.Data.Configurations.Organization;

public sealed class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("roles");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasMaxLength(50);

        builder.Property(x => x.Name).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Description).HasMaxLength(500);
        builder.Property(x => x.OrgType).IsRequired();
        builder.Property(x => x.OrgId).HasMaxLength(50);

        builder.HasIndex(x => new { x.OrgType, x.IsSystemRole });
        builder.HasIndex(x => x.OrgId);

        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt).IsRequired();
        builder.Property(x => x.DeletedAt);
        builder.HasQueryFilter(x => x.DeletedAt == null);
    }
}
