using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Cinturon360.Domain.Entities.Identity;

namespace Cinturon360.Data.Configurations.Identity;

public sealed class UserMfaMethodConfiguration : IEntityTypeConfiguration<UserMfaMethod>
{
    public void Configure(EntityTypeBuilder<UserMfaMethod> builder)
    {
        builder.ToTable("user_mfa_methods");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasMaxLength(50);

        builder.Property(x => x.UserId).IsRequired().HasMaxLength(50);
        builder.Property(x => x.MfaMethod).IsRequired();
        builder.Property(x => x.SecretOrTarget).HasMaxLength(1000);

        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => new { x.UserId, x.MfaMethod });

        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt).IsRequired();
    }
}
