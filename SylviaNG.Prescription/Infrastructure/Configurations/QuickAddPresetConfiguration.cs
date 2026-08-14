using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SylviaNG.Prescription.Domain.Entities;

namespace SylviaNG.Prescription.Infrastructure.Configurations
{
    public class QuickAddPresetConfiguration : IEntityTypeConfiguration<QuickAddPreset>
    {
        public void Configure(EntityTypeBuilder<QuickAddPreset> builder)
        {
            builder.ToTable("QuickAddPresets");
            builder.HasKey(q => q.QuickAddPresetId);

            builder.Property(q => q.SectionType)
                .HasConversion<string>()
                .HasMaxLength(20);

            builder.Property(q => q.Label)
                .IsRequired()
                .HasMaxLength(300);

            builder.Property(q => q.PayloadJson).IsRequired();

            builder.HasIndex(q => new { q.DoctorId, q.SectionType });

            builder.HasOne<Doctor>()
                .WithMany()
                .HasForeignKey(q => q.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
