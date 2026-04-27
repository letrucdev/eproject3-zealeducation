using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZealEducation.Domain.Entities;

namespace ZealEducation.Infrastructure.Data.Configurations;

public class BatchConfiguration : IEntityTypeConfiguration<Batch>
{
    public void Configure(EntityTypeBuilder<Batch> builder)
    {
        builder.ToTable("batch");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.BatchCode)
            .IsRequired()
            .HasMaxLength(30);

        builder.Property(b => b.Location)
            .HasMaxLength(100);

        builder.Property(b => b.StartDate)
            .IsRequired();

        builder.Property(b => b.EndDate)
            .IsRequired();

        builder.Property(b => b.MaxCapacity)
            .IsRequired()
            .HasDefaultValue(30);

        builder.Property(b => b.Status)
            .IsRequired()
            .HasMaxLength(20)
            .HasConversion<string>()
            .HasDefaultValue(Domain.Enums.BatchStatus.NeedsInstructor);

        builder.HasOne(b => b.Course)
            .WithMany(c => c.Batches)
            .HasForeignKey(b => b.CourseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(b => b.Faculty)
            .WithMany()
            .HasForeignKey(b => b.FacultyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(b => b.BatchCode).IsUnique();

        builder.Ignore(b => b.DomainEvents);
    }
}
