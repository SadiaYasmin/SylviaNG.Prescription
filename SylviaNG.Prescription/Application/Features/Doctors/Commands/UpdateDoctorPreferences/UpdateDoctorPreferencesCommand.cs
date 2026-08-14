using MediatR;
using SylviaNG.Prescription.Application.Features.Doctors.Models;

namespace SylviaNG.Prescription.Application.Features.Doctors.Commands.UpdateDoctorPreferences
{
    public class UpdateDoctorPreferencesCommand : IRequest<DoctorPreferencesResponse>
    {
        public string KeycloakId { get; set; }
        public UpdateDoctorPreferencesRequest Request { get; set; }

        public UpdateDoctorPreferencesCommand(string keycloakId, UpdateDoctorPreferencesRequest request)
        {
            KeycloakId = keycloakId;
            Request = request;
        }
    }
}
