using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZealEducation.Domain.Entities;

namespace ZealEducation.Infrastructure.Data.Configurations;

public class InstallmentPlanConfiguration : IEntityTypeConfiguration<InstallmentPlan>
{
    public void Configure(EntityTypeBuilder<InstallmentPlan> builder)
    {
        builder.ToTable("installment_plan");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.InstallmentNo)
            .IsRequired();

        builder.Property(p => p.AmountDue)
            .IsRequired()
            .HasColumnType("decimal(12,2)");

        builder.Property(p => p.DueDate)
            .IsRequired();

        builder.Property(p => p.AmountPaid)
            .IsRequired()
            .HasColumnType("decimal(12,2)")
            .HasDefaultValue(0m);

        builder.Property(p => p.PaidDate);

        builder.Property(p => p.Status)
            .IsRequired()
            .HasMaxLength(15)
            .HasConversion<string>()
            .HasDefaultValue(Domain.Enums.InstallmentStatus.Pending);

        builder.Property(p => p.PenaltyAmount)
            .IsRequired()
            .HasColumnType("decimal(12,2)")
            .HasDefaultValue(0m);

        builder.HasOne(p => p.FeeStructure)
            .WithMany(f => f.InstallmentPlans)
            .HasForeignKey(p => p.FeeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(p => new { p.FeeId, p.InstallmentNo }).IsUnique();

        builder.Ignore(p => p.DomainEvents);
    }
}
