using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZealEducation.Domain.Entities;

namespace ZealEducation.Infrastructure.Data.Configurations;

public class EnquiryNoteConfiguration : IEntityTypeConfiguration<EnquiryNote>
{
    public void Configure(EntityTypeBuilder<EnquiryNote> builder)
    {
        builder.ToTable("enquiry_note");

        builder.HasKey(n => n.Id);

        builder.Property(n => n.Content)
            .IsRequired()
            .HasColumnType("nvarchar(max)");

        builder.HasOne(n => n.AuthorStaff)
            .WithMany()
            .HasForeignKey(n => n.AuthorStaffId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(n => n.EnquiryId);
        builder.HasIndex(n => n.AuthorStaffId);

        builder.Ignore(n => n.DomainEvents);
    }
}
