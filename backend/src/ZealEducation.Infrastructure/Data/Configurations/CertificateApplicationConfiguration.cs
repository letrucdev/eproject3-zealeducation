using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZealEducation.Domain.Entities;

namespace ZealEducation.Infrastructure.Data.Configurations;

public class CertificateApplicationConfiguration : IEntityTypeConfiguration<CertificateApplication>
{
    public void Configure(EntityTypeBuilder<CertificateApplication> builder)
    {
        builder.ToTable("certificate_application");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Status)
            .IsRequired()
            .HasMaxLength(20)
            .HasConversion<string>();

        builder.Property(c => c.CertificateNumber)
            .HasMaxLength(50);

        builder.Property(c => c.CertificateFilePath)
            .HasMaxLength(500);

        builder.Property(c => c.ApprovedAt);

        builder.HasOne(c => c.Enrollment)
            .WithMany()
            .HasForeignKey(c => c.EnrollmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.ApprovedByStaff)
            .WithMany()
            .HasForeignKey(c => c.ApprovedByStaffId)
            .OnDelete(DeleteBehavior.Restrict);

        // At most one active (Pending or Approved) application per enrollment.
        // SQL Server treats nulls as distinct in unique indexes, but Status is
        // not nullable so a plain unique index on EnrollmentId is enough — both
        // Pending and Approved are valid "active" states and we only have those two.
        builder.HasIndex(c => c.EnrollmentId)
            .HasDatabaseName("ix_certificate_application_enrollment")
            .IsUnique();

        builder.HasIndex(c => c.CertificateNumber)
            .HasDatabaseName("ix_certificate_application_certificate_number")
            .IsUnique()
            .HasFilter("[CertificateNumber] IS NOT NULL");

        builder.HasIndex(c => c.Status)
            .HasDatabaseName("ix_certificate_application_status");

        builder.Ignore(c => c.DomainEvents);
    }
}
