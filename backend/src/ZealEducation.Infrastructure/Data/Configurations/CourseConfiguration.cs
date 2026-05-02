using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZealEducation.Domain.Entities;

namespace ZealEducation.Infrastructure.Data.Configurations;

public class CourseConfiguration : IEntityTypeConfiguration<Course>
{
    public void Configure(EntityTypeBuilder<Course> builder)
    {
        builder.ToTable("course");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.CourseName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.Description)
            .HasColumnType("nvarchar(max)");

        builder.Property(c => c.DurationWeeks)
            .IsRequired();

        builder.Property(c => c.BaseFee)
            .IsRequired()
            .HasColumnType("decimal(12,2)");

        builder.Property(c => c.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.HasIndex(c => c.CourseName).IsUnique();

        builder.Ignore(c => c.DomainEvents);
    }
}
