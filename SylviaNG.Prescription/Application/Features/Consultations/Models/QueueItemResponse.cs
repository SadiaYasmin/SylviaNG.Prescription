using SylviaNG.Prescription.Domain.Enums;

namespace SylviaNG.Prescription.Application.Features.Consultations.Models
{
    /// <summary>
    /// One row of a same-day queue, shared by both GetTodaysQueue (Doctor) and GetMyQueue
    /// (Staff) — both need the same joined patient/doctor display fields, just filtered
    /// differently, so one shape covers both rather than two near-identical DTOs.
    /// </summary>
    public class QueueItemResponse
    {
        public long ConsultationId { get; set; }
        public string DisplayCode { get; set; } = string.Empty;
        public string TokenNumber { get; set; } = string.Empty;
        public ConsultationStatusEnum Status { get; set; }
        public DateTime CheckInAt { get; set; }
        public long PatientId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public long DoctorId { get; set; }
        public string DoctorName { get; set; } = string.Empty;
    }
}
