using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZealEducation.Domain.Entities;

namespace ZealEducation.Infrastructure.Data.Configurations;

public class PaymentTransactionConfiguration : IEntityTypeConfiguration<PaymentTransaction>
{
    public void Configure(EntityTypeBuilder<PaymentTransaction> builder)
    {
        builder.ToTable("payment_transaction");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Amount)
            .IsRequired()
            .HasColumnType("decimal(12,2)");

        builder.Property(t => t.OutstandingBalanceAfter)
            .IsRequired()
            .HasColumnType("decimal(12,2)");

        builder.Property(t => t.PaymentMethod)
            .IsRequired()
            .HasMaxLength(30)
            .HasConversion<string>();

        builder.Property(t => t.ReceiptNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(t => t.PaymentDate)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(t => t.ReceiptFilePath)
            .HasMaxLength(500);

        builder.Property(t => t.BankTransferProofPath)
            .HasMaxLength(500);

        builder.HasOne(t => t.FeeStructure)
            .WithMany(f => f.PaymentTransactions)
            .HasForeignKey(t => t.FeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.ProcessedByStaff)
            .WithMany()
            .HasForeignKey(t => t.ProcessedByStaffId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.InstallmentPlan)
            .WithMany()
            .HasForeignKey(t => t.InstallmentPlanId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(t => t.ReceiptNumber).IsUnique();
        builder.HasIndex(t => t.InstallmentPlanId);

        builder.Ignore(t => t.DomainEvents);
    }
}
