using MediatR;
using SylviaNG.Prescription.Application.Features.QuickAdd.Models;

namespace SylviaNG.Prescription.Application.Features.QuickAdd.Commands.AddQuickAddPreset
{
    public class AddQuickAddPresetCommand : IRequest<QuickAddPresetResponse>
    {
        public string KeycloakId { get; set; }
        public AddQuickAddPresetRequest Request { get; set; }

        public AddQuickAddPresetCommand(string keycloakId, AddQuickAddPresetRequest request)
        {
            KeycloakId = keycloakId;
            Request = request;
        }
    }
}
