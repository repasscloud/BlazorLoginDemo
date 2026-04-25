using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Cinturon360.Domain.Entities.Identity;

namespace Cinturon360.Data.Configurations.Identity;

public sealed class UserRoleAssignmentConfiguration : IEntityTypeConfiguration<UserRoleAssignment>
{
    public void Configure(EntityTypeBuilder<UserRoleAssignment> builder)
    {
        builder.ToTable("user_role_assignments");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasMaxLength(50);

        builder.Property(x => x.UserId).IsRequired().HasMaxLength(50);
        builder.Property(x => x.OrgId).IsRequired().HasMaxLength(50);
        builder.Property(x => x.RoleId).IsRequired().HasMaxLength(50);
        builder.Property(x => x.ScopeMode).IsRequired();
        builder.Property(x => x.GrantedByUserId).HasMaxLength(50);

        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => new { x.UserId, x.OrgId });
        builder.HasIndex(x => new { x.UserId, x.OrgId, x.RoleId, x.IsActive });

        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt).IsRequired();
    }
}
