using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SylviaNG.Prescription.Domain.Entities;

namespace SylviaNG.Prescription.Infrastructure.Configurations
{
    public class PrescriptionConfiguration : IEntityTypeConfiguration<PrescriptionRecord>
    {
        public void Configure(EntityTypeBuilder<PrescriptionRecord> builder)
        {
            builder.ToTable("Prescriptions");
            builder.HasKey(p => p.PrescriptionId);

            builder.Property(p => p.DisplayCode)
                .IsRequired()
                .HasMaxLength(30);

            builder.Property(p => p.Language)
                .HasConversion<string>()
                .HasMaxLength(10);

            builder.Property(p => p.Status)
                .HasConversion<string>()
                .HasMaxLength(20);

            // Unbounded text columns, same reasoning as PrescriptionTemplate.ConfigJson —
            // portable across the SqlServer/Npgsql/Oracle providers this project references.
            builder.Property(p => p.ChiefComplaintsJson).IsRequired();
            builder.Property(p => p.HistoryJson).IsRequired();
            builder.Property(p => p.DiagnosesJson).IsRequired();
            builder.Property(p => p.InvestigationsJson).IsRequired();
            builder.Property(p => p.MedicinesJson).IsRequired();
            builder.Property(p => p.AdviceJson).IsRequired();

            builder.HasIndex(p => p.DisplayCode).IsUnique();
            builder.HasIndex(p => p.ConsultationId).IsUnique();
            builder.HasIndex(p => p.PatientId);
            builder.HasIndex(p => p.DoctorId);
            builder.HasIndex(p => p.Status);

            // Ids-only FKs, no navigation properties — same style as ConsultationConfiguration.
            builder.HasOne<Consultation>()
                .WithMany()
                .HasForeignKey(p => p.ConsultationId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<Patient>()
                .WithMany()
                .HasForeignKey(p => p.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<Doctor>()
                .WithMany()
                .HasForeignKey(p => p.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<PrescriptionTemplate>()
                .WithMany()
                .HasForeignKey(p => p.TemplateId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
