using MediatR;
using SylviaNG.Prescription.Application.Features.Analytics.Models;

namespace SylviaNG.Prescription.Application.Features.Analytics.Queries.GetDoctorLeaderboard
{
    public class GetDoctorLeaderboardQuery : IRequest<List<DoctorLeaderboardEntry>>
    {
    }
}
