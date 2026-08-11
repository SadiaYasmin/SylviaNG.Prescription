using MediatR;

namespace SylviaNG.Prescription.Application.Features.Templates.Commands.DeleteTemplate
{
    public class DeleteTemplateCommand : IRequest<Unit>
    {
        public long TemplateId { get; set; }

        public DeleteTemplateCommand(long templateId)
        {
            TemplateId = templateId;
        }
    }
}
