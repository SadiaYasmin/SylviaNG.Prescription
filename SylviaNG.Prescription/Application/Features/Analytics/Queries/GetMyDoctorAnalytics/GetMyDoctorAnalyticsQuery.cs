using MediatR;
using SylviaNG.Prescription.Application.Features.Analytics.Models;

namespace SylviaNG.Prescription.Application.Features.Analytics.Queries.GetMyDoctorAnalytics
{
    public class GetMyDoctorAnalyticsQuery : IRequest<MyDoctorAnalyticsResponse>
    {
        public string KeycloakId { get; set; }

        /// <summary>Optional period for the Doctor Dashboard's "Patients Consulted"/"Finalized Prescriptions" cards (Today/This Week/This Month/All Time, resolved client-side into concrete bounds). Null on either end = all-time, unaffected. Every other field on this response (OwnPatientCount, DraftPrescriptionCount, AssignedStaffCount, TopMedicines) is unaffected regardless.</summary>
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }

        public GetMyDoctorAnalyticsQuery(string keycloakId, DateTime? from = null, DateTime? to = null)
        {
            KeycloakId = keycloakId;
            From = from;
            To = to;
        }
    }
}
