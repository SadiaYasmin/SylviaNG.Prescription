using MediatR;
using Microsoft.EntityFrameworkCore;
using SylviaNG.Prescription.Application.Common.Exceptions;
using SylviaNG.Prescription.Application.Features.Medicines.Models;
using SylviaNG.Prescription.Application.Features.Prescriptions;
using SylviaNG.Prescription.Application.Interfaces.Repositories;
using SylviaNG.Prescription.Application.Mappings;
using SylviaNG.Prescription.SharedKernel.Generic;

namespace SylviaNG.Prescription.Application.Features.Medicines.Commands.UpdateMedicine
{
    public class UpdateMedicineHandler : IRequestHandler<UpdateMedicineCommand, MedicineCatalogResponse>
    {
        private readonly IMedicineRepository _medicineRepository;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateMedicineHandler(IMedicineRepository medicineRepository, IUnitOfWork unitOfWork)
        {
            _medicineRepository = medicineRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<MedicineCatalogResponse> Handle(UpdateMedicineCommand command, CancellationToken cancellationToken)
        {
            var medicine = await _medicineRepository.GetByIdAsync(command.MedicineId)
                ?? throw new NotFoundException("Medicine", command.MedicineId);

            var request = command.Request;
            var key = MedicineDuplicateGuard.NormalizeKey(request.BrandName, request.Strength);

            var others = await _medicineRepository.Query()
                .Where(m => m.MedicineId != command.MedicineId)
                .ToListAsync(cancellationToken);
            if (others.Any(m => MedicineDuplicateGuard.NormalizeKey(m.BrandName, m.Strength) == key))
            {
                throw new BadRequestException(
                    $"A medicine named \"{request.BrandName}\" ({request.Strength}) already exists in the catalog.");
            }

            medicine.BrandName = request.BrandName;
            medicine.GenericName = request.GenericName;
            medicine.Strength = request.Strength;
            medicine.Manufacturer = request.Manufacturer;
            medicine.DosageForm = request.DosageForm;
            medicine.Route = request.Route;
            medicine.Category = request.Category;
            medicine.UnitPrice = request.UnitPrice;
            medicine.DgdaRegistered = request.DgdaRegistered;

            _medicineRepository.Update(medicine);
            await _unitOfWork.SaveChangesAsync();

            return medicine.ToCatalogResponse(0);
        }
    }
}
