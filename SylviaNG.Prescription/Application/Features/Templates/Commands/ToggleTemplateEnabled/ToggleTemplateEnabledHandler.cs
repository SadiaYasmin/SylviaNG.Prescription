using MediatR;
using SylviaNG.Prescription.Application.Common.Exceptions;
using SylviaNG.Prescription.Application.Features.Templates.Models;
using SylviaNG.Prescription.Application.Interfaces.Repositories;
using SylviaNG.Prescription.Application.Mappings;
using SylviaNG.Prescription.SharedKernel.Generic;

namespace SylviaNG.Prescription.Application.Features.Templates.Commands.ToggleTemplateEnabled
{
    public class ToggleTemplateEnabledHandler : IRequestHandler<ToggleTemplateEnabledCommand, TemplateSummaryResponse>
    {
        private readonly ITemplateRepository _templateRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ToggleTemplateEnabledHandler(ITemplateRepository templateRepository, IUnitOfWork unitOfWork)
        {
            _templateRepository = templateRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<TemplateSummaryResponse> Handle(ToggleTemplateEnabledCommand command, CancellationToken cancellationToken)
        {
            var template = await _templateRepository.GetByIdAsync(command.TemplateId)
                ?? throw new NotFoundException("PrescriptionTemplate", command.TemplateId);

            var newEnabled = !template.Enabled;

            // Re-enabling is always fine; disabling the system default is not (US-050).
            if (template.IsSystemDefault && !newEnabled)
                throw new BadRequestException("Cannot disable the system default template.");

            template.Enabled = newEnabled;

            _templateRepository.Update(template);
            await _unitOfWork.SaveChangesAsync();

            return template.ToSummaryResponse();
        }
    }
}
