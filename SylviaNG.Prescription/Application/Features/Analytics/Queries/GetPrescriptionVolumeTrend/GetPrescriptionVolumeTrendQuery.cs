using MediatR;
using SylviaNG.Prescription.Application.Features.Analytics.Models;

namespace SylviaNG.Prescription.Application.Features.Analytics.Queries.GetPrescriptionVolumeTrend
{
    /// <summary>
    /// US-074, also reused by US-072's embedded Day-granularity trend chart — one handler,
    /// not a duplicate per tab.
    /// </summary>
    public class GetPrescriptionVolumeTrendQuery : IRequest<PrescriptionVolumeTrendResponse>
    {
        public AnalyticsGranularity Granularity { get; set; }

        /// <summary>Inclusive UTC range bounds. Null on either end falls back to <see cref="AnalyticsDateBucketing.GetDefaultRange"/> for the chosen granularity (30 days / 12 weeks / 12 months).</summary>
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }

        public GetPrescriptionVolumeTrendQuery(AnalyticsGranularity granularity = AnalyticsGranularity.Day, DateTime? from = null, DateTime? to = null)
        {
            Granularity = granularity;
            From = from;
            To = to;
        }
    }
}
