using MediatR;
using SylviaNG.Prescription.Application.Features.Analytics.Models;

namespace SylviaNG.Prescription.Application.Features.Analytics.Queries.GetBusiestConsultationHours
{
    public class GetBusiestConsultationHoursQuery : IRequest<BusiestConsultationHoursResponse>
    {
    }
}
