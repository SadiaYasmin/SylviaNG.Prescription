using MediatR;
using SylviaNG.Prescription.Application.Features.Doctors.Models;

namespace SylviaNG.Prescription.Application.Features.Doctors.Commands.UpdateDoctorProfile
{
    public class UpdateDoctorProfileCommand : IRequest<DoctorProfileResponse>
    {
        public string KeycloakId { get; set; }
        public UpdateDoctorProfileRequest Request { get; set; }

        public UpdateDoctorProfileCommand(string keycloakId, UpdateDoctorProfileRequest request)
        {
            KeycloakId = keycloakId;
            Request = request;
        }
    }
}
