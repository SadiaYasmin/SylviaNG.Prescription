using SylviaNG.Prescription.Domain.Enums;
using SylviaNG.Prescription.SharedKernel.Audit;

namespace SylviaNG.Prescription.Domain.Entities;

/// <summary>
/// A hospital-branded prescription layout (Epic H / US-045-052). <see cref="Name"/>,
/// <see cref="Type"/>, <see cref="Language"/>, and <see cref="Enabled"/> are real columns
/// (needed for filtering/listing); every header/footer/style/visibility/print/label detail
/// lives in <see cref="ConfigJson"/> as a single serialized <c>TemplateConfig</c> — see
/// Application/Features/Templates/Models/TemplateConfig.cs — rather than one DB column per
/// field, since this project must stay portable across SqlServer/Npgsql/Oracle and EF's
/// `.ToJson()` owned-entity mapping isn't reliably portable across all three.
/// </summary>
public class PrescriptionTemplate : Audit
{
    public long TemplateId { get; set; }
    public string Name { get; set; } = string.Empty;
    public TemplateTypeEnum Type { get; set; }
    public TemplateLanguageEnum Language { get; set; }
    public bool Enabled { get; set; }
    public bool IsSystemDefault { get; set; }
    public string ConfigJson { get; set; } = string.Empty;
}
