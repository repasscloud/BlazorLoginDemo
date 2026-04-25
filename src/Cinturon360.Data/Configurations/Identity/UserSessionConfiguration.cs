using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Cinturon360.Domain.Entities.Identity;

namespace Cinturon360.Data.Configurations.Identity;

public sealed class UserSessionConfiguration : IEntityTypeConfiguration<UserSession>
{
    public void Configure(EntityTypeBuilder<UserSession> builder)
    {
        builder.ToTable("user_sessions");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasMaxLength(50);

        builder.Property(x => x.UserId).IsRequired().HasMaxLength(50);
        builder.Property(x => x.Jti).IsRequired().HasMaxLength(200);
        builder.Property(x => x.TokenClass).IsRequired();
        builder.Property(x => x.DeviceId).HasMaxLength(200);
        builder.Property(x => x.UserAgent).HasMaxLength(500);
        builder.Property(x => x.IpAddress).HasMaxLength(45);
        builder.Property(x => x.RevokedReason).HasMaxLength(500);

        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.Jti).IsUnique();
        builder.HasIndex(x => new { x.UserId, x.IsRevoked, x.ExpiresAt });

        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt).IsRequired();
    }
}
