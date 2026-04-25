using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Cinturon360.Domain.Entities.Identity;

namespace Cinturon360.Data.Configurations.Identity;

public sealed class UserApiTokenConfiguration : IEntityTypeConfiguration<UserApiToken>
{
    public void Configure(EntityTypeBuilder<UserApiToken> builder)
    {
        builder.ToTable("user_api_tokens");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasMaxLength(50);

        builder.Property(x => x.UserId).IsRequired().HasMaxLength(50);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
        builder.Property(x => x.TokenHash).IsRequired().HasMaxLength(500);
        builder.Property(x => x.TokenPrefix).IsRequired().HasMaxLength(30);
        builder.Property(x => x.TokenClass).IsRequired();
        builder.Property(x => x.Scopes).HasMaxLength(2000);

        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.TokenHash).IsUnique();
        builder.HasIndex(x => new { x.UserId, x.IsRevoked });

        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt).IsRequired();
    }
}
