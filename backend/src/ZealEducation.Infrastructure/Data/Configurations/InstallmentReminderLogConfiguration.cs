using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZealEducation.Domain.Entities;

namespace ZealEducation.Infrastructure.Data.Configurations;

public class InstallmentReminderLogConfiguration : IEntityTypeConfiguration<InstallmentReminderLog>
{
    public void Configure(EntityTypeBuilder<InstallmentReminderLog> builder)
    {
        builder.ToTable("installment_reminder_log");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.ReminderType)
            .IsRequired()
            .HasMaxLength(15)
            .HasConversion<string>();

        builder.Property(l => l.DaysOffset)
            .IsRequired();

        builder.Property(l => l.SentForDate)
            .IsRequired();

        builder.Property(l => l.RecipientEmail)
            .IsRequired()
            .HasMaxLength(256);

        builder.HasOne(l => l.InstallmentPlan)
            .WithMany()
            .HasForeignKey(l => l.InstallmentPlanId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(l => new { l.InstallmentPlanId, l.SentForDate, l.ReminderType })
            .IsUnique();

        builder.HasIndex(l => l.SentForDate);

        builder.Ignore(l => l.DomainEvents);
    }
}
