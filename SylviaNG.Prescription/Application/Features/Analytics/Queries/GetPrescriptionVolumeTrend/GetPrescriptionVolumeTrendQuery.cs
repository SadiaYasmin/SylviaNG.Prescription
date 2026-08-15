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

        public GetPrescriptionVolumeTrendQuery(AnalyticsGranularity granularity = AnalyticsGranularity.Day)
        {
            Granularity = granularity;
        }
    }
}
