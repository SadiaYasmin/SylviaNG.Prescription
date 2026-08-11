namespace SylviaNG.Prescription.Domain.Enums;

/// <summary>
/// The label language a <see cref="Entities.PrescriptionTemplate"/> is authored in (Epic H /
/// US-046, US-049). Drives which default label set <c>TemplateDefaults</c> seeds on create.
/// </summary>
public enum TemplateLanguageEnum
{
    En,
    Bn
}
