using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Cinturon360.Domain.Entities.Identity;

namespace Cinturon360.Data.Configurations.Identity;

public sealed class UserAccessOverrideConfiguration : IEntityTypeConfiguration<UserAccessOverride>
{
    public void Configure(EntityTypeBuilder<UserAccessOverride> builder)
    {
        builder.ToTable("user_access_overrides");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasMaxLength(50);

        builder.Property(x => x.UserId).IsRequired().HasMaxLength(50);
        builder.Property(x => x.PolicyKey).IsRequired().HasMaxLength(100);
        builder.Property(x => x.PolicyValue).IsRequired().HasMaxLength(500);
        builder.Property(x => x.GrantedByUserId).HasMaxLength(50);

        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => new { x.UserId, x.PolicyKey, x.IsActive });

        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt).IsRequired();
    }
}
