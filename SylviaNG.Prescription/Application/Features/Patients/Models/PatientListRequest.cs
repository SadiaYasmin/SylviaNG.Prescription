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
        /// <summary>Optional date range (e.g. from an Analytics KPI card carrying its selected date filter). Null on either end = no date filter, unchanged from before this existed. With <see cref="ReturningOnly"/> unset (the plain/New Patients case), filters by registration date directly.</summary>
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }

        /// <summary>
        /// True (the "New Patients" KPI card drill-down): patients registered inside [<see cref="From"/>,<see cref="To"/>) —
        /// mirrors <c>GetPatientAnalyticsHandler</c>'s "New Patients" definition. No dedicated branch needed server-side:
        /// this is identical to the plain registration-date filter, so it's accepted here purely for the caller's clarity.
        /// </summary>
        public bool NewOnly { get; set; }

        /// <summary>
        /// True: show only patients registered BEFORE <see cref="From"/> who have at least one Completed consultation
        /// inside [<see cref="From"/>,<see cref="To"/>) — mirrors <c>GetPatientAnalyticsHandler</c>'s "Returning Patients"
        /// definition exactly, for the Patient Analytics tab's "Returning Patients" KPI card drill-down.
        /// </summary>
        public bool ReturningOnly { get; set; }

        /// <summary>
        /// Doctor-only. True: show only patients the CALLING doctor had a Completed consultation with inside
        /// [<see cref="From"/>,<see cref="To"/>) — or ever, if From/To are both null ("All Time"). Backs the
        /// Doctor Dashboard's "Patients Consulted" KPI card drill-down; ignored for a non-Doctor caller.
        /// </summary>
        public bool CompletedWithMeOnly { get; set; }
    }
}
