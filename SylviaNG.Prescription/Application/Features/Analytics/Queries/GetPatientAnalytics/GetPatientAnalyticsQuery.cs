using MediatR;
using SylviaNG.Prescription.Application.Features.Analytics.Models;

namespace SylviaNG.Prescription.Application.Features.Analytics.Queries.GetPatientAnalytics
{
    public class GetPatientAnalyticsQuery : IRequest<PatientAnalyticsResponse>
    {
        /// <summary>Day/Week/Month grouping for the New Registrations trend chart only — the rest of this response is unaffected.</summary>
        public AnalyticsGranularity Granularity { get; set; }

        public GetPatientAnalyticsQuery(AnalyticsGranularity granularity = AnalyticsGranularity.Day)
        {
            Granularity = granularity;
        }
    }
}
