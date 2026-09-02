using MediatR;
using SylviaNG.Prescription.Application.Features.Analytics.Models;

namespace SylviaNG.Prescription.Application.Features.Analytics.Queries.GetBusiestConsultationHours
{
    public class GetBusiestConsultationHoursQuery : IRequest<BusiestConsultationHoursResponse>
    {
        public DateTime From { get; set; }
        public DateTime To { get; set; }

        public GetBusiestConsultationHoursQuery(DateTime from, DateTime to)
        {
            From = from;
            To = to;
        }
    }
}
