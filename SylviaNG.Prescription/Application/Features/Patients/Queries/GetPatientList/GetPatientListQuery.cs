using MediatR;
using SylviaNG.Prescription.Application.Features.Patients.Models;

namespace SylviaNG.Prescription.Application.Features.Patients.Queries.GetPatientList
{
    /// <summary>
    /// One query handles all three roles (Staff/Doctor/Admin) rather than Staff's
    /// split-by-role query pair — the handler resolves the caller's role from
    /// <see cref="KeycloakId"/> and applies PatientVisibilityScope internally.
    /// </summary>
    public class GetPatientListQuery : IRequest<PatientListResponse>
    {
        public string KeycloakId { get; set; }
        public PatientListRequest Request { get; set; }

        public GetPatientListQuery(string keycloakId, PatientListRequest request)
        {
            KeycloakId = keycloakId;
            Request = request;
        }
    }
}
