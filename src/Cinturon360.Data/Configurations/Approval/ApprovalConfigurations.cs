using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Cinturon360.Domain.Entities.Approval;

namespace Cinturon360.Data.Configurations.Approval;

public sealed class ApprovalRequestConfiguration : IEntityTypeConfiguration<ApprovalRequest>
{
    public void Configure(EntityTypeBuilder<ApprovalRequest> builder)
    {
        builder.ToTable("approval_requests");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasMaxLength(50);
        builder.Property(x => x.OrgId).IsRequired().HasMaxLength(50);
        builder.Property(x => x.SubjectType).HasConversion<int>();
        builder.Property(x => x.SubjectId).IsRequired().HasMaxLength(50);
        builder.Property(x => x.RequestedByUserId).IsRequired().HasMaxLength(50);
        builder.Property(x => x.Status).HasConversion<int>();
        builder.Property(x => x.Notes).HasMaxLength(2000);
        builder.HasIndex(x => x.OrgId);
        builder.HasIndex(x => x.SubjectId);
        builder.HasIndex(x => x.Status);
    }
}

public sealed class ApprovalDecisionConfiguration : IEntityTypeConfiguration<ApprovalDecision>
{
    public void Configure(EntityTypeBuilder<ApprovalDecision> builder)
    {
        builder.ToTable("approval_decisions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasMaxLength(50);
        builder.Property(x => x.ApprovalRequestId).IsRequired().HasMaxLength(50);
        builder.Property(x => x.ApproverUserId).IsRequired().HasMaxLength(50);
        builder.Property(x => x.Decision).HasConversion<int>();
        builder.Property(x => x.Comments).HasMaxLength(2000);
        builder.HasIndex(x => x.ApprovalRequestId);
        builder.HasIndex(x => x.ApproverUserId);
    }
}
