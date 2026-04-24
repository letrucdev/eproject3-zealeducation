using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZealEducation.Domain.Entities;

namespace ZealEducation.Infrastructure.Data.Configurations;

public class FeeStructureConfiguration : IEntityTypeConfiguration<FeeStructure>
{
    public void Configure(EntityTypeBuilder<FeeStructure> builder)
    {
        builder.ToTable("fee_structure");

        builder.HasKey(f => f.Id);

        builder.Property(f => f.TotalFee)
            .IsRequired()
            .HasColumnType("decimal(12,2)");

        builder.Property(f => f.AmountPaid)
            .IsRequired()
            .HasColumnType("decimal(12,2)")
            .HasDefaultValue(0m);

        builder.Property(f => f.FeeType)
            .IsRequired()
            .HasMaxLength(20)
            .HasConversion<string>()
            .HasDefaultValue(Domain.Enums.FeeType.Tuition);

        builder.Property(f => f.OutstandingBalance)
            .HasColumnType("decimal(12,2)")
            .HasComputedColumnSql("[TotalFee] - [AmountPaid]", stored: true);

        builder.Property(f => f.PaymentStatus)
            .IsRequired()
            .HasMaxLength(20)
            .HasConversion<string>()
            .HasDefaultValue(Domain.Enums.PaymentStatus.Unpaid);

        builder.Property(f => f.PaymentType)
            .IsRequired()
            .HasMaxLength(20)
            .HasConversion<string>()
            .HasDefaultValue(Domain.Enums.PaymentType.NotSet);

        builder.Property(f => f.Notes)
            .HasColumnType("nvarchar(max)");

        builder.HasOne(f => f.Candidate)
            .WithMany()
            .HasForeignKey(f => f.CandidateId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(f => f.CandidateId);

        builder.Ignore(f => f.DomainEvents);
    }
}
