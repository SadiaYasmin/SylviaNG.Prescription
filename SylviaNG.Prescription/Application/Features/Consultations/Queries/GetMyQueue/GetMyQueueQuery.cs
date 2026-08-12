using MediatR;
using SylviaNG.Prescription.Application.Features.Consultations.Models;

namespace SylviaNG.Prescription.Application.Features.Consultations.Queries.GetMyQueue
{
    public class GetMyQueueQuery : IRequest<List<QueueItemResponse>>
    {
        public string KeycloakId { get; set; }

        public GetMyQueueQuery(string keycloakId)
        {
            KeycloakId = keycloakId;
        }
    }
}
