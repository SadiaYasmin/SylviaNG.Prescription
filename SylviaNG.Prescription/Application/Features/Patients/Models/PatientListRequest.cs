using SylviaNG.Prescription.SharedKernel.Pagination;

namespace SylviaNG.Prescription.Application.Features.Patients.Models
{
    /// <summary>
    /// <see cref="PagedRequest"/> already carries <c>SearchTerm</c> (matched against Name
    /// OR Phone by <c>GetPatientListHandler</c>) plus paging — no Patient-specific filters
    /// exist yet, so this subclass exists purely for parity with Staff/Doctor's list
    /// request types and as a home for future filters (e.g. blood group).
    /// </summary>
    public class PatientListRequest : PagedRequest
    {
    }
}
