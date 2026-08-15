using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SylviaNG.Prescription.Domain.Entities;

namespace SylviaNG.Prescription.Infrastructure.Configurations
{
    public class DoctorQuickAddSeedStateConfiguration : IEntityTypeConfiguration<DoctorQuickAddSeedState>
    {
        public void Configure(EntityTypeBuilder<DoctorQuickAddSeedState> builder)
        {
            builder.ToTable("DoctorQuickAddSeedStates");
            builder.HasKey(s => new { s.DoctorId, s.SectionType });

            builder.Property(s => s.SectionType)
                .HasConversion<string>()
                .HasMaxLength(20);

            builder.HasOne<Doctor>()
                .WithMany()
                .HasForeignKey(s => s.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
