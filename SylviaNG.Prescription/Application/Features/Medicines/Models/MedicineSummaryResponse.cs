namespace SylviaNG.Prescription.Application.Features.Medicines.Models
{
    /// <summary>Epic F stub: catalog search result row, feeding the medicine autocomplete (US-022).</summary>
    public class MedicineSummaryResponse
    {
        public long MedicineId { get; set; }
        public string BrandName { get; set; } = string.Empty;
        public string? GenericName { get; set; }
        public string? Strength { get; set; }
        public string? DosageForm { get; set; }
        public string? Category { get; set; }
    }
}
