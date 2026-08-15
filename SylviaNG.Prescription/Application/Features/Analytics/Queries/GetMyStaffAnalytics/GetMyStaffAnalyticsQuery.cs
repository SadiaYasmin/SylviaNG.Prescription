using MediatR;
using SylviaNG.Prescription.Application.Features.Analytics.Models;

namespace SylviaNG.Prescription.Application.Features.Analytics.Queries.GetMyStaffAnalytics
{
    public class GetMyStaffAnalyticsQuery : IRequest<MyStaffAnalyticsResponse>
    {
        public string KeycloakId { get; set; }

        public GetMyStaffAnalyticsQuery(string keycloakId)
        {
            KeycloakId = keycloakId;
        }
    }
}
