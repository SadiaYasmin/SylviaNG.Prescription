using MediatR;
using SylviaNG.Prescription.Application.Features.Patients.Models;

namespace SylviaNG.Prescription.Application.Features.Patients.Commands.CreatePatient
{
    /// <summary>
    /// Carries the caller's raw Keycloak subject id (resolved at the controller boundary,
    /// same as GetAssignedStaffQuery) so the handler — not the controller — resolves "which
    /// Staff member is registering this patient" and stays independently testable.
    /// </summary>
    public class CreatePatientCommand : IRequest<PatientSummaryResponse>
    {
        public string KeycloakId { get; set; }
        public CreatePatientRequest Request { get; set; }

        public CreatePatientCommand(string keycloakId, CreatePatientRequest request)
        {
            KeycloakId = keycloakId;
            Request = request;
        }
    }
}
