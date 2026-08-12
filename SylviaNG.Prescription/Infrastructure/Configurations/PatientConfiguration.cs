using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SylviaNG.Prescription.Domain.Entities;

namespace SylviaNG.Prescription.Infrastructure.Configurations
{
    public class PatientConfiguration : IEntityTypeConfiguration<Patient>
    {
        public void Configure(EntityTypeBuilder<Patient> builder)
        {
            builder.ToTable("Patients");
            builder.HasKey(p => p.PatientId);

            builder.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(p => p.Phone)
                .IsRequired()
                .HasMaxLength(11);

            builder.Property(p => p.Address).HasMaxLength(500);
            builder.Property(p => p.AllergyOtherText).HasMaxLength(500);
            builder.Property(p => p.SavedHistory).HasMaxLength(4000);

            builder.Property(p => p.Gender)
                .HasConversion<string>()
                .HasMaxLength(10);

            builder.Property(p => p.BloodGroup)
                .HasConversion<string>()
                .HasMaxLength(10);

            builder.Property(p => p.AllergyPresetId)
                .HasConversion<string>()
                .HasMaxLength(20);

            builder.HasIndex(p => p.RegisteredByStaffId);

            // FK to Staff without a navigation property on Patient — Patient only ever
            // needs the id for visibility-scope joins, never the loaded Staff object.
            builder.HasOne<Staff>()
                .WithMany()
                .HasForeignKey(p => p.RegisteredByStaffId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
