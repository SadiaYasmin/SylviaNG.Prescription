using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SylviaNG.Prescription.Domain.Entities;

namespace SylviaNG.Prescription.Infrastructure.Configurations
{
    public class SequenceCounterConfiguration : IEntityTypeConfiguration<SequenceCounter>
    {
        public void Configure(EntityTypeBuilder<SequenceCounter> builder)
        {
            builder.ToTable("SequenceCounters");
            builder.HasKey(s => s.Id);

            builder.Property(s => s.CounterKey)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(s => s.PeriodKey)
                .IsRequired()
                .HasMaxLength(50);

            // The atomicity guarantee in SequenceGenerator relies entirely on this unique
            // index backing a Postgres ON CONFLICT upsert — without it the upsert has
            // nothing to conflict on and silently degrades to plain inserts.
            builder.HasIndex(s => new { s.CounterKey, s.PeriodKey }).IsUnique();
        }
    }
}
