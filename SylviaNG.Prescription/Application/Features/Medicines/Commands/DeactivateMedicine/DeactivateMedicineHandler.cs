using MediatR;
using SylviaNG.Prescription.Application.Common.Exceptions;
using SylviaNG.Prescription.Application.Interfaces.Repositories;
using SylviaNG.Prescription.SharedKernel.Generic;

namespace SylviaNG.Prescription.Application.Features.Medicines.Commands.DeactivateMedicine
{
    public class DeactivateMedicineHandler : IRequestHandler<DeactivateMedicineCommand, Unit>
    {
        private readonly IMedicineRepository _medicineRepository;
        private readonly IUnitOfWork _unitOfWork;

        public DeactivateMedicineHandler(IMedicineRepository medicineRepository, IUnitOfWork unitOfWork)
        {
            _medicineRepository = medicineRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Unit> Handle(DeactivateMedicineCommand command, CancellationToken cancellationToken)
        {
            var medicine = await _medicineRepository.GetByIdAsync(command.MedicineId)
                ?? throw new NotFoundException("Medicine", command.MedicineId);

            // Never a hard delete — historical prescriptions store a text snapshot of each
            // line (PrescriptionRecord.MedicinesJson), unaffected either way, but a deactivated
            // catalog row simply drops out of SearchMedicinesHandler's Active-only filter so
            // future prescribing (autocomplete + catalog CRUD list) stops offering it.
            medicine.Active = false;

            _medicineRepository.Update(medicine);
            await _unitOfWork.SaveChangesAsync();

            return Unit.Value;
        }
    }
}
