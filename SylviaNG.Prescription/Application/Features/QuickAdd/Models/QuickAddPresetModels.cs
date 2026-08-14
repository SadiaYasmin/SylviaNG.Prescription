using SylviaNG.Prescription.Domain.Enums;

namespace SylviaNG.Prescription.Application.Features.QuickAdd.Models
{
    /// <summary>Epic G stub: add/list/delete only — no edit, no auto-translate, no starter-seed.</summary>
    public class QuickAddPresetResponse
    {
        public long QuickAddPresetId { get; set; }
        public QuickAddSectionTypeEnum SectionType { get; set; }
        public string Label { get; set; } = string.Empty;
        public string PayloadJson { get; set; } = string.Empty;
    }

    public class AddQuickAddPresetRequest
    {
        public QuickAddSectionTypeEnum SectionType { get; set; }
        public string Label { get; set; } = string.Empty;
        public string PayloadJson { get; set; } = string.Empty;
    }
}
