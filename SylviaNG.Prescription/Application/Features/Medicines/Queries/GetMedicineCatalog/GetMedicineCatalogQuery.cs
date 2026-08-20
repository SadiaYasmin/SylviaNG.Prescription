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

        public GetMedicineCatalogQuery(string? searchTerm, string keycloakId, int page = 1, int pageSize = 25)
        {
            SearchTerm = searchTerm;
            KeycloakId = keycloakId;
            Page = page < 1 ? 1 : page;
            PageSize = pageSize is < 1 or > 200 ? 25 : pageSize;
        }
    }
}
