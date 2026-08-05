using MediatR;
using SylviaNG.Prescription.Application.Features.Staffs.Models;

namespace SylviaNG.Prescription.Application.Features.Staffs.Queries.GetAssignedStaff
{
    /// <summary>
    /// US-059 "doctor views their own assigned staff, read-only". Takes the caller's
    /// Keycloak subject id (resolved from the JWT at the controller boundary, not
    /// looked up there) so the handler — not the controller — resolves "which doctor
    /// is this" and stays independently testable.
    /// </summary>
    public class GetAssignedStaffQuery : IRequest<StaffListResponse>
    {
        public string KeycloakId { get; set; }
        public string? SearchTerm { get; set; }

        public GetAssignedStaffQuery(string keycloakId, string? searchTerm = null)
        {
            KeycloakId = keycloakId;
            SearchTerm = searchTerm;
        }
    }
}
