using MediatR;
using SylviaNG.Prescription.Application.Features.Consultations.Models;

namespace SylviaNG.Prescription.Application.Features.Consultations.Queries.GetTodaysQueue
{
    public class GetTodaysQueueQuery : IRequest<List<QueueItemResponse>>
    {
        public string KeycloakId { get; set; }

        public GetTodaysQueueQuery(string keycloakId)
        {
            KeycloakId = keycloakId;
        }
    }
}
