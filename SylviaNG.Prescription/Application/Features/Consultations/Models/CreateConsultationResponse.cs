namespace SylviaNG.Prescription.Application.Features.Consultations.Models
{
    /// <summary>
    /// <see cref="DuplicateFound"/> is the branch signal for the frontend: when true, no row
    /// was created — <see cref="ExistingConsultation"/> carries the already-queued
    /// consultation so the UI can explain why creation was rejected. Duplicate creation is
    /// never overridden for active consultations. When false,
    /// <see cref="Consultation"/> carries the newly created row.
    /// </summary>
    public class CreateConsultationResponse
    {
        public bool DuplicateFound { get; set; }
        public ConsultationSummaryResponse? Consultation { get; set; }
        public ConsultationSummaryResponse? ExistingConsultation { get; set; }
    }
}
