using MediatR;
using SylviaNG.Prescription.Application.Features.Consultations.Models;

namespace SylviaNG.Prescription.Application.Features.Consultations.Queries.GetMyAssignedDoctors
{
    public class GetMyAssignedDoctorsQuery : IRequest<List<AssignedDoctorSummaryResponse>>
    {
        public string KeycloakId { get; set; }

        public GetMyAssignedDoctorsQuery(string keycloakId)
        {
            KeycloakId = keycloakId;
        }
    }
}
