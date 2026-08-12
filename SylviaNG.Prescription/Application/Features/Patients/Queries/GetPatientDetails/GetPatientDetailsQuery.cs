using MediatR;
using SylviaNG.Prescription.Application.Features.Patients.Models;

namespace SylviaNG.Prescription.Application.Features.Patients.Queries.GetPatientDetails
{
    public class GetPatientDetailsQuery : IRequest<PatientDetailsResponse>
    {
        public long PatientId { get; set; }
        public string KeycloakId { get; set; }

        public GetPatientDetailsQuery(long patientId, string keycloakId)
        {
            PatientId = patientId;
            KeycloakId = keycloakId;
        }
    }
}
