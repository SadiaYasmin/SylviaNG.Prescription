using SylviaNG.Prescription.Application.Features.Consultations.Models;

namespace SylviaNG.Prescription.Application.Features.Prescriptions.Models
{
    /// <summary>
    /// Either a guard prompt (US-011/012, when <see cref="StartOrResumePrescriptionRequest.Force"/>
    /// wasn't set) or the ready-to-author <see cref="Document"/> — never both. Mirrors
    /// CreateConsultationResponse's DuplicateFound shape for the same kind of prompt-before-proceed UX.
    /// </summary>
    public class StartOrResumePrescriptionResponse
    {
        public bool DuplicateActiveFound { get; set; }
        public ConsultationSummaryResponse? ExistingActiveConsultation { get; set; }

        public bool UnfinishedDraftFound { get; set; }
        public List<PrescriptionListItemResponse> UnfinishedDrafts { get; set; } = new();

        public PrescriptionDocumentResponse? Document { get; set; }
    }
}
