using SylviaNG.Prescription.Domain.Enums;

namespace SylviaNG.Prescription.Application.Features.Consultations.Models
{
    public class ConsultationListItemResponse
    {
        public long ConsultationId { get; set; }
        public string DisplayCode { get; set; } = string.Empty;
        public string TokenNumber { get; set; } = string.Empty;
        public ConsultationStatusEnum Status { get; set; }
        public DateOnly VisitDate { get; set; }
        public DateTime CheckInAt { get; set; }
        public long PatientId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string PatientPhone { get; set; } = string.Empty;
        public long DoctorId { get; set; }
        public string DoctorName { get; set; } = string.Empty;
    }
}
