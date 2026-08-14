using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SylviaNG.Prescription.Domain.Entities;

namespace SylviaNG.Prescription.Infrastructure.Configurations
{
    public class DoctorConfiguration : IEntityTypeConfiguration<Doctor>
    {
        public void Configure(EntityTypeBuilder<Doctor> builder)
        {
            builder.ToTable("Doctors");
            builder.HasKey(d => d.DoctorId);

            builder.Property(d => d.FullName)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(d => d.Phone)
                .IsRequired()
                .HasMaxLength(11);

            builder.Property(d => d.Qualification).HasMaxLength(200);
            builder.Property(d => d.Department).HasMaxLength(100);
            builder.Property(d => d.LicenseNumber).HasMaxLength(50);
            builder.Property(d => d.Specialization).HasMaxLength(200);

            builder.Property(d => d.Gender)
                .HasConversion<string>()
                .HasMaxLength(10);

            builder.Property(d => d.PreferredLanguage)
                .HasConversion<string>()
                .HasMaxLength(10);

            builder.HasIndex(d => d.UserId).IsUnique();
            builder.HasIndex(d => d.LicenseNumber).IsUnique();

            builder.HasOne(d => d.User)
                .WithMany()
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // SetNull (not Restrict) so deleting a template a doctor prefers (US-050) can't
            // ever fail on this FK — it silently falls the doctor back to "no preference"
            // (the finalize-validation checklist then prompts them to pick again), matching
            // US-050's "falls back to default" intent without touching Epic H's delete code.
            builder.HasOne<PrescriptionTemplate>()
                .WithMany()
                .HasForeignKey(d => d.PreferredTemplateId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
