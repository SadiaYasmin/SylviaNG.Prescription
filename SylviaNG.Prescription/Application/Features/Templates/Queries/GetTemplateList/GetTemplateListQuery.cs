using MediatR;
using SylviaNG.Prescription.Application.Features.Templates.Models;

namespace SylviaNG.Prescription.Application.Features.Templates.Queries.GetTemplateList
{
    public class GetTemplateListQuery : IRequest<TemplateListResponse>
    {
    }
}
