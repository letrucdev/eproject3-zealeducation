using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZealEducation.Domain.Entities;

namespace ZealEducation.Infrastructure.Data.Configurations;

public class SystemAssetConfiguration : IEntityTypeConfiguration<SystemAsset>
{
    public void Configure(EntityTypeBuilder<SystemAsset> builder)
    {
        builder.ToTable("system_asset");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.SerialNumber)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.ConditionStatus)
            .HasConversion<string>()
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.AssetType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.AssetName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(e => e.Location)
            .IsRequired()
            .HasMaxLength(255);

        // Indexes
        builder.HasIndex(e => e.SerialNumber).IsUnique();
        builder.HasIndex(e => e.ConditionStatus);
        builder.HasIndex(e => e.AssetType);

        // Required by framework conventions if there's a related Staff entity
        // ManagedBy matches Staff Id
        builder.HasOne<Staff>()
            .WithMany()
            .HasForeignKey(e => e.ManagedBy)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(e => e.DomainEvents);
    }
}
