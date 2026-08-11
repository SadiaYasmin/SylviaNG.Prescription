namespace SylviaNG.Prescription.Domain.Enums;

/// <summary>
/// The base visual layout family a <see cref="Entities.PrescriptionTemplate"/> is built from
/// (Epic H / US-046). Government templates are structurally monochrome at render time — the
/// backend stores config for them like any other type but does not enforce that rendering rule.
/// </summary>
public enum TemplateTypeEnum
{
    Classic,
    Corporate,
    Government
}
