using Cinturon360.Domain.Entities.Ticketing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cinturon360.Data.Configurations.Ticketing;

public sealed class TicketEmailTemplateConfiguration : IEntityTypeConfiguration<TicketEmailTemplate>
{
    public void Configure(EntityTypeBuilder<TicketEmailTemplate> builder)
    {
        builder.ToTable("ticket_email_templates");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasMaxLength(50);
        builder.Property(x => x.Code).IsRequired().HasMaxLength(200);
        builder.Property(x => x.LanguageCode).IsRequired().HasMaxLength(10);
        builder.Property(x => x.Description).HasMaxLength(500);
        builder.Property(x => x.HtmlBody).IsRequired();
        builder.Property(x => x.PlainTextBody);
        builder.Property(x => x.IsActive).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt).IsRequired();

        builder.HasIndex(x => new { x.Code, x.LanguageCode }).IsUnique();
        builder.HasIndex(x => x.IsActive);
    }
}