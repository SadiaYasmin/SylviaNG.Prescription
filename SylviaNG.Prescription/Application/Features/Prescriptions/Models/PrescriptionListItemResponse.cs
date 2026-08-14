using SylviaNG.Prescription.Domain.Enums;

namespace SylviaNG.Prescription.Application.Features.Prescriptions.Models
{
    /// <summary>Row shape for the draft/finalized lists (US-029/030) and patient-history panel (US-032).</summary>
    public class PrescriptionListItemResponse
    {
        public long PrescriptionId { get; set; }
        public string DisplayCode { get; set; } = string.Empty;
        public long PatientId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public long DoctorId { get; set; }
        public string DoctorName { get; set; } = string.Empty;
        public PrescriptionStatusEnum Status { get; set; }
        public DateTime? SavedAt { get; set; }
        public DateTime? FinalizedAt { get; set; }
        public int DiagnosisCount { get; set; }
        public int MedicineCount { get; set; }
        public int InvestigationCount { get; set; }
    }

    public class PrescriptionListResponse
    {
        public List<PrescriptionListItemResponse> Prescriptions { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }
}
