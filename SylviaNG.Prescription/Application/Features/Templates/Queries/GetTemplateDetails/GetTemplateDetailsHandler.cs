using MediatR;
using SylviaNG.Prescription.Application.Common.Exceptions;
using SylviaNG.Prescription.Application.Features.Templates.Models;
using SylviaNG.Prescription.Application.Interfaces.Repositories;
using SylviaNG.Prescription.Application.Mappings;

namespace SylviaNG.Prescription.Application.Features.Templates.Queries.GetTemplateDetails
{
    public class GetTemplateDetailsHandler : IRequestHandler<GetTemplateDetailsQuery, TemplateDetailsResponse>
    {
        private readonly ITemplateRepository _templateRepository;

        public GetTemplateDetailsHandler(ITemplateRepository templateRepository)
        {
            _templateRepository = templateRepository;
        }

        public async Task<TemplateDetailsResponse> Handle(GetTemplateDetailsQuery query, CancellationToken cancellationToken)
        {
            var template = await _templateRepository.GetByIdAsync(query.TemplateId)
                ?? throw new NotFoundException("PrescriptionTemplate", query.TemplateId);

            return template.ToDetailsResponse();
        }
    }
}
