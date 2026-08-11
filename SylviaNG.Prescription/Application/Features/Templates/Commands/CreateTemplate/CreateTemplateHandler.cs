using MediatR;
using SylviaNG.Prescription.Application.Features.Templates.Models;
using SylviaNG.Prescription.Application.Interfaces.Repositories;
using SylviaNG.Prescription.Application.Mappings;
using SylviaNG.Prescription.Domain.Entities;
using SylviaNG.Prescription.SharedKernel.Generic;

namespace SylviaNG.Prescription.Application.Features.Templates.Commands.CreateTemplate
{
    public class CreateTemplateHandler : IRequestHandler<CreateTemplateCommand, TemplateDetailsResponse>
    {
        private readonly ITemplateRepository _templateRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreateTemplateHandler(ITemplateRepository templateRepository, IUnitOfWork unitOfWork)
        {
            _templateRepository = templateRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<TemplateDetailsResponse> Handle(CreateTemplateCommand command, CancellationToken cancellationToken)
        {
            var request = command.Request;

            var template = new PrescriptionTemplate
            {
                Name = request.Name,
                Type = request.Type,
                Language = request.Language,
                Enabled = true,
                IsSystemDefault = false
            };

            // Client does not supply config on create (US-046) — server seeds it from
            // TemplateDefaults for the chosen type+language.
            template.SetConfig(TemplateDefaults.BuildDefaultConfig(request.Type, request.Language));

            await _templateRepository.AddAsync(template);
            await _unitOfWork.SaveChangesAsync();

            return template.ToDetailsResponse();
        }
    }
}
