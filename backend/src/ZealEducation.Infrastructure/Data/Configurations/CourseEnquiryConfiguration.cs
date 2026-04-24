using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Enums;

namespace ZealEducation.Infrastructure.Data.Configurations;

public class CourseEnquiryConfiguration : IEntityTypeConfiguration<CourseEnquiry>
{
    public void Configure(EntityTypeBuilder<CourseEnquiry> builder)
    {
        builder.ToTable("course_enquiry");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.FullName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.Phone)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(e => e.Email)
            .HasMaxLength(100);

        builder.Property(e => e.Source)
            .IsRequired()
            .HasMaxLength(30)
            .HasConversion<string>();

        builder.Property(e => e.Status)
            .IsRequired()
            .HasMaxLength(30)
            .HasConversion<string>()
            .HasDefaultValue(EnquiryStatus.New);

        builder.Property(e => e.NextFollowUpDate)
            .HasColumnType("date");

        builder.HasOne(e => e.CourseInterested)
            .WithMany(c => c.Enquiries)
            .HasForeignKey(e => e.CourseInterestedId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.AssignedCounselor)
            .WithMany()
            .HasForeignKey(e => e.AssignedCounselorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.ConvertedCandidate)
            .WithMany()
            .HasForeignKey(e => e.ConvertedCandidateId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.Notes)
            .WithOne(n => n.Enquiry)
            .HasForeignKey(n => n.EnquiryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => e.AssignedCounselorId);
        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => e.NextFollowUpDate);
        builder.HasIndex(e => e.Phone);
        builder.HasIndex(e => e.CourseInterestedId);

        builder.Ignore(e => e.DomainEvents);
    }
}
