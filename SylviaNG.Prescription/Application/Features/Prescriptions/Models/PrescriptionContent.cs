namespace SylviaNG.Prescription.Application.Features.Prescriptions.Models
{
    /// <summary>
    /// The full editable clinical payload of a prescription (US-021/022/023) — shared shape
    /// between <see cref="SaveDraftPrescriptionRequest"/>'s request body and the section of
    /// <see cref="PrescriptionDocumentResponse"/> returned for authoring/view/verify, so the
    /// frontend can round-trip the same object it received.
    /// </summary>
    public class PrescriptionContent
    {
        public List<string> ChiefComplaints { get; set; } = new();
        public List<string> History { get; set; } = new();
        public ExaminationDto Examination { get; set; } = new();
        public List<DiagnosisItem> Diagnoses { get; set; } = new();
        public List<string> Investigations { get; set; } = new();
        public List<MedicineItem> Medicines { get; set; } = new();
        public List<string> Advice { get; set; } = new();
        public string? FollowUp { get; set; }
    }

    public class ExaminationDto
    {
        public string? Bp { get; set; }
        public string? Pulse { get; set; }
        public string? Temperature { get; set; }
        public string? RespiratoryRate { get; set; }
        public string? Spo2 { get; set; }
        public string? Weight { get; set; }
        public string? Height { get; set; }
        public string? BloodSugar { get; set; }
        public string? PainScore { get; set; }
        public string? HeartRate { get; set; }
    }
}
