using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZealEducation.Domain.Entities;

namespace ZealEducation.Infrastructure.Data.Configurations;

public class StudyMaterialConfiguration : IEntityTypeConfiguration<StudyMaterial>
{
    public void Configure(EntityTypeBuilder<StudyMaterial> builder)
    {
        builder.ToTable("study_material");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(m => m.FileName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(m => m.FilePath)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(m => m.FileType)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(m => m.FileSizeMb)
            .IsRequired()
            .HasColumnType("decimal(8,2)");

        builder.Property(m => m.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(m => m.UploadedAt)
            .IsRequired();

        builder.HasOne(m => m.Course)
            .WithMany(c => c.StudyMaterials)
            .HasForeignKey(m => m.CourseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(m => m.UploadedByStaff)
            .WithMany()
            .HasForeignKey(m => m.UploadedByStaffId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(m => m.CourseId);
        builder.HasIndex(m => m.IsActive);
        builder.HasIndex(m => m.FilePath);
        builder.HasIndex(m => new { m.CourseId, m.IsActive });

        builder.Ignore(m => m.DomainEvents);
    }
}
