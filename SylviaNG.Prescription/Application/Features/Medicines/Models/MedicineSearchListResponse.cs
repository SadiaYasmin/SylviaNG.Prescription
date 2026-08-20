namespace SylviaNG.Prescription.Application.Features.Medicines.Models
{
    /// <summary>
    /// Paged wrapper for the plain catalog search/browse (Admin/Doctor/Staff — no analytics
    /// fields). Previously this endpoint returned a bare capped list with no real total, so
    /// Staff's "(50 total)" browse header was lying about the catalog's actual size and had
    /// no way to page past the first 50 rows. Same shape as <see cref="MedicineCatalogListResponse"/>
    /// so both list screens paginate identically.
    /// </summary>
    public class MedicineSearchListResponse
    {
        public List<MedicineSummaryResponse> Medicines { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }
}
