using MediatR;
using SylviaNG.Prescription.Application.Features.Prescriptions.Models;

namespace SylviaNG.Prescription.Application.Features.Prescriptions.Commands.FinalizePrescription
{
    /// <summary>
    /// Finalize (US-028) accepts the same Language+Content shape as Save Draft — the doctor
    /// may have unsaved edits at the moment they click Finalize, so this both syncs content
    /// and flips status in one atomic transaction, rather than requiring a separate save first.
    /// </summary>
    public class FinalizePrescriptionCommand : IRequest<PrescriptionDocumentResponse>
    {
        public string KeycloakId { get; set; }
        public long PrescriptionId { get; set; }
        public SaveDraftPrescriptionRequest Request { get; set; }

        public FinalizePrescriptionCommand(string keycloakId, long prescriptionId, SaveDraftPrescriptionRequest request)
        {
            KeycloakId = keycloakId;
            PrescriptionId = prescriptionId;
            Request = request;
        }
    }
}
