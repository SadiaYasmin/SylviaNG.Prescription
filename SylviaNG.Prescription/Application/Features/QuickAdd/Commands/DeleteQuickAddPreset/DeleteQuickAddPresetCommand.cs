using MediatR;

namespace SylviaNG.Prescription.Application.Features.QuickAdd.Commands.DeleteQuickAddPreset
{
    public class DeleteQuickAddPresetCommand : IRequest<Unit>
    {
        public string KeycloakId { get; set; }
        public long QuickAddPresetId { get; set; }

        public DeleteQuickAddPresetCommand(string keycloakId, long quickAddPresetId)
        {
            KeycloakId = keycloakId;
            QuickAddPresetId = quickAddPresetId;
        }
    }
}
