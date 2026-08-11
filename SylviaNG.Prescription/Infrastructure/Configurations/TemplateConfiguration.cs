using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SylviaNG.Prescription.Domain.Entities;

namespace SylviaNG.Prescription.Infrastructure.Configurations
{
    public class TemplateConfiguration : IEntityTypeConfiguration<PrescriptionTemplate>
    {
        public void Configure(EntityTypeBuilder<PrescriptionTemplate> builder)
        {
            builder.ToTable("PrescriptionTemplates");
            builder.HasKey(t => t.TemplateId);

            builder.Property(t => t.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(t => t.Type)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(t => t.Language)
                .HasConversion<string>()
                .HasMaxLength(10)
                .IsRequired();

            // Unbounded/large text column — no HasMaxLength, so EF maps to each provider's
            // default unbounded string type (nvarchar(max) / text / nclob) rather than a
            // portability-breaking provider-specific HasColumnType.
            builder.Property(t => t.ConfigJson)
                .IsRequired();

            builder.HasIndex(t => t.Name);
        }
    }
}
