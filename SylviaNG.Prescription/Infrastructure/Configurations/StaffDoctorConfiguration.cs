using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SylviaNG.Prescription.Domain.Entities;

namespace SylviaNG.Prescription.Infrastructure.Configurations
{
    public class StaffDoctorConfiguration : IEntityTypeConfiguration<StaffDoctor>
    {
        public void Configure(EntityTypeBuilder<StaffDoctor> builder)
        {
            builder.ToTable("StaffDoctors");
            builder.HasKey(sd => sd.StaffDoctorId);

            builder.HasIndex(sd => new { sd.StaffId, sd.DoctorId }).IsUnique();

            builder.HasOne(sd => sd.Staff)
                .WithMany(s => s.StaffDoctors)
                .HasForeignKey(sd => sd.StaffId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(sd => sd.Doctor)
                .WithMany()
                .HasForeignKey(sd => sd.DoctorId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
