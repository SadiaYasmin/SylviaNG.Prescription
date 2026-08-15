namespace SylviaNG.Prescription.Application.Features.Medicines.Models
{
    /// <summary>Epic F (US-037): Admin creates a catalog entry.</summary>
    public class CreateMedicineRequest
    {
        public string BrandName { get; set; } = string.Empty;
        public string? GenericName { get; set; }
        public string? Strength { get; set; }
        public string? Manufacturer { get; set; }
        public string? DosageForm { get; set; }
        public string? Category { get; set; }
    }
}
