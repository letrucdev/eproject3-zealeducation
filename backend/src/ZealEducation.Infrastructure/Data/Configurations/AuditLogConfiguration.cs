using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZealEducation.Domain.Entities;

namespace ZealEducation.Infrastructure.Data.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("audit_log");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.TableName)
            .IsRequired()
            .HasMaxLength(60);

        builder.Property(a => a.RecordId)
            .IsRequired()
            .HasMaxLength(40);

        builder.Property(a => a.Action)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(10);

        builder.Property(a => a.OldValue)
            .HasColumnType("nvarchar(max)");

        builder.Property(a => a.NewValue)
            .HasColumnType("nvarchar(max)");

        builder.Property(a => a.ChangedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(a => a.IpAddress)
            .HasMaxLength(45);

        builder.HasOne(a => a.User)
            .WithMany()
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(a => a.ChangedAt).IsDescending();
        builder.HasIndex(a => new { a.TableName, a.RecordId });
        builder.HasIndex(a => a.UserId);
        builder.HasIndex(a => a.Action);

        builder.Ignore(a => a.DomainEvents);
    }
}
