using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Cinturon360.Domain.Entities.Identity;

namespace Cinturon360.Data.Configurations.Identity;

public sealed class UserAuthMethodConfiguration : IEntityTypeConfiguration<UserAuthMethod>
{
    public void Configure(EntityTypeBuilder<UserAuthMethod> builder)
    {
        builder.ToTable("user_auth_methods");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasMaxLength(50);

        builder.Property(x => x.UserId).IsRequired().HasMaxLength(50);
        builder.Property(x => x.LoginMethod).IsRequired();
        builder.Property(x => x.Status).IsRequired();
        builder.Property(x => x.ExternalSubject).HasMaxLength(500);
        builder.Property(x => x.ProviderConfigId).HasMaxLength(50);

        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => new { x.UserId, x.LoginMethod });
        builder.HasIndex(x => new { x.LoginMethod, x.ExternalSubject });

        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt).IsRequired();
    }
}
