using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZealEducation.Domain.Entities;

namespace ZealEducation.Infrastructure.Data.Configurations;

public class CandidateConfiguration : IEntityTypeConfiguration<Candidate>
{
    public void Configure(EntityTypeBuilder<Candidate> builder)
    {
        builder.ToTable("candidate");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.CandidateCode)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(c => c.Address)
            .HasColumnType("nvarchar(max)");

        builder.Property(c => c.EmergencyContact)
            .HasMaxLength(100);

        builder.Property(c => c.Notes)
            .HasColumnType("nvarchar(max)");

        builder.Property(c => c.Status)
            .IsRequired()
            .HasMaxLength(20)
            .HasConversion<string>()
            .HasDefaultValue(Domain.Enums.CandidateStatus.Active);

        builder.Property(c => c.RegisteredAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.HasOne(c => c.UserAccount)
            .WithOne()
            .HasForeignKey<Candidate>(c => c.UserAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.RegisteredByStaff)
            .WithMany()
            .HasForeignKey(c => c.RegisteredByStaffId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(c => c.UserAccountId).IsUnique();
        builder.HasIndex(c => c.CandidateCode).IsUnique();

        builder.Ignore(c => c.DomainEvents);
    }
}
