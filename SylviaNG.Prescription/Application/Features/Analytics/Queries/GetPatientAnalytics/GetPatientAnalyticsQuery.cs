using MediatR;
using SylviaNG.Prescription.Application.Features.Analytics.Models;

namespace SylviaNG.Prescription.Application.Features.Analytics.Queries.GetPatientAnalytics
{
    public class GetPatientAnalyticsQuery : IRequest<PatientAnalyticsResponse>
    {
        /// <summary>Day/Week/Month grouping for the New Registrations trend chart — the range it's bucketed within is <see cref="From"/>/<see cref="To"/>.</summary>
        public AnalyticsGranularity Granularity { get; set; }
        public DateTime From { get; set; }
        public DateTime To { get; set; }

        public GetPatientAnalyticsQuery(DateTime from, DateTime to, AnalyticsGranularity granularity = AnalyticsGranularity.Day)
        {
            From = from;
            To = to;
            Granularity = granularity;
        }
    }
}
