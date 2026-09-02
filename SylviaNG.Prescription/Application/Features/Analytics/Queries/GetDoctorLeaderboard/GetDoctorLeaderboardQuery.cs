using MediatR;
using SylviaNG.Prescription.Application.Features.Analytics.Models;

namespace SylviaNG.Prescription.Application.Features.Analytics.Queries.GetDoctorLeaderboard
{
    public class GetDoctorLeaderboardQuery : IRequest<List<DoctorLeaderboardEntry>>
    {
        public DateTime From { get; set; }
        public DateTime To { get; set; }

        public GetDoctorLeaderboardQuery(DateTime from, DateTime to)
        {
            From = from;
            To = to;
        }
    }
}
