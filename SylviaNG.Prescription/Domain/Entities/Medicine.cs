using SylviaNG.Prescription.SharedKernel.Audit;

namespace SylviaNG.Prescription.Domain.Entities;

/// <summary>
/// Minimal Epic F stub: a searchable reference list backing Prescription authoring's
/// medicine autocomplete (US-022). No admin CRUD yet — rows come from
/// <c>MedicineCatalogSeeder</c> only. <see cref="Active"/> exists now so the full Epic F
/// (deactivate without breaking historical prescriptions) can land later without a schema
/// change, even though nothing sets it false yet.
/// </summary>
public class Medicine : Audit
{
    public long MedicineId { get; set; }
    public string BrandName { get; set; } = string.Empty;
    public string? GenericName { get; set; }
    public string? Strength { get; set; }
    public string? Manufacturer { get; set; }
    public string? DosageForm { get; set; }
    public string? Route { get; set; }
    public string? Category { get; set; }
    public decimal? UnitPrice { get; set; }
    public bool DgdaRegistered { get; set; }
    public bool Active { get; set; } = true;
}
