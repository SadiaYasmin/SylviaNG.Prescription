using MediatR;
using SylviaNG.Prescription.Application.Features.Medicines.Models;

namespace SylviaNG.Prescription.Application.Features.Medicines.Queries.SearchMedicines
{
    /// <summary>
    /// US-036/022: plain catalog search feeding both the prescription-authoring autocomplete
    /// (hot path — every keystroke, all three roles) and Staff's plain catalog browse
    /// (US-040 — Staff must never receive analytics data at all). Deliberately kept cheap and
    /// role-agnostic; the "Total Prescribed" analytics view for Admin/Doctor is a separate,
    /// heavier query (<see cref="GetMedicineCatalog.GetMedicineCatalogQuery"/>) so the
    /// autocomplete never pays for an aggregation it doesn't need on every keystroke. Real
    /// Page/PageSize pagination (not just a capped Take) so every role can reach the same
    /// full catalog, not just Admin/Doctor via the separate catalog view.
    /// </summary>
    public class SearchMedicinesQuery : IRequest<MedicineSearchListResponse>
    {
        public string? SearchTerm { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }

        /// <summary>Admin/Doctor catalog tooling can opt into seeing non-DGDA-registered rows; everyone else stays on the safe default.</summary>
        public bool IncludeUnregistered { get; set; }

        public SearchMedicinesQuery(string? searchTerm, int page = 1, int pageSize = 10, bool includeUnregistered = false)
        {
            SearchTerm = searchTerm;
            Page = page < 1 ? 1 : page;
            PageSize = pageSize is < 1 or > 200 ? 10 : pageSize;
            IncludeUnregistered = includeUnregistered;
        }
    }
}
