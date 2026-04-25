using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Cinturon360.Domain.Entities.Policy;

namespace Cinturon360.Data.Configurations.Policy;

public sealed class TravelPolicyConfiguration : IEntityTypeConfiguration<TravelPolicy>
{
    public void Configure(EntityTypeBuilder<TravelPolicy> builder)
    {
        builder.ToTable("travel_policies");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasMaxLength(50);
        builder.Property(x => x.OrgId).IsRequired().HasMaxLength(50);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Description).HasMaxLength(1000);
        builder.HasIndex(x => x.OrgId);
    }
}

public sealed class PolicyRuleConfiguration : IEntityTypeConfiguration<PolicyRule>
{
    public void Configure(EntityTypeBuilder<PolicyRule> builder)
    {
        builder.ToTable("policy_rules");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasMaxLength(50);
        builder.Property(x => x.PolicyId).IsRequired().HasMaxLength(50);
        builder.Property(x => x.RuleType).HasConversion<int>();
        builder.Property(x => x.ViolationAction).HasConversion<int>();
        builder.Property(x => x.ValueString).HasMaxLength(500);
        builder.Property(x => x.ValueDecimal).HasPrecision(18, 4);
        builder.Property(x => x.Description).HasMaxLength(500);
        builder.HasIndex(x => x.PolicyId);
    }
}

public sealed class PolicyAssignmentConfiguration : IEntityTypeConfiguration<PolicyAssignment>
{
    public void Configure(EntityTypeBuilder<PolicyAssignment> builder)
    {
        builder.ToTable("policy_assignments");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasMaxLength(50);
        builder.Property(x => x.PolicyId).IsRequired().HasMaxLength(50);
        builder.Property(x => x.TargetType).HasConversion<int>();
        builder.Property(x => x.TargetId).IsRequired().HasMaxLength(50);
        builder.HasIndex(x => new { x.PolicyId, x.TargetType, x.TargetId }).IsUnique();
    }
}
