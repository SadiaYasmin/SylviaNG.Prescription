using MediatR;
using SylviaNG.Prescription.Application.Features.Auth.Models;

namespace SylviaNG.Prescription.Application.Features.Auth.Commands.RequestEmailChange
{
    public class RequestEmailChangeCommand : IRequest<Unit>
    {
        public string KeycloakId { get; set; }
        public RequestEmailChangeRequest Request { get; set; }
        public RequestEmailChangeCommand(string keycloakId, RequestEmailChangeRequest request)
        {
            KeycloakId = keycloakId;
            Request = request;
        }
    }
}
