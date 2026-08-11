using MediatR;
using SylviaNG.Prescription.Application.Features.Templates.Models;

namespace SylviaNG.Prescription.Application.Features.Templates.Commands.CreateTemplate
{
    public class CreateTemplateCommand : IRequest<TemplateDetailsResponse>
    {
        public CreateTemplateRequest Request { get; set; }

        public CreateTemplateCommand(CreateTemplateRequest request)
        {
            Request = request;
        }
    }
}
