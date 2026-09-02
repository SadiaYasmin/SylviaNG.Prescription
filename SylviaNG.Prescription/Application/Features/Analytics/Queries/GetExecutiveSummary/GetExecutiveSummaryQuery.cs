using MediatR;
using SylviaNG.Prescription.Application.Features.Analytics.Models;

namespace SylviaNG.Prescription.Application.Features.Analytics.Queries.GetExecutiveSummary
{
    public class GetExecutiveSummaryQuery : IRequest<ExecutiveSummaryResponse>
    {
        /// <summary>Inclusive-start/exclusive-end UTC range for all period-sensitive fields on this response. Does not affect the master counts (TotalPatients/TotalDoctors/TotalStaff/TotalMedicines).</summary>
        public DateTime From { get; set; }
        public DateTime To { get; set; }

        public GetExecutiveSummaryQuery(DateTime from, DateTime to)
        {
            From = from;
            To = to;
        }
    }
}
