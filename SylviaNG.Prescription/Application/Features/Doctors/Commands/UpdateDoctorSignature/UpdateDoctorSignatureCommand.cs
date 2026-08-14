using MediatR;
using SylviaNG.Prescription.Application.Features.Doctors.Models;

namespace SylviaNG.Prescription.Application.Features.Doctors.Commands.UpdateDoctorSignature
{
    public class UpdateDoctorSignatureCommand : IRequest<DoctorPreferencesResponse>
    {
        public string KeycloakId { get; set; }
        public UpdateDoctorSignatureRequest Request { get; set; }

        public UpdateDoctorSignatureCommand(string keycloakId, UpdateDoctorSignatureRequest request)
        {
            KeycloakId = keycloakId;
            Request = request;
        }
    }
}
