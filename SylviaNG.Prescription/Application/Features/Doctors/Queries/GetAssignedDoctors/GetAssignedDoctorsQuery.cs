using MediatR;
using SylviaNG.Prescription.Application.Features.Doctors.Models;

namespace SylviaNG.Prescription.Application.Features.Doctors.Queries.GetAssignedDoctors
{
    /// <summary>
    /// A staff member's own "assigned to me" view of doctors — the mirror of
    /// GetAssignedStaffQuery (which is a doctor's view of their assigned staff).
    /// </summary>
    public class GetAssignedDoctorsQuery : IRequest<AssignedDoctorListResponse>
    {
        public string KeycloakId { get; set; }

        public GetAssignedDoctorsQuery(string keycloakId)
        {
            KeycloakId = keycloakId;
        }
    }
}
