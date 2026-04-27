using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZealEducation.Domain.Entities;

namespace ZealEducation.Infrastructure.Data.Configurations;

public class ExaminationConfiguration : IEntityTypeConfiguration<Examination>
{
    public void Configure(EntityTypeBuilder<Examination> builder)
    {
        builder.ToTable("examination");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.ExamName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.ExamDate)
            .IsRequired();

        builder.Property(e => e.Location)
            .HasMaxLength(100);

        builder.Property(e => e.MaxScore)
            .IsRequired()
            .HasDefaultValue(100);

        builder.Property(e => e.PassScore)
            .IsRequired()
            .HasDefaultValue(50);

        builder.HasOne(e => e.Batch)
            .WithMany(b => b.Examinations)
            .HasForeignKey(e => e.BatchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.ScheduledBy)
            .WithMany()
            .HasForeignKey(e => e.ScheduledById)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.ExamResults)
            .WithOne(r => r.Examination)
            .HasForeignKey(r => r.ExamId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => new { e.BatchId, e.ExamName, e.ExamDate }).IsUnique();

        builder.Ignore(e => e.DomainEvents);
    }
}
