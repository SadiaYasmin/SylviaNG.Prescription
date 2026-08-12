using MediatR;
using SylviaNG.Prescription.Application.Features.Consultations.Models;

namespace SylviaNG.Prescription.Application.Features.Consultations.Queries.GetConsultationList
{
    /// <summary>
    /// Admin-only listing — no KeycloakId/visibility scoping needed since Admin sees
    /// everyone, same reasoning as GetStaffListQuery.
    /// </summary>
    public class GetConsultationListQuery : IRequest<ConsultationListResponse>
    {
        public ConsultationListRequest Request { get; set; }

        public GetConsultationListQuery(ConsultationListRequest request)
        {
            Request = request;
        }
    }
}
