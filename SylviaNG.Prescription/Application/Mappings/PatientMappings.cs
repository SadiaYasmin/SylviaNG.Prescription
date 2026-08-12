using SylviaNG.Prescription.Application.Features.Patients.Models;
using SylviaNG.Prescription.Domain.Entities;

namespace SylviaNG.Prescription.Application.Mappings
{
    public static class PatientMappings
    {
        public static PatientSummaryResponse ToSummaryResponse(this Patient patient, string registeredByName)
        {
            return new PatientSummaryResponse
            {
                PatientId = patient.PatientId,
                Name = patient.Name,
                Phone = patient.Phone,
                DateOfBirth = patient.DateOfBirth,
                Age = patient.Age,
                Gender = patient.Gender,
                Address = patient.Address,
                BloodGroup = patient.BloodGroup,
                AllergyPresetId = patient.AllergyPresetId,
                AllergyOtherText = patient.AllergyOtherText,
                SavedHistory = patient.SavedHistory,
                RegisteredByStaffId = patient.RegisteredByStaffId,
                RegisteredByName = registeredByName,
                RegisteredAt = patient.RegisteredAt
            };
        }

        public static PatientDetailsResponse ToDetailsResponse(this Patient patient, string registeredByName)
        {
            return new PatientDetailsResponse
            {
                Profile = patient.ToSummaryResponse(registeredByName)
            };
        }
    }
}
