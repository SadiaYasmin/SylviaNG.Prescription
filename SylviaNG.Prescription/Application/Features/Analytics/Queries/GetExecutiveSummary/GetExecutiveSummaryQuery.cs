using MediatR;
using SylviaNG.Prescription.Application.Features.Analytics.Models;

namespace SylviaNG.Prescription.Application.Features.Analytics.Queries.GetExecutiveSummary
{
    public class GetExecutiveSummaryQuery : IRequest<ExecutiveSummaryResponse>
    {
    }
}
