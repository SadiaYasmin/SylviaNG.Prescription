using MediatR;
using SylviaNG.Prescription.Application.Features.Medicines.Models;

namespace SylviaNG.Prescription.Application.Features.Medicines.Queries.SearchMedicines
{
    /// <summary>
    /// US-036/022: plain catalog search feeding both the prescription-authoring autocomplete
    /// (hot path — every keystroke, all three roles) and Staff's plain catalog browse
    /// (US-040 — Staff must never receive analytics data at all). Deliberately kept cheap and
    /// role-agnostic; the "Total Prescribed" analytics view for Admin/Doctor is a separate,
    /// heavier query (<see cref="GetMedicineCatalogQuery"/>) so the autocomplete never pays for
    /// an aggregation it doesn't need on every keystroke.
    /// </summary>
    public class SearchMedicinesQuery : IRequest<List<MedicineSummaryResponse>>
    {
        public string? SearchTerm { get; set; }

        public SearchMedicinesQuery(string? searchTerm)
        {
            SearchTerm = searchTerm;
        }
    }
}
