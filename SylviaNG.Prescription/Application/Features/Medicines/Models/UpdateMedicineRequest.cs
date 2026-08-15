namespace SylviaNG.Prescription.Application.Features.Medicines.Models
{
    /// <summary>Epic F (US-037): Admin edits a catalog entry.</summary>
    public class UpdateMedicineRequest
    {
        public string BrandName { get; set; } = string.Empty;
        public string? GenericName { get; set; }
        public string? Strength { get; set; }
        public string? Manufacturer { get; set; }
        public string? DosageForm { get; set; }
        public string? Category { get; set; }
    }
}
