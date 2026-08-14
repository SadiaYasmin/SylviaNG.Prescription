using MediatR;
using SylviaNG.Prescription.Application.Features.Prescriptions.Models;

namespace SylviaNG.Prescription.Application.Features.Prescriptions.Commands.StartOrResumePrescription
{
    public class StartOrResumePrescriptionCommand : IRequest<StartOrResumePrescriptionResponse>
    {
        public string KeycloakId { get; set; }
        public StartOrResumePrescriptionRequest Request { get; set; }

        public StartOrResumePrescriptionCommand(string keycloakId, StartOrResumePrescriptionRequest request)
        {
            KeycloakId = keycloakId;
            Request = request;
        }
    }
}
