using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Cinturon360.Domain.Entities.Ticketing;

namespace Cinturon360.Data.Configurations.Ticketing;

public sealed class TicketEscalationConfiguration : IEntityTypeConfiguration<TicketEscalation>
{
    public void Configure(EntityTypeBuilder<TicketEscalation> builder)
    {
        builder.ToTable("ticket_escalations");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasMaxLength(50);

        builder.Property(x => x.TicketId).IsRequired().HasMaxLength(50);
        builder.Property(x => x.ActorUserId).IsRequired().HasMaxLength(50);
        builder.Property(x => x.FromQueue).IsRequired();
        builder.Property(x => x.ToQueue).IsRequired();
        builder.Property(x => x.IsEscalation).IsRequired();
        builder.Property(x => x.Reason).HasMaxLength(1000);
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt).IsRequired();

        builder.HasIndex(x => x.TicketId);
    }
}
