using MediatR;
using SylviaNG.Prescription.Application.Features.Prescriptions.Models;

namespace SylviaNG.Prescription.Application.Features.Prescriptions.Commands.AutoSavePrescription
{
    /// <summary>
    /// Background auto-save of an actively-authored (InProgress) prescription — pure data
    /// protection. Reuses <see cref="SaveDraftPrescriptionRequest"/> for the payload but,
    /// unlike Save-as-Draft, it never stamps SavedAt, never flips the linked Consultation,
    /// and never promotes the row out of InProgress — so an in-progress prescription stays
    /// out of the Draft Prescriptions list.
    /// </summary>
    public class AutoSavePrescriptionCommand : IRequest<AutoSavePrescriptionResponse>
    {
        public string KeycloakId { get; set; }
        public long PrescriptionId { get; set; }
        public SaveDraftPrescriptionRequest Request { get; set; }

        public AutoSavePrescriptionCommand(string keycloakId, long prescriptionId, SaveDraftPrescriptionRequest request)
        {
            KeycloakId = keycloakId;
            PrescriptionId = prescriptionId;
            Request = request;
        }
    }
}
