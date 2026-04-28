using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZealEducation.Domain.Entities;

namespace ZealEducation.Infrastructure.Data.Configurations;

public class AttendanceRecordConfiguration : IEntityTypeConfiguration<AttendanceRecord>
{
    public void Configure(EntityTypeBuilder<AttendanceRecord> builder)
    {
        builder.ToTable("attendance_record");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Status)
            .IsRequired()
            .HasMaxLength(20)
            .HasConversion<string>();

        builder.Property(a => a.PracticalHours)
            .HasColumnType("decimal(5,2)");

        builder.Property(a => a.Remarks)
            .HasMaxLength(500);

        builder.HasOne(a => a.ClassSession)
            .WithMany(s => s.AttendanceRecords)
            .HasForeignKey(a => a.ClassSessionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(a => a.Enrollment)
            .WithMany(e => e.AttendanceRecords)
            .HasForeignKey(a => a.EnrollmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(a => new { a.ClassSessionId, a.EnrollmentId }).IsUnique();

        builder.Ignore(a => a.DomainEvents);
    }
}
