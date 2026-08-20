using SylviaNG.Prescription.Application.Features.Consultations.Models;
using SylviaNG.Prescription.Domain.Entities;

namespace SylviaNG.Prescription.Application.Mappings
{
    public static class ConsultationMappings
    {
        public static ConsultationSummaryResponse ToSummaryResponse(this Consultation consultation)
        {
            return new ConsultationSummaryResponse
            {
                ConsultationId = consultation.ConsultationId,
                DisplayCode = consultation.DisplayCode,
                TokenNumber = consultation.TokenNumber,
                Status = consultation.Status
            };
        }

        public static OpenConsultationResponse ToOpenResponse(this Consultation consultation, string patientName, string doctorName)
        {
            return new OpenConsultationResponse
            {
                ConsultationId = consultation.ConsultationId,
                DisplayCode = consultation.DisplayCode,
                TokenNumber = consultation.TokenNumber,
                Status = consultation.Status,
                VisitDate = consultation.VisitDate,
                PatientId = consultation.PatientId,
                PatientName = patientName,
                DoctorId = consultation.DoctorId,
                DoctorName = doctorName
            };
        }

        public static QueueItemResponse ToQueueItemResponse(this Consultation consultation, string patientName, string doctorName, bool hasSavedDraft = false)
        {
            return new QueueItemResponse
            {
                ConsultationId = consultation.ConsultationId,
                DisplayCode = consultation.DisplayCode,
                TokenNumber = consultation.TokenNumber,
                Status = consultation.Status,
                CheckInAt = consultation.CheckInAt,
                PatientId = consultation.PatientId,
                PatientName = patientName,
                DoctorId = consultation.DoctorId,
                DoctorName = doctorName,
                HasSavedDraft = hasSavedDraft
            };
        }

        public static ConsultationListItemResponse ToListItemResponse(this Consultation consultation, string patientName, string patientPhone, string doctorName)
        {
            return new ConsultationListItemResponse
            {
                ConsultationId = consultation.ConsultationId,
                DisplayCode = consultation.DisplayCode,
                TokenNumber = consultation.TokenNumber,
                Status = consultation.Status,
                VisitDate = consultation.VisitDate,
                CheckInAt = consultation.CheckInAt,
                PatientId = consultation.PatientId,
                PatientName = patientName,
                PatientPhone = patientPhone,
                DoctorId = consultation.DoctorId,
                DoctorName = doctorName
            };
        }

        public static ConsultationDetailsResponse ToDetailsResponse(
            this Consultation consultation, string patientName, string patientPhone, string doctorName, string registeredByName)
        {
            return new ConsultationDetailsResponse
            {
                ConsultationId = consultation.ConsultationId,
                DisplayCode = consultation.DisplayCode,
                TokenNumber = consultation.TokenNumber,
                Status = consultation.Status,
                VisitDate = consultation.VisitDate,
                CheckInAt = consultation.CheckInAt,
                PatientId = consultation.PatientId,
                PatientName = patientName,
                PatientPhone = patientPhone,
                DoctorId = consultation.DoctorId,
                DoctorName = doctorName,
                RegisteredByStaffId = consultation.RegisteredByStaffId,
                RegisteredByName = registeredByName
            };
        }
    }
}
