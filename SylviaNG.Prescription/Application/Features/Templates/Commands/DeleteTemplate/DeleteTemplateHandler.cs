using MediatR;
using SylviaNG.Prescription.Application.Common.Exceptions;
using SylviaNG.Prescription.Application.Interfaces.Repositories;
using SylviaNG.Prescription.SharedKernel.Generic;

namespace SylviaNG.Prescription.Application.Features.Templates.Commands.DeleteTemplate
{
    public class DeleteTemplateHandler : IRequestHandler<DeleteTemplateCommand, Unit>
    {
        private readonly ITemplateRepository _templateRepository;
        private readonly IUnitOfWork _unitOfWork;

        public DeleteTemplateHandler(ITemplateRepository templateRepository, IUnitOfWork unitOfWork)
        {
            _templateRepository = templateRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Unit> Handle(DeleteTemplateCommand command, CancellationToken cancellationToken)
        {
            var template = await _templateRepository.GetByIdAsync(command.TemplateId)
                ?? throw new NotFoundException("PrescriptionTemplate", command.TemplateId);

            if (template.IsSystemDefault)
                throw new BadRequestException("Cannot delete the system default template.");

            // No FK references exist from any other entity yet, so a hard delete is safe.
            _templateRepository.Delete(template);
            await _unitOfWork.SaveChangesAsync();

            return Unit.Value;
        }
    }
}
