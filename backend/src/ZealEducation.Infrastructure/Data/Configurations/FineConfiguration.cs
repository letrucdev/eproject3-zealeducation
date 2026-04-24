using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZealEducation.Domain.Entities;

namespace ZealEducation.Infrastructure.Data.Configurations;

public class FineConfiguration : IEntityTypeConfiguration<Fine>
{
    public void Configure(EntityTypeBuilder<Fine> builder)
    {
        builder.ToTable("fine");

        builder.HasKey(f => f.Id);

        builder.Property(f => f.ViolationReason)
            .IsRequired()
            .HasColumnType("nvarchar(max)");

        builder.Property(f => f.PenaltyAmount)
            .IsRequired()
            .HasColumnType("decimal(12,2)");

        builder.Property(f => f.IssuedDate)
            .IsRequired();

        builder.Property(f => f.IsPaid)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(f => f.PaidDate);

        builder.HasOne(f => f.FeeStructure)
            .WithOne(fs => fs.Fine)
            .HasForeignKey<Fine>(f => f.FeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(f => f.Candidate)
            .WithMany()
            .HasForeignKey(f => f.CandidateId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(f => f.IssuedByStaff)
            .WithMany()
            .HasForeignKey(f => f.IssuedByStaffId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(f => f.FeeId).IsUnique();

        builder.Ignore(f => f.DomainEvents);
    }
}
