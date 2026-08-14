using SylviaNG.Prescription.Application.Features.QuickAdd.Models;
using SylviaNG.Prescription.Domain.Entities;

namespace SylviaNG.Prescription.Application.Mappings
{
    public static class QuickAddMappings
    {
        public static QuickAddPresetResponse ToResponse(this QuickAddPreset preset) => new()
        {
            QuickAddPresetId = preset.QuickAddPresetId,
            SectionType = preset.SectionType,
            Label = preset.Label,
            PayloadJson = preset.PayloadJson
        };
    }
}
