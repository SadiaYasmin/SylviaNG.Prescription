using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SylviaNG.Prescription.Domain.Entities;

namespace SylviaNG.Prescription.Infrastructure.Configurations
{
    public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
    {
        public void Configure(EntityTypeBuilder<Employee> builder)
        {
            builder.ToTable("Employees");
            builder.HasKey(e => e.EmployeeId);

            builder.Property(e => e.EmployeeName)
                .HasMaxLength(200);

            builder.Property(e => e.EmployeeCode)
                .HasMaxLength(50);

            // Indexes
            builder.HasIndex(e => e.EmployeeCode);
        }
    }
}
