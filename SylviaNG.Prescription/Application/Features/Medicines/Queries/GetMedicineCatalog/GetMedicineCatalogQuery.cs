using MediatR;
using SylviaNG.Prescription.Application.Features.Medicines.Models;

namespace SylviaNG.Prescription.Application.Features.Medicines.Queries.GetMedicineCatalog
{
    /// <summary>
    /// US-037/038/039: the Medicine Catalog management screen's list — every medicine
    /// (active and deactivated, so Admin can re-activate one) plus a role-scoped "Total
    /// Prescribed" count (Admin: hospital-wide; Doctor: their own finalized prescriptions
    /// only), sorted by that total descending. Admin/Doctor only — Staff never calls this
    /// query at all (US-040), they use the plain <see cref="SearchMedicines.SearchMedicinesQuery"/>.
    /// </summary>
    public class GetMedicineCatalogQuery : IRequest<MedicineCatalogListResponse>
    {
        public string? SearchTerm { get; set; }
        public string KeycloakId { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }

        /// <summary>Optional date-range for the "Total Prescribed"/"Prescribed" count. Null on either end = lifetime (Admin's default behavior). Doctor role always applies this when supplied by the Doctor Medicine List's own filter.</summary>
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }

        /// <summary>Admin-only drill-down override: scope the count to one specific doctor (e.g. from the Doctor Details "Total Medicines Prescribed" card). Ignored for a Doctor caller — they're always scoped to their own DoctorId regardless of this value.</summary>
        public long? DoctorId { get; set; }

        public GetMedicineCatalogQuery(string? searchTerm, string keycloakId, int page = 1, int pageSize = 25, DateTime? from = null, DateTime? to = null, long? doctorId = null)
        {
            SearchTerm = searchTerm;
            KeycloakId = keycloakId;
            Page = page < 1 ? 1 : page;
            PageSize = pageSize is < 1 or > 200 ? 25 : pageSize;
            From = from;
            To = to;
            DoctorId = doctorId;
        }
    }
}
