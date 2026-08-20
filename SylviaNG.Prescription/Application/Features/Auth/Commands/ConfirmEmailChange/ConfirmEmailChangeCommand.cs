using MediatR;
using SylviaNG.Prescription.Application.Features.Auth.Models;

namespace SylviaNG.Prescription.Application.Features.Auth.Commands.ConfirmEmailChange
{
    public class ConfirmEmailChangeCommand : IRequest<Unit>
    {
        public string KeycloakId { get; set; }
        public ConfirmEmailChangeRequest Request { get; set; }
        public ConfirmEmailChangeCommand(string keycloakId, ConfirmEmailChangeRequest request)
        {
            KeycloakId = keycloakId;
            Request = request;
        }
    }
}
