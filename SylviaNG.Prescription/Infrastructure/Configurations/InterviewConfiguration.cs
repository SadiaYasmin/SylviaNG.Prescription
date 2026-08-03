using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SylviaNG.Prescription.Domain.Entities;

namespace SylviaNG.Prescription.Infrastructure.Configurations
{
    public class InterviewConfiguration : IEntityTypeConfiguration<Interview>
    {
        public void Configure(EntityTypeBuilder<Interview> builder)
        {
            builder.ToTable("Interviews");
            builder.HasKey(i => i.InterviewId);

            builder.Property(i => i.Location)
                .HasMaxLength(300);

            builder.Property(i => i.MeetingLink)
                .HasMaxLength(500);

            builder.Property(i => i.Round)
                .HasMaxLength(100);

            builder.Property(i => i.Feedback)
                .HasColumnType("text");

            builder.Property(i => i.Result)
                .HasMaxLength(100);

            // Indexes
            builder.HasIndex(i => i.JobApplicationId);
        }
    }
}
