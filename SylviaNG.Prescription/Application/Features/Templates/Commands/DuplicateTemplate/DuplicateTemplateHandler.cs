using MediatR;
using SylviaNG.Prescription.Application.Common.Exceptions;
using SylviaNG.Prescription.Application.Features.Templates.Models;
using SylviaNG.Prescription.Application.Interfaces.Repositories;
using SylviaNG.Prescription.Application.Mappings;
using SylviaNG.Prescription.Domain.Entities;
using SylviaNG.Prescription.SharedKernel.Generic;

namespace SylviaNG.Prescription.Application.Features.Templates.Commands.DuplicateTemplate
{
    public class DuplicateTemplateHandler : IRequestHandler<DuplicateTemplateCommand, TemplateDetailsResponse>
    {
        private readonly ITemplateRepository _templateRepository;
        private readonly IUnitOfWork _unitOfWork;

        public DuplicateTemplateHandler(ITemplateRepository templateRepository, IUnitOfWork unitOfWork)
        {
            _templateRepository = templateRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<TemplateDetailsResponse> Handle(DuplicateTemplateCommand command, CancellationToken cancellationToken)
        {
            var source = await _templateRepository.GetByIdAsync(command.TemplateId)
                ?? throw new NotFoundException("PrescriptionTemplate", command.TemplateId);

            var clone = new PrescriptionTemplate
            {
                Name = $"{source.Name} (Copy)",
                Type = source.Type,
                Language = source.Language,
                Enabled = source.Enabled,
                // Duplicates are never the system default, regardless of the source (US-050).
                IsSystemDefault = false,
                ConfigJson = source.ConfigJson
            };

            await _templateRepository.AddAsync(clone);
            await _unitOfWork.SaveChangesAsync();

            return clone.ToDetailsResponse();
        }
    }
}
