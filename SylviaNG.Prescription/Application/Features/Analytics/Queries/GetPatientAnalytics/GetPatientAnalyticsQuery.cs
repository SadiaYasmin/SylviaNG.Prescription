using MediatR;
using SylviaNG.Prescription.Application.Features.Analytics.Models;

namespace SylviaNG.Prescription.Application.Features.Analytics.Queries.GetPatientAnalytics
{
    public class GetPatientAnalyticsQuery : IRequest<PatientAnalyticsResponse>
    {
    }
}
