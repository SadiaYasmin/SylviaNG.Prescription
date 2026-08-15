using SylviaNG.Prescription.Domain.Enums;

namespace SylviaNG.Prescription.Domain.Entities;

/// <summary>
/// US-044: tracks, per doctor and per Quick Add section, whether the starter preset set has
/// already been seeded — a presence row means "don't seed this section for this doctor
/// again," independent of the section's current preset count. Deliberately NOT inferred from
/// "does the doctor have zero presets in this section," because a doctor who has since
/// deliberately deleted every preset in a section must stay empty, not get silently
/// re-seeded. Composite key (no surrogate id needed — one row per doctor+section, ever).
/// </summary>
public class DoctorQuickAddSeedState
{
    public long DoctorId { get; set; }
    public QuickAddSectionTypeEnum SectionType { get; set; }
    public DateTime SeededAt { get; set; }
}
