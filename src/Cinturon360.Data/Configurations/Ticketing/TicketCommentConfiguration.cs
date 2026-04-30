using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Cinturon360.Domain.Entities.Ticketing;

namespace Cinturon360.Data.Configurations.Ticketing;

public sealed class TicketCommentConfiguration : IEntityTypeConfiguration<TicketComment>
{
    public void Configure(EntityTypeBuilder<TicketComment> builder)
    {
        builder.ToTable("ticket_comments");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasMaxLength(50);

        builder.Property(x => x.TicketId).IsRequired().HasMaxLength(50);
        builder.Property(x => x.AuthorUserId).IsRequired().HasMaxLength(50);
        builder.Property(x => x.AuthorDisplayName).IsRequired().HasMaxLength(200);
        builder.Property(x => x.IsPrivate).IsRequired();
        builder.Property(x => x.Body).IsRequired().HasMaxLength(10000);
        builder.Property(x => x.GitHubCommentId);
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt).IsRequired();

        builder.HasIndex(x => x.TicketId);
        builder.HasIndex(x => new { x.TicketId, x.IsPrivate });
    }
}
