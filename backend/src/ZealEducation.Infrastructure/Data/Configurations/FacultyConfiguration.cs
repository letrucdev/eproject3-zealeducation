using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZealEducation.Domain.Entities;

namespace ZealEducation.Infrastructure.Data.Configurations;

public class FacultyConfiguration : IEntityTypeConfiguration<Faculty>
{
    public void Configure(EntityTypeBuilder<Faculty> builder)
    {
        builder.ToTable("faculty");

        builder.HasKey(f => f.Id);

        builder.Property(f => f.FacultyCode)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(f => f.Qualification)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(f => f.Specialization)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(f => f.ExperienceYears)
            .IsRequired()
            .HasDefaultValue(0);

        builder.HasOne(f => f.Staff)
            .WithOne(s => s.Faculty)
            .HasForeignKey<Faculty>(f => f.StaffId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(f => f.StaffId).IsUnique();
        builder.HasIndex(f => f.FacultyCode).IsUnique();

        builder.Ignore(f => f.DomainEvents);
    }
}
