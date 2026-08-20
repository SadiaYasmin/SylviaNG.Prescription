using SylviaNG.Prescription.Application.Features.Staffs.Models;
using SylviaNG.Prescription.Domain.Entities;

namespace SylviaNG.Prescription.Application.Mappings
{
    public static class StaffMappings
    {
        public static StaffSummaryResponse ToSummaryResponse(this Staff staff, User user, List<AssignedDoctorSummary> assignedDoctors)
        {
            return new StaffSummaryResponse
            {
                StaffId = staff.StaffId,
                UserId = user.UserId,
                FullName = staff.FullName,
                Username = user.Username,
                Email = user.Email,
                Phone = staff.Phone,
                Departments = assignedDoctors
                    .Where(d => !string.IsNullOrWhiteSpace(d.Department))
                    .Select(d => d.Department!)
                    .Distinct()
                    .OrderBy(d => d)
                    .ToList(),
                IsActive = user.IsActive,
                AssignedDoctors = assignedDoctors
            };
        }
    }
}
