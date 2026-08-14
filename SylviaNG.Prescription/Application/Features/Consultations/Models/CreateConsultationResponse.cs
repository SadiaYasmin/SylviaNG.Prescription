using SylviaNG.Prescription.Application.Features.Prescriptions.Models;

namespace SylviaNG.Prescription.Application.Features.Consultations.Models
{
    /// <summary>
    /// <see cref="DuplicateFound"/> is the branch signal for the frontend: when true, no row
    /// was created — <see cref="ExistingConsultation"/> carries the already-queued
    /// consultation so the UI can explain why creation was rejected. Duplicate creation is
    /// never overridden for active consultations (<see cref="CreateConsultationRequest.Force"/>
    /// is ignored for this check). <see cref="UnfinishedDraftFound"/> (US-012, Epic D) is the
    /// second, separate prompt — unlike DuplicateFound, this one respects Force so staff can
    /// explicitly proceed after seeing the prompt. When neither guard fires,
    /// <see cref="Consultation"/> carries the newly created row.
    /// </summary>
    public class CreateConsultationResponse
    {
        public bool DuplicateFound { get; set; }
        public ConsultationSummaryResponse? Consultation { get; set; }
        public ConsultationSummaryResponse? ExistingConsultation { get; set; }

        public bool UnfinishedDraftFound { get; set; }
        public List<PrescriptionListItemResponse> UnfinishedDrafts { get; set; } = new();
    }
}
