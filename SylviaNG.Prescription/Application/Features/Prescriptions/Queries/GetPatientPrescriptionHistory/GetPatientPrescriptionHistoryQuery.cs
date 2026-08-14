using MediatR;
using SylviaNG.Prescription.Application.Features.Prescriptions.Models;

namespace SylviaNG.Prescription.Application.Features.Prescriptions.Queries.GetPatientPrescriptionHistory
{
    /// <summary>US-032: a patient's past prescriptions, shown alongside the live authoring view.</summary>
    public class GetPatientPrescriptionHistoryQuery : IRequest<PrescriptionListResponse>
    {
        public string KeycloakId { get; set; }
        public long PatientId { get; set; }

        public GetPatientPrescriptionHistoryQuery(string keycloakId, long patientId)
        {
            KeycloakId = keycloakId;
            PatientId = patientId;
        }
    }
}
