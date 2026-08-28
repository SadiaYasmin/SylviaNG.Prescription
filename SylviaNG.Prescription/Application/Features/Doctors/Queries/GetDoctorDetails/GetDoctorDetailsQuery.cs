using MediatR;
using SylviaNG.Prescription.Application.Features.Analytics;
using SylviaNG.Prescription.Application.Features.Doctors.Models;

namespace SylviaNG.Prescription.Application.Features.Doctors.Queries.GetDoctorDetails
{
    public class GetDoctorDetailsQuery : IRequest<DoctorDetailsResponse>
    {
        public long DoctorId { get; set; }

        /// <summary>Day/Week/Month grouping for the Activity Trend chart only — every other stat on this page is unaffected.</summary>
        public AnalyticsGranularity ActivityGranularity { get; set; }

        public GetDoctorDetailsQuery(long doctorId, AnalyticsGranularity activityGranularity = AnalyticsGranularity.Day)
        {
            DoctorId = doctorId;
            ActivityGranularity = activityGranularity;
        }
    }
}
