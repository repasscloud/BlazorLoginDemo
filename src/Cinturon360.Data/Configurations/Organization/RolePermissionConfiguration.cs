using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Cinturon360.Domain.Entities.Organization;

namespace Cinturon360.Data.Configurations.Organization;

public sealed class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> builder)
    {
        builder.ToTable("role_permissions");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasMaxLength(50);

        builder.Property(x => x.RoleId).IsRequired().HasMaxLength(50);
        builder.Property(x => x.PermissionCode).IsRequired().HasMaxLength(100);

        builder.HasIndex(x => x.RoleId);
        builder.HasIndex(x => new { x.RoleId, x.PermissionCode }).IsUnique();

        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt).IsRequired();
    }
}
