using SylviaNG.Prescription.Domain.Enums;

namespace SylviaNG.Prescription.Application.Features.Consultations.Models
{
    /// <summary>
    /// Lightweight response for a just-opened consultation, with the couple of joined
    /// display fields (patient/doctor name) a doctor's "start consultation" screen needs —
    /// mirrors the lightweight-joined-fields style of PatientDetailsResponse/
    /// StaffSummaryResponse rather than a heavier nested DTO.
    /// </summary>
    public class OpenConsultationResponse
    {
        public long ConsultationId { get; set; }
        public string DisplayCode { get; set; } = string.Empty;
        public string TokenNumber { get; set; } = string.Empty;
        public ConsultationStatusEnum Status { get; set; }
        public DateOnly VisitDate { get; set; }
        public long PatientId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public long DoctorId { get; set; }
        public string DoctorName { get; set; } = string.Empty;
    }
}
