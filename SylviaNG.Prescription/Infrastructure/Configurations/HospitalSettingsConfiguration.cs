using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SylviaNG.Prescription.Domain.Entities;

namespace SylviaNG.Prescription.Infrastructure.Configurations
{
    public class HospitalSettingsConfiguration : IEntityTypeConfiguration<HospitalSettings>
    {
        public void Configure(EntityTypeBuilder<HospitalSettings> builder)
        {
            builder.ToTable("HospitalSettings");
            builder.HasKey(h => h.HospitalSettingsId);

            builder.Property(h => h.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(h => h.Address)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(h => h.Phone)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(h => h.EmergencyNumber).HasMaxLength(20);
            builder.Property(h => h.Email).HasMaxLength(200);
            builder.Property(h => h.Website).HasMaxLength(200);
            builder.Property(h => h.Slogan).HasMaxLength(300);
            builder.Property(h => h.SloganBn).HasMaxLength(300);
            builder.Property(h => h.LicenseNumber).HasMaxLength(100);

            // US-083: relative URLs (e.g. "/uploads/hospital-logo/{guid}.png"), not base64 blobs.
            builder.Property(h => h.LogoUrl).HasMaxLength(500);
            builder.Property(h => h.SealUrl).HasMaxLength(500);
        }
    }
}
