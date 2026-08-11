using MediatR;
using SylviaNG.Prescription.Application.Features.Templates.Models;

namespace SylviaNG.Prescription.Application.Features.Templates.Commands.ToggleTemplateEnabled
{
    public class ToggleTemplateEnabledCommand : IRequest<TemplateSummaryResponse>
    {
        public long TemplateId { get; set; }

        public ToggleTemplateEnabledCommand(long templateId)
        {
            TemplateId = templateId;
        }
    }
}
