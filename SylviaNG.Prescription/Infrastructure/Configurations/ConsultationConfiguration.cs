using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SylviaNG.Prescription.Domain.Entities;

namespace SylviaNG.Prescription.Infrastructure.Configurations
{
    public class ConsultationConfiguration : IEntityTypeConfiguration<Consultation>
    {
        public void Configure(EntityTypeBuilder<Consultation> builder)
        {
            builder.ToTable("Consultations");
            builder.HasKey(c => c.ConsultationId);

            builder.Property(c => c.DisplayCode)
                .IsRequired()
                .HasMaxLength(30);

            builder.Property(c => c.TokenNumber)
                .IsRequired()
                .HasMaxLength(10);

            builder.Property(c => c.Status)
                .HasConversion<string>()
                .HasMaxLength(20);

            builder.HasIndex(c => c.PatientId);
            builder.HasIndex(c => c.DoctorId);
            builder.HasIndex(c => c.RegisteredByStaffId);
            builder.HasIndex(c => c.VisitDate);
            builder.HasIndex(c => c.DisplayCode).IsUnique();

            // FK relationships with no navigation properties on Consultation itself — it
            // only ever needs the ids for visibility/queue joins, matching how
            // PatientConfiguration FK's to Staff without a Staff navigation on Patient.
            builder.HasOne<Patient>()
                .WithMany()
                .HasForeignKey(c => c.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<Doctor>()
                .WithMany()
                .HasForeignKey(c => c.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<Staff>()
                .WithMany()
                .HasForeignKey(c => c.RegisteredByStaffId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
