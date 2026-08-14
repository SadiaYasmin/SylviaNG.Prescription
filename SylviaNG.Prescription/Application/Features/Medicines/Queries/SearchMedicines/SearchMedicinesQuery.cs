using MediatR;
using SylviaNG.Prescription.Application.Features.Medicines.Models;

namespace SylviaNG.Prescription.Application.Features.Medicines.Queries.SearchMedicines
{
    /// <summary>US-036 (read side only — Epic F stub, no admin CRUD yet).</summary>
    public class SearchMedicinesQuery : IRequest<List<MedicineSummaryResponse>>
    {
        public string? SearchTerm { get; set; }

        public SearchMedicinesQuery(string? searchTerm)
        {
            SearchTerm = searchTerm;
        }
    }
}
