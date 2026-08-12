using MediatR;
using SylviaNG.Prescription.Application.Features.Consultations.Models;

namespace SylviaNG.Prescription.Application.Features.Consultations.Commands.OpenConsultation
{
    public class OpenConsultationCommand : IRequest<OpenConsultationResponse>
    {
        public long ConsultationId { get; set; }
        public string KeycloakId { get; set; }

        public OpenConsultationCommand(long consultationId, string keycloakId)
        {
            ConsultationId = consultationId;
            KeycloakId = keycloakId;
        }
    }
}
