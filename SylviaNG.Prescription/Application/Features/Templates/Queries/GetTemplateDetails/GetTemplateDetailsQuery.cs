using MediatR;
using SylviaNG.Prescription.Application.Features.Templates.Models;

namespace SylviaNG.Prescription.Application.Features.Templates.Queries.GetTemplateDetails
{
    public class GetTemplateDetailsQuery : IRequest<TemplateDetailsResponse>
    {
        public long TemplateId { get; set; }

        public GetTemplateDetailsQuery(long templateId)
        {
            TemplateId = templateId;
        }
    }
}
