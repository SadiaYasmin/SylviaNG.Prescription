using MediatR;
using SylviaNG.Prescription.Application.Features.Doctors.Models;

namespace SylviaNG.Prescription.Application.Features.Doctors.Queries.GetDoctorPreferences
{
    public class GetDoctorPreferencesQuery : IRequest<DoctorPreferencesResponse>
    {
        public string KeycloakId { get; set; }

        public GetDoctorPreferencesQuery(string keycloakId)
        {
            KeycloakId = keycloakId;
        }
    }
}
