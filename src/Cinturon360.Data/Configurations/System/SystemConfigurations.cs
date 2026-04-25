using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Cinturon360.Domain.Entities.System;

namespace Cinturon360.Data.Configurations.System;

public sealed class JobConfiguration : IEntityTypeConfiguration<Job>
{
    public void Configure(EntityTypeBuilder<Job> builder)
    {
        builder.ToTable("jobs");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasMaxLength(50);
        builder.Property(x => x.JobType).HasConversion<int>();
        builder.Property(x => x.Status).HasConversion<int>();
        builder.Property(x => x.OrgId).HasMaxLength(50);
        builder.Property(x => x.UserId).HasMaxLength(50);
        builder.Property(x => x.ErrorMessage).HasMaxLength(4000);
        builder.Property(x => x.TraceId).HasMaxLength(100);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.JobType);
        builder.HasIndex(x => x.ScheduledAt);
    }
}

public sealed class StoredDocumentConfiguration : IEntityTypeConfiguration<StoredDocument>
{
    public void Configure(EntityTypeBuilder<StoredDocument> builder)
    {
        builder.ToTable("stored_documents");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasMaxLength(50);
        builder.Property(x => x.DocumentType).HasConversion<int>();
        builder.Property(x => x.OrgId).HasMaxLength(50);
        builder.Property(x => x.UserId).HasMaxLength(50);
        builder.Property(x => x.SubjectId).HasMaxLength(50);
        builder.Property(x => x.FileName).IsRequired().HasMaxLength(500);
        builder.Property(x => x.StorageKey).IsRequired().HasMaxLength(1000);
        builder.Property(x => x.ContentType).HasMaxLength(200);
        builder.HasIndex(x => x.OrgId);
        builder.HasIndex(x => x.SubjectId);
    }
}
