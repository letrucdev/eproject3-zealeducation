using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZealEducation.Domain.Entities;

namespace ZealEducation.Infrastructure.Data.Configurations;

public class MaterialDownloadLogConfiguration : IEntityTypeConfiguration<MaterialDownloadLog>
{
    public void Configure(EntityTypeBuilder<MaterialDownloadLog> builder)
    {
        builder.ToTable("material_download_log");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.DownloadedAt)
            .IsRequired();

        builder.HasOne(d => d.Material)
            .WithMany(m => m.DownloadLogs)
            .HasForeignKey(d => d.MaterialId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(d => d.Candidate)
            .WithMany()
            .HasForeignKey(d => d.CandidateId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(d => new { d.MaterialId, d.DownloadedAt });
        builder.HasIndex(d => d.CandidateId);

        builder.Ignore(d => d.DomainEvents);
    }
}
