using MediatR;

namespace SylviaNG.Prescription.Application.Features.Auth.Commands.RequestPasswordChange
{
    public class RequestPasswordChangeCommand : IRequest<Unit>
    {
        public string KeycloakId { get; set; }
        public RequestPasswordChangeCommand(string keycloakId) => KeycloakId = keycloakId;
    }
}
