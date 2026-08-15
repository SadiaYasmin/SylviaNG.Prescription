namespace SylviaNG.Prescription.Application.Features.Medicines.Models
{
    /// <summary>
    /// Epic F (US-038/039): the same catalog row as <see cref="MedicineSummaryResponse"/> plus
    /// a role-scoped "Total Prescribed" count — Admin sees it hospital-wide, Doctor sees it
    /// scoped to their own finalized prescriptions. A standalone class, not a subclass of
    /// <see cref="MedicineSummaryResponse"/>, so a Staff-authenticated request can never
    /// receive this shape even by accident (US-040 requires the field be structurally absent,
    /// not just unpopulated).
    /// </summary>
    public class MedicineCatalogResponse
    {
        public long MedicineId { get; set; }
        public string BrandName { get; set; } = string.Empty;
        public string? GenericName { get; set; }
        public string? Strength { get; set; }
        public string? Manufacturer { get; set; }
        public string? DosageForm { get; set; }
        public string? Category { get; set; }
        public bool Active { get; set; }
        public int TotalPrescribed { get; set; }
    }
}
