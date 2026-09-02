using MediatR;
using SylviaNG.Prescription.Application.Features.Analytics;
using SylviaNG.Prescription.Application.Features.Doctors.Models;

namespace SylviaNG.Prescription.Application.Features.Doctors.Queries.GetDoctorDetails
{
    public class GetDoctorDetailsQuery : IRequest<DoctorDetailsResponse>
    {
        public long DoctorId { get; set; }

        /// <summary>Day/Week/Month grouping for the Activity Trend chart, bucketed within <see cref="From"/>/<see cref="To"/>.</summary>
        public AnalyticsGranularity ActivityGranularity { get; set; }

        /// <summary>Date-range filter for every KPI/chart on the page except TodayPrescriptions/ThisMonthPrescriptions, which are always independent of this. Null on either end = all-time (used by callers like the manage-doctor edit form that only need the profile, not a filtered view).</summary>
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }

        public GetDoctorDetailsQuery(long doctorId, DateTime? from = null, DateTime? to = null, AnalyticsGranularity activityGranularity = AnalyticsGranularity.Day)
        {
            DoctorId = doctorId;
            From = from;
            To = to;
            ActivityGranularity = activityGranularity;
        }
    }
}
