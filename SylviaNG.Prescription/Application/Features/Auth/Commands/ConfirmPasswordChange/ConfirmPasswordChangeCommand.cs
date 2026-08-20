using MediatR;
using SylviaNG.Prescription.Application.Features.Auth.Models;

namespace SylviaNG.Prescription.Application.Features.Auth.Commands.ConfirmPasswordChange
{
    public class ConfirmPasswordChangeCommand : IRequest<Unit>
    {
        public string KeycloakId { get; set; }
        public ConfirmPasswordChangeRequest Request { get; set; }
        public ConfirmPasswordChangeCommand(string keycloakId, ConfirmPasswordChangeRequest request)
        {
            KeycloakId = keycloakId;
            Request = request;
        }
    }
}
