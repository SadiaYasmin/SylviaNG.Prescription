using MediatR;
using SylviaNG.Prescription.Application.Features.Prescriptions.Models;

namespace SylviaNG.Prescription.Application.Features.Prescriptions.Queries.GetPrescriptionDetails
{
    /// <summary>US-031: read-only single-prescription view, any of Doctor/Staff/Admin per PrescriptionVisibilityScope.</summary>
    public class GetPrescriptionDetailsQuery : IRequest<PrescriptionDocumentResponse>
    {
        public string KeycloakId { get; set; }
        public long PrescriptionId { get; set; }

        public GetPrescriptionDetailsQuery(string keycloakId, long prescriptionId)
        {
            KeycloakId = keycloakId;
            PrescriptionId = prescriptionId;
        }
    }
}
