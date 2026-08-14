using SylviaNG.Prescription.Application.Features.Patients.Models;
using SylviaNG.Prescription.Domain.Entities;
using SylviaNG.Prescription.Domain.Enums;

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

        public static DoctorPatientQueueItemResponse ToDoctorQueueItemResponse(
            this Patient patient,
            string registeredByName,
            long? todayConsultationId,
            ConsultationStatusEnum? todayConsultationStatus,
            long? todayPrescriptionId)
        {
            return new DoctorPatientQueueItemResponse
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
                RegisteredAt = patient.RegisteredAt,
                TodayConsultationId = todayConsultationId,
                TodayConsultationStatus = todayConsultationStatus,
                TodayPrescriptionId = todayPrescriptionId
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
