using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZealEducation.Domain.Entities;

namespace ZealEducation.Infrastructure.Data.Configurations;

public class ExamResultConfiguration : IEntityTypeConfiguration<ExamResult>
{
    public void Configure(EntityTypeBuilder<ExamResult> builder)
    {
        builder.ToTable("exam_result");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Score)
            .IsRequired()
            .HasColumnType("numeric(6,2)");

        builder.Property(r => r.Grade)
            .HasMaxLength(5);

        builder.Property(r => r.IsPassed)
            .IsRequired();

        builder.Property(r => r.IsOverridden)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(r => r.OverrideReason)
            .HasMaxLength(500);

        builder.Property(r => r.GradedAt)
            .IsRequired();

        builder.HasOne(r => r.Examination)
            .WithMany(e => e.ExamResults)
            .HasForeignKey(r => r.ExamId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Enrollment)
            .WithMany(e => e.ExamResults)
            .HasForeignKey(r => r.EnrollmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.GradedBy)
            .WithMany()
            .HasForeignKey(r => r.GradedById)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.OverrideBy)
            .WithMany()
            .HasForeignKey(r => r.OverrideById)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(r => new { r.ExamId, r.EnrollmentId }).IsUnique();

        builder.Ignore(r => r.DomainEvents);
    }
}
