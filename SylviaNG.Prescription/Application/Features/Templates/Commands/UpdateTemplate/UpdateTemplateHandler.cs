using MediatR;
using SylviaNG.Prescription.Application.Common.Exceptions;
using SylviaNG.Prescription.Application.Features.Templates.Models;
using SylviaNG.Prescription.Application.Interfaces.Repositories;
using SylviaNG.Prescription.Application.Mappings;
using SylviaNG.Prescription.SharedKernel.Generic;

namespace SylviaNG.Prescription.Application.Features.Templates.Commands.UpdateTemplate
{
    public class UpdateTemplateHandler : IRequestHandler<UpdateTemplateCommand, TemplateDetailsResponse>
    {
        private readonly ITemplateRepository _templateRepository;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateTemplateHandler(ITemplateRepository templateRepository, IUnitOfWork unitOfWork)
        {
            _templateRepository = templateRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<TemplateDetailsResponse> Handle(UpdateTemplateCommand command, CancellationToken cancellationToken)
        {
            var template = await _templateRepository.GetByIdAsync(command.TemplateId)
                ?? throw new NotFoundException("PrescriptionTemplate", command.TemplateId);

            var request = command.Request;

            template.Name = request.Name;
            template.SetConfig(request.Config);

            _templateRepository.Update(template);
            await _unitOfWork.SaveChangesAsync();

            return template.ToDetailsResponse();
        }
    }
}
