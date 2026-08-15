using MediatR;
using SylviaNG.Prescription.Application.Features.Doctors.Models;

namespace SylviaNG.Prescription.Application.Features.Doctors.Queries.GetDoctorProfile
{
    public class GetDoctorProfileQuery : IRequest<DoctorProfileResponse>
    {
        public string KeycloakId { get; set; }

        public GetDoctorProfileQuery(string keycloakId)
        {
            KeycloakId = keycloakId;
        }
    }
}
