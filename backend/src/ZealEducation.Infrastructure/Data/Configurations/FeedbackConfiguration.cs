using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZealEducation.Domain.Entities;

namespace ZealEducation.Infrastructure.Data.Configurations;

public class FeedbackConfiguration : IEntityTypeConfiguration<Feedback>
{
    public void Configure(EntityTypeBuilder<Feedback> builder)
    {
        builder.ToTable("feedback");

        builder.HasKey(f => f.Id);

        builder.Property(f => f.Type)
            .IsRequired()
            .HasMaxLength(20)
            .HasConversion<string>();

        builder.Property(f => f.Rating)
            .IsRequired();

        builder.Property(f => f.Comment)
            .HasMaxLength(1000);

        builder.Property(f => f.IsProcessed)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(f => f.ProcessedAt);

        builder.HasOne(f => f.ProcessedBy)
            .WithMany()
            .HasForeignKey(f => f.ProcessedById)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(f => f.IsProcessed)
            .HasDatabaseName("ix_feedback_is_processed");

        builder.HasOne(f => f.Candidate)
            .WithMany()
            .HasForeignKey(f => f.CandidateId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(f => f.Batch)
            .WithMany()
            .HasForeignKey(f => f.BatchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(f => f.TargetFaculty)
            .WithMany()
            .HasForeignKey(f => f.TargetFacultyId)
            .OnDelete(DeleteBehavior.Restrict);

        // A candidate may submit at most one feedback per (batch, type, target faculty).
        // For Course/General, TargetFacultyId is null and the unique index treats nulls as distinct in
        // SQL Server, so we add a filtered unique index per case.
        builder.HasIndex(f => new { f.CandidateId, f.BatchId, f.Type, f.TargetFacultyId })
            .HasDatabaseName("ix_feedback_unique_target")
            .IsUnique()
            .HasFilter("[TargetFacultyId] IS NOT NULL");

        builder.HasIndex(f => new { f.CandidateId, f.BatchId, f.Type })
            .HasDatabaseName("ix_feedback_unique_no_target")
            .IsUnique()
            .HasFilter("[TargetFacultyId] IS NULL");

        builder.Ignore(f => f.DomainEvents);
    }
}
