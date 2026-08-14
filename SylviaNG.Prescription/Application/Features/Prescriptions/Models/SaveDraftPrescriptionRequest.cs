using SylviaNG.Prescription.Domain.Enums;

namespace SylviaNG.Prescription.Application.Features.Prescriptions.Models
{
    /// <summary>Full section payload for Save as Draft (US-027) and the pre-finalize content sync.</summary>
    public class SaveDraftPrescriptionRequest
    {
        public TemplateLanguageEnum Language { get; set; }
        public PrescriptionContent Content { get; set; } = new();
    }
}
