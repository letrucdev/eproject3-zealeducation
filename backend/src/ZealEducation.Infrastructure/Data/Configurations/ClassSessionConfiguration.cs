using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZealEducation.Domain.Entities;

namespace ZealEducation.Infrastructure.Data.Configurations;

public class ClassSessionConfiguration : IEntityTypeConfiguration<ClassSession>
{
    public void Configure(EntityTypeBuilder<ClassSession> builder)
    {
        builder.ToTable("class_session");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.SessionDate)
            .IsRequired();

        builder.Property(s => s.StartTime)
            .IsRequired();

        builder.Property(s => s.EndTime)
            .IsRequired();

        builder.Property(s => s.Topic)
            .HasMaxLength(200);

        builder.Property(s => s.Location)
            .HasMaxLength(100);

        builder.Property(s => s.Status)
            .IsRequired()
            .HasMaxLength(20)
            .HasConversion<string>()
            .HasDefaultValue(Domain.Enums.ClassSessionStatus.Scheduled);

        builder.HasOne(s => s.Batch)
            .WithMany(b => b.ClassSessions)
            .HasForeignKey(s => s.BatchId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(s => new { s.BatchId, s.SessionDate, s.StartTime }).IsUnique();

        builder.Ignore(s => s.DomainEvents);
    }
}
