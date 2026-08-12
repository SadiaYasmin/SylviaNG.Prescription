using MediatR;
using SylviaNG.Prescription.Application.Features.Consultations.Models;

namespace SylviaNG.Prescription.Application.Features.Consultations.Queries.GetConsultationDetails
{
    public class GetConsultationDetailsQuery : IRequest<ConsultationDetailsResponse>
    {
        public long ConsultationId { get; set; }

        public GetConsultationDetailsQuery(long consultationId)
        {
            ConsultationId = consultationId;
        }
    }
}
