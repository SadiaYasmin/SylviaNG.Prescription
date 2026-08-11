using MediatR;
using SylviaNG.Prescription.Application.Features.Templates.Models;

namespace SylviaNG.Prescription.Application.Features.Templates.Commands.DuplicateTemplate
{
    public class DuplicateTemplateCommand : IRequest<TemplateDetailsResponse>
    {
        public long TemplateId { get; set; }

        public DuplicateTemplateCommand(long templateId)
        {
            TemplateId = templateId;
        }
    }
}
