using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SylviaNG.Prescription.Domain.Entities;

namespace SylviaNG.Prescription.Infrastructure.Configurations
{
    public class VerificationCodeConfiguration : IEntityTypeConfiguration<VerificationCode>
    {
        public void Configure(EntityTypeBuilder<VerificationCode> builder)
        {
            builder.ToTable("VerificationCodes");
            builder.HasKey(v => v.VerificationCodeId);

            builder.Property(v => v.Email).IsRequired().HasMaxLength(256);
            builder.Property(v => v.CodeHash).IsRequired().HasMaxLength(128);
            builder.Property(v => v.NewValue).HasMaxLength(256);

            builder.Property(v => v.Purpose)
                .HasConversion<string>()
                .HasMaxLength(20);

            builder.HasIndex(v => new { v.Email, v.Purpose, v.ConsumedAt });
        }
    }
}
