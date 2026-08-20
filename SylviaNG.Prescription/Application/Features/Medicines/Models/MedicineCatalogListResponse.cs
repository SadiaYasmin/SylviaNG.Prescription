namespace SylviaNG.Prescription.Application.Features.Medicines.Models
{
    /// <summary>Paged wrapper for the Medicine Catalog admin/doctor screen — see GetMedicineCatalogHandler for why this exists (20k+ row catalogs after a CSV import made the unpaged list freeze the browser).</summary>
    public class MedicineCatalogListResponse
    {
        public List<MedicineCatalogResponse> Medicines { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }
}
