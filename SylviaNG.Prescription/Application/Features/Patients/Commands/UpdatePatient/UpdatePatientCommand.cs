using MediatR;
using SylviaNG.Prescription.Application.Features.Patients.Models;

namespace SylviaNG.Prescription.Application.Features.Patients.Commands.UpdatePatient
{
    public class UpdatePatientCommand : IRequest<PatientSummaryResponse>
    {
        public long PatientId { get; set; }
        public string KeycloakId { get; set; }
        public UpdatePatientRequest Request { get; set; }

        public UpdatePatientCommand(long patientId, string keycloakId, UpdatePatientRequest request)
        {
            PatientId = patientId;
            KeycloakId = keycloakId;
            Request = request;
        }
    }
}
