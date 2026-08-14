using MediatR;
using SylviaNG.Prescription.Application.Features.QuickAdd.Models;
using SylviaNG.Prescription.Domain.Enums;

namespace SylviaNG.Prescription.Application.Features.QuickAdd.Queries.GetQuickAddPresets
{
    public class GetQuickAddPresetsQuery : IRequest<List<QuickAddPresetResponse>>
    {
        public string KeycloakId { get; set; }
        public QuickAddSectionTypeEnum SectionType { get; set; }

        public GetQuickAddPresetsQuery(string keycloakId, QuickAddSectionTypeEnum sectionType)
        {
            KeycloakId = keycloakId;
            SectionType = sectionType;
        }
    }
}
