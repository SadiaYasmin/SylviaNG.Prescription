using MediatR;
using SylviaNG.Prescription.Application.Features.Consultations.Models;

namespace SylviaNG.Prescription.Application.Features.Consultations.Commands.CreateConsultation
{
    /// <summary>
    /// Carries the caller's raw Keycloak subject id (resolved at the controller boundary)
    /// so the handler resolves "which Staff member is checking this patient in" and stays
    /// independently testable — same pattern as CreatePatientCommand.
    /// </summary>
    public class CreateConsultationCommand : IRequest<CreateConsultationResponse>
    {
        public string KeycloakId { get; set; }
        public CreateConsultationRequest Request { get; set; }

        public CreateConsultationCommand(string keycloakId, CreateConsultationRequest request)
        {
            KeycloakId = keycloakId;
            Request = request;
        }
    }
}
