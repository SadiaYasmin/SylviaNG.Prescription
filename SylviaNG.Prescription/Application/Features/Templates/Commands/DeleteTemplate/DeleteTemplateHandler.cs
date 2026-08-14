using MediatR;
using Microsoft.EntityFrameworkCore;
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

            // Doctor.PreferredTemplateId falls back to null automatically (DB-level SetNull,
            // see DoctorConfiguration) so that half of US-050's "falls back to default" needs
            // no check here. Prescription.TemplateId is Restrict, not SetNull, though — a
            // finalized prescription must always be able to re-render its original template
            // (US-064), so deleting a template that's ever been used is explicitly refused
            // with a clear message instead of surfacing the raw DB FK-violation as a 500.
            var isUsedByAnyPrescription = await _unitOfWork.Context.Prescriptions
                .AnyAsync(p => p.TemplateId == command.TemplateId, cancellationToken);
            if (isUsedByAnyPrescription)
                throw new BadRequestException("Cannot delete a template that has been used by an existing prescription.");

            _templateRepository.Delete(template);
            await _unitOfWork.SaveChangesAsync();

            return Unit.Value;
        }
    }
}
