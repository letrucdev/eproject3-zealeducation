using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZealEducation.Domain.Entities;

namespace ZealEducation.Infrastructure.Data.Configurations;

public class EnrollmentConfiguration : IEntityTypeConfiguration<Enrollment>
{
    public void Configure(EntityTypeBuilder<Enrollment> builder)
    {
        builder.ToTable("enrollment");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.EnrollmentDate)
            .IsRequired();

        builder.Property(e => e.Status)
            .IsRequired()
            .HasMaxLength(20)
            .HasConversion<string>()
            .HasDefaultValue(Domain.Enums.EnrollmentStatus.PendingAssignment);

        builder.Property(e => e.Notes)
            .HasColumnType("nvarchar(max)");

        builder.HasOne(e => e.Candidate)
            .WithMany()
            .HasForeignKey(e => e.CandidateId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Course)
            .WithMany()
            .HasForeignKey(e => e.CourseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Batch)
            .WithMany(b => b.Enrollments)
            .HasForeignKey(e => e.BatchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Incharge)
            .WithMany()
            .HasForeignKey(e => e.InchargeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Fee)
            .WithOne(f => f.Enrollment)
            .HasForeignKey<Enrollment>(e => e.FeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(e => e.DomainEvents);
    }
}
