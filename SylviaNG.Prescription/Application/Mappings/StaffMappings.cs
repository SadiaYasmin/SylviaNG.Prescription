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
                Department = staff.Department,
                IsActive = user.IsActive,
                AssignedDoctors = assignedDoctors
            };
        }
    }
}
