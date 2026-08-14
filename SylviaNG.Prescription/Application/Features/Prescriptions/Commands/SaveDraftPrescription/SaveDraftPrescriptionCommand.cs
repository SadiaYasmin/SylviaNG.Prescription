using MediatR;
using SylviaNG.Prescription.Application.Features.Prescriptions.Models;

namespace SylviaNG.Prescription.Application.Features.Prescriptions.Commands.SaveDraftPrescription
{
    public class SaveDraftPrescriptionCommand : IRequest<PrescriptionDocumentResponse>
    {
        public string KeycloakId { get; set; }
        public long PrescriptionId { get; set; }
        public SaveDraftPrescriptionRequest Request { get; set; }

        public SaveDraftPrescriptionCommand(string keycloakId, long prescriptionId, SaveDraftPrescriptionRequest request)
        {
            KeycloakId = keycloakId;
            PrescriptionId = prescriptionId;
            Request = request;
        }
    }
}
