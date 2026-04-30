using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Cinturon360.Domain.Entities.Ticketing;

namespace Cinturon360.Data.Configurations.Ticketing;

public sealed class TicketAttachmentConfiguration : IEntityTypeConfiguration<TicketAttachment>
{
    public void Configure(EntityTypeBuilder<TicketAttachment> builder)
    {
        builder.ToTable("ticket_attachments");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasMaxLength(50);

        builder.Property(x => x.TicketId).HasMaxLength(50).IsRequired();
        builder.Property(x => x.UploadedByUserId).HasMaxLength(50).IsRequired();
        builder.Property(x => x.FileName).HasMaxLength(500).IsRequired();
        builder.Property(x => x.ContentType).HasMaxLength(200).IsRequired();
        builder.Property(x => x.FileSize).IsRequired();
        builder.Property(x => x.Content).IsRequired();
        builder.Property(x => x.IsPrivate).IsRequired();

        builder.HasOne<SupportTicket>()
            .WithMany(t => t.Attachments)
            .HasForeignKey(x => x.TicketId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.TicketId, x.IsPrivate });
    }
}
