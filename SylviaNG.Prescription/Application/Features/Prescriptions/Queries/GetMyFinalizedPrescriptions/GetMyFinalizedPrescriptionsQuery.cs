using MediatR;
using SylviaNG.Prescription.Application.Features.Prescriptions.Models;

namespace SylviaNG.Prescription.Application.Features.Prescriptions.Queries.GetMyFinalizedPrescriptions
{
    /// <summary>
    /// US-030: a doctor's own finalized prescriptions, newest first, filtered by patient
    /// name/Rx code and an optional FinalizedAt date range for the My Finalized
    /// Prescriptions list.
    /// </summary>
    public class GetMyFinalizedPrescriptionsQuery : IRequest<PrescriptionListResponse>
    {
        public string KeycloakId { get; set; }
        public string? SearchTerm { get; set; }
        public DateOnly? FromDate { get; set; }
        public DateOnly? ToDate { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;

        public GetMyFinalizedPrescriptionsQuery(
            string keycloakId,
            string? searchTerm = null,
            DateOnly? fromDate = null,
            DateOnly? toDate = null,
            int page = 1,
            int pageSize = 20)
        {
            KeycloakId = keycloakId;
            SearchTerm = searchTerm;
            FromDate = fromDate;
            ToDate = toDate;
            Page = page;
            PageSize = pageSize;
        }
    }
}
