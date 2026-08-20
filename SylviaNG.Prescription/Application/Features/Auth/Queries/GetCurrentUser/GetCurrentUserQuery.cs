using MediatR;
using SylviaNG.Prescription.Application.Features.Auth.Models;

namespace SylviaNG.Prescription.Application.Features.Auth.Queries.GetCurrentUser
{
    public class GetCurrentUserQuery : IRequest<CurrentUserResponse>
    {
        public string KeycloakId { get; set; }
        public GetCurrentUserQuery(string keycloakId) => KeycloakId = keycloakId;
    }
}
