using MediatR;
using SylviaNG.Prescription.Application.Features.Prescriptions.Models;

namespace SylviaNG.Prescription.Application.Features.Prescriptions.Queries.GetMyDraftPrescriptions
{
    /// <summary>
    /// US-029: a doctor's own drafts, optionally narrowed to one patient (deep-link from the
    /// patient-history panel), or filtered by patient name/Rx code/phone and a single saved
    /// date for the Saved Draft Prescriptions list.
    /// </summary>
    public class GetMyDraftPrescriptionsQuery : IRequest<PrescriptionListResponse>
    {
        public string KeycloakId { get; set; }
        public long? PatientId { get; set; }
        public string? SearchTerm { get; set; }
        public DateOnly? Date { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;

        public GetMyDraftPrescriptionsQuery(
            string keycloakId,
            long? patientId,
            string? searchTerm = null,
            DateOnly? date = null,
            int page = 1,
            int pageSize = 20)
        {
            KeycloakId = keycloakId;
            PatientId = patientId;
            SearchTerm = searchTerm;
            Date = date;
            Page = page;
            PageSize = pageSize;
        }
    }
}
