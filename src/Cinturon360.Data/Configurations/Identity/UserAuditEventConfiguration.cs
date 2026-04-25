using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Cinturon360.Domain.Entities.Identity;

namespace Cinturon360.Data.Configurations.Identity;

public sealed class UserAuditEventConfiguration : IEntityTypeConfiguration<UserAuditEvent>
{
    public void Configure(EntityTypeBuilder<UserAuditEvent> builder)
    {
        builder.ToTable("user_audit_events");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasMaxLength(50);

        builder.Property(x => x.UserId).IsRequired().HasMaxLength(50);
        builder.Property(x => x.EventType).IsRequired();
        builder.Property(x => x.ActorUserId).HasMaxLength(50);
        builder.Property(x => x.OrgId).HasMaxLength(50);
        builder.Property(x => x.IpAddress).HasMaxLength(45);
        builder.Property(x => x.UserAgent).HasMaxLength(500);
        builder.Property(x => x.CorrelationId).HasMaxLength(100);
        builder.Property(x => x.Details).HasColumnType("jsonb");
        builder.Property(x => x.FailureReason).HasMaxLength(1000);

        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => new { x.UserId, x.EventType });
        builder.HasIndex(x => x.CreatedAt);

        // Audit events are append-only — block updates
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt).IsRequired();
    }
}
