using MediatR;
using SylviaNG.Prescription.Application.Features.Doctors.Models;

namespace SylviaNG.Prescription.Application.Features.Doctors.Queries.GetAssignedDoctorDetails
{
    /// <summary>
    /// A staff member's read-only detail view of ONE of their own assigned doctors. The
    /// handler verifies the doctor is actually assigned to the calling staff member before
    /// returning anything — a staff user must not be able to view an arbitrary doctor's
    /// details just by guessing a doctorId in the URL.
    /// </summary>
    public class GetAssignedDoctorDetailsQuery : IRequest<AssignedDoctorDetailsResponse>
    {
        public string KeycloakId { get; set; }
        public long DoctorId { get; set; }

        public GetAssignedDoctorDetailsQuery(string keycloakId, long doctorId)
        {
            KeycloakId = keycloakId;
            DoctorId = doctorId;
        }
    }
}
