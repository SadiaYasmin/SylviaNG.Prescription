using MediatR;
using SylviaNG.Prescription.Application.Features.Doctors.Models;

namespace SylviaNG.Prescription.Application.Features.Doctors.Commands.UpdateDoctorPhoto
{
    /// <summary>US-062. A null <see cref="UpdateDoctorPhotoRequest.PhotoBase64"/> removes the photo.</summary>
    public class UpdateDoctorPhotoCommand : IRequest<DoctorProfileResponse>
    {
        public string KeycloakId { get; set; }
        public UpdateDoctorPhotoRequest Request { get; set; }

        public UpdateDoctorPhotoCommand(string keycloakId, UpdateDoctorPhotoRequest request)
        {
            KeycloakId = keycloakId;
            Request = request;
        }
    }
}
