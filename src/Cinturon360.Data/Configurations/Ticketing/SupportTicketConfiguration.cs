using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Cinturon360.Domain.Entities.Ticketing;

namespace Cinturon360.Data.Configurations.Ticketing;

public sealed class SupportTicketConfiguration : IEntityTypeConfiguration<SupportTicket>
{
    public void Configure(EntityTypeBuilder<SupportTicket> builder)
    {
        builder.ToTable("support_tickets");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasMaxLength(50);

        builder.Property(x => x.OrgId).IsRequired().HasMaxLength(50);
        builder.Property(x => x.RaisedByUserId).IsRequired().HasMaxLength(50);

        builder.Property(x => x.Status).IsRequired();
        builder.Property(x => x.Priority).IsRequired();
        builder.Property(x => x.Queue).IsRequired();
        builder.Property(x => x.Category).IsRequired();

        builder.Property(x => x.Subject).IsRequired().HasMaxLength(500);
        builder.Property(x => x.Description).IsRequired().HasMaxLength(10000);
        builder.Property(x => x.ErrorContext).HasColumnType("text");

        builder.Property(x => x.GitHubIssueNumber);
        builder.Property(x => x.GitHubIssueUrl).HasMaxLength(500);

        builder.Property(x => x.ResolvedAt);
        builder.Property(x => x.ClosedAt);
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt).IsRequired();

        builder.HasIndex(x => x.OrgId);
        builder.HasIndex(x => x.RaisedByUserId);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.Queue);
        builder.HasIndex(x => new { x.OrgId, x.Status, x.Queue });

        builder.HasMany(x => x.Comments)
            .WithOne()
            .HasForeignKey(c => c.TicketId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Escalations)
            .WithOne()
            .HasForeignKey(e => e.TicketId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
