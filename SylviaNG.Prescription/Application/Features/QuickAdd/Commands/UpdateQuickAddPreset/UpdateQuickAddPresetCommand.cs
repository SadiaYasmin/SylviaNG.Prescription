using MediatR;
using SylviaNG.Prescription.Application.Features.QuickAdd.Models;

namespace SylviaNG.Prescription.Application.Features.QuickAdd.Commands.UpdateQuickAddPreset
{
    public class UpdateQuickAddPresetCommand : IRequest<QuickAddPresetResponse>
    {
        public string KeycloakId { get; set; }
        public long QuickAddPresetId { get; set; }
        public UpdateQuickAddPresetRequest Request { get; set; }

        public UpdateQuickAddPresetCommand(string keycloakId, long quickAddPresetId, UpdateQuickAddPresetRequest request)
        {
            KeycloakId = keycloakId;
            QuickAddPresetId = quickAddPresetId;
            Request = request;
        }
    }
}
