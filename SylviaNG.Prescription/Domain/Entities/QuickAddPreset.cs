using SylviaNG.Prescription.Domain.Enums;
using SylviaNG.Prescription.SharedKernel.Audit;

namespace SylviaNG.Prescription.Domain.Entities;

/// <summary>
/// Minimal Epic G stub: one doctor-owned saved preset, insertable with one click into the
/// matching Prescription authoring section (US-025). A single table covers all five kinds
/// (<see cref="SectionType"/>) rather than five near-identical tables — <see cref="Label"/>
/// is the display text shown in the picker, <see cref="PayloadJson"/> is the shape that
/// section's list items use (e.g. the full medicine line for Medicine, a plain string for
/// the others), same serialized-JSON-column pattern as
/// <see cref="PrescriptionTemplate.ConfigJson"/>. List/add/delete only — no edit, no
/// auto-translate, no starter-seed; those are full Epic G's job.
/// </summary>
public class QuickAddPreset : Audit
{
    public long QuickAddPresetId { get; set; }
    public long DoctorId { get; set; }
    public QuickAddSectionTypeEnum SectionType { get; set; }
    public string Label { get; set; } = string.Empty;
    public string PayloadJson { get; set; } = string.Empty;
}
