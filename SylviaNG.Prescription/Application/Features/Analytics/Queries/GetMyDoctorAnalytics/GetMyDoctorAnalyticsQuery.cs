using MediatR;
using SylviaNG.Prescription.Application.Features.Analytics.Models;

namespace SylviaNG.Prescription.Application.Features.Analytics.Queries.GetMyDoctorAnalytics
{
    public class GetMyDoctorAnalyticsQuery : IRequest<MyDoctorAnalyticsResponse>
    {
        public string KeycloakId { get; set; }

        public GetMyDoctorAnalyticsQuery(string keycloakId)
        {
            KeycloakId = keycloakId;
        }
    }
}
