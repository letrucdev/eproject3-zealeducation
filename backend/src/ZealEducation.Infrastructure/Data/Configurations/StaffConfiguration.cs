using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZealEducation.Domain.Entities;

namespace ZealEducation.Infrastructure.Data.Configurations;

public class StaffConfiguration : IEntityTypeConfiguration<Staff>
{
    public void Configure(EntityTypeBuilder<Staff> builder)
    {
        builder.ToTable("staff");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Position)
            .IsRequired()
            .HasMaxLength(60);

        builder.Property(s => s.Department)
            .IsRequired()
            .HasMaxLength(60);

        builder.Property(s => s.JoinedDate)
            .HasColumnType("date")
            .HasDefaultValueSql("CAST(GETUTCDATE() AS date)");

        builder.Property(s => s.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.HasOne(s => s.UserAccount)
            .WithOne()
            .HasForeignKey<Staff>(s => s.UserAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(s => s.UserAccountId).IsUnique();

        builder.Ignore(s => s.DomainEvents);
    }
}
