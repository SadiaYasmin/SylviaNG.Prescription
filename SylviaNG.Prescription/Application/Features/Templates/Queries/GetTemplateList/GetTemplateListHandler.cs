using MediatR;
using Microsoft.EntityFrameworkCore;
using SylviaNG.Prescription.Application.Features.Templates.Models;
using SylviaNG.Prescription.Application.Interfaces.Repositories;
using SylviaNG.Prescription.Application.Mappings;

namespace SylviaNG.Prescription.Application.Features.Templates.Queries.GetTemplateList
{
    public class GetTemplateListHandler : IRequestHandler<GetTemplateListQuery, TemplateListResponse>
    {
        private readonly ITemplateRepository _templateRepository;

        public GetTemplateListHandler(ITemplateRepository templateRepository)
        {
            _templateRepository = templateRepository;
        }

        public async Task<TemplateListResponse> Handle(GetTemplateListQuery query, CancellationToken cancellationToken)
        {
            var templates = _templateRepository.Query();
            if (query.EnabledOnly)
                templates = templates.Where(t => t.Enabled);

            var results = await templates
                .OrderBy(t => t.Name)
                .ToListAsync(cancellationToken);

            return new TemplateListResponse
            {
                Templates = results.Select(t => t.ToSummaryResponse()).ToList()
            };
        }
    }
}
