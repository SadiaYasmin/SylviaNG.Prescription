using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SylviaNG.Prescription.Domain.Entities;

namespace SylviaNG.Prescription.Infrastructure.Configurations
{
    public class MedicineConfiguration : IEntityTypeConfiguration<Medicine>
    {
        public void Configure(EntityTypeBuilder<Medicine> builder)
        {
            builder.ToTable("Medicines");
            builder.HasKey(m => m.MedicineId);

            builder.Property(m => m.BrandName)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(m => m.GenericName).HasMaxLength(200);
            builder.Property(m => m.Strength).HasMaxLength(50);
            builder.Property(m => m.Manufacturer).HasMaxLength(200);
            builder.Property(m => m.DosageForm).HasMaxLength(50);
            builder.Property(m => m.Route).HasMaxLength(100);
            builder.Property(m => m.Category).HasMaxLength(100);
            builder.Property(m => m.UnitPrice).HasPrecision(10, 2);
            builder.Property(m => m.DgdaRegistered).HasDefaultValue(false);

            builder.HasIndex(m => m.BrandName);
            builder.HasIndex(m => m.GenericName);
            builder.HasIndex(m => m.Active);
            builder.HasIndex(m => m.DgdaRegistered);
        }
    }
}
