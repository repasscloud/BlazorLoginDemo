using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Cinturon360.Domain.Entities.Identity;

namespace Cinturon360.Data.Configurations.Identity;

public sealed class UserRecoveryCodeConfiguration : IEntityTypeConfiguration<UserRecoveryCode>
{
    public void Configure(EntityTypeBuilder<UserRecoveryCode> builder)
    {
        builder.ToTable("user_recovery_codes");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasMaxLength(50);

        builder.Property(x => x.UserId).IsRequired().HasMaxLength(50);
        builder.Property(x => x.CodeHash).IsRequired().HasMaxLength(500);

        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => new { x.UserId, x.IsUsed });

        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt).IsRequired();
    }
}
