using MediatR;
using SylviaNG.Prescription.Application.Features.Templates.Models;

namespace SylviaNG.Prescription.Application.Features.Templates.Commands.UpdateTemplate
{
    public class UpdateTemplateCommand : IRequest<TemplateDetailsResponse>
    {
        public long TemplateId { get; set; }
        public UpdateTemplateRequest Request { get; set; }

        public UpdateTemplateCommand(long templateId, UpdateTemplateRequest request)
        {
            TemplateId = templateId;
            Request = request;
        }
    }
}
