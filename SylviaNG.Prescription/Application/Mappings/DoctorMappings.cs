using SylviaNG.Prescription.Application.Features.Doctors.Models;
using SylviaNG.Prescription.Domain.Entities;

namespace SylviaNG.Prescription.Application.Mappings
{
    public static class DoctorMappings
    {
        public static DoctorSummaryResponse ToSummaryResponse(this Doctor doctor, User user)
        {
            return new DoctorSummaryResponse
            {
                DoctorId = doctor.DoctorId,
                UserId = user.UserId,
                FullName = doctor.FullName,
                Username = user.Username,
                Email = user.Email,
                Phone = doctor.Phone,
                Qualification = doctor.Qualification,
                Department = doctor.Department,
                LicenseNumber = doctor.LicenseNumber,
                Specialization = doctor.Specialization,
                ExperienceYears = doctor.ExperienceYears,
                Gender = doctor.Gender,
                JoiningDate = doctor.JoiningDate,
                PhotoUrl = doctor.PhotoUrl,
                IsActive = user.IsActive
            };
        }
    }
}
