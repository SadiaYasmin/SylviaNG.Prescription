namespace SylviaNG.Prescription.Application.Features.Prescriptions.Models
{
    /// <summary>
    /// One diagnosis entry (US-021). <see cref="Icd10"/> is optional free text, matching the
    /// reference prototype's "trailing parenthesis" authoring convention on the frontend —
    /// the backend just stores whatever the two fields are handed.
    /// </summary>
    public class DiagnosisItem
    {
        public string Text { get; set; } = string.Empty;
        public string? Icd10 { get; set; }
    }

    /// <summary>
    /// One Rx line item (US-022). <see cref="Medicine"/> + <see cref="Strength"/> (normalized,
    /// case-insensitive) is the duplicate-guard key — see PrescriptionMappings/handlers.
    /// </summary>
    public class MedicineItem
    {
        public string Medicine { get; set; } = string.Empty;
        public string? Generic { get; set; }
        public string Strength { get; set; } = string.Empty;
        public string? Dosage { get; set; }
        public string? Frequency { get; set; }
        public string? Duration { get; set; }
        public string? Instructions { get; set; }
    }
}
