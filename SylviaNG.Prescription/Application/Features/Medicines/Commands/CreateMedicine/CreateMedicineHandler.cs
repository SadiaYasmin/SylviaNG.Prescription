using MediatR;
using Microsoft.EntityFrameworkCore;
using SylviaNG.Prescription.Application.Common.Exceptions;
using SylviaNG.Prescription.Application.Features.Medicines.Models;
using SylviaNG.Prescription.Application.Features.Prescriptions;
using SylviaNG.Prescription.Application.Interfaces.Repositories;
using SylviaNG.Prescription.Application.Mappings;
using SylviaNG.Prescription.Domain.Entities;
using SylviaNG.Prescription.SharedKernel.Generic;

namespace SylviaNG.Prescription.Application.Features.Medicines.Commands.CreateMedicine
{
    public class CreateMedicineHandler : IRequestHandler<CreateMedicineCommand, MedicineCatalogResponse>
    {
        private readonly IMedicineRepository _medicineRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreateMedicineHandler(IMedicineRepository medicineRepository, IUnitOfWork unitOfWork)
        {
            _medicineRepository = medicineRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<MedicineCatalogResponse> Handle(CreateMedicineCommand command, CancellationToken cancellationToken)
        {
            var request = command.Request;
            var key = MedicineDuplicateGuard.NormalizeKey(request.BrandName, request.Strength);

            var existing = await _medicineRepository.Query().ToListAsync(cancellationToken);
            if (existing.Any(m => MedicineDuplicateGuard.NormalizeKey(m.BrandName, m.Strength) == key))
            {
                throw new BadRequestException(
                    $"A medicine named \"{request.BrandName}\" ({request.Strength}) already exists in the catalog.");
            }

            var medicine = new Medicine
            {
                BrandName = request.BrandName,
                GenericName = request.GenericName,
                Strength = request.Strength,
                Manufacturer = request.Manufacturer,
                DosageForm = request.DosageForm,
                Route = request.Route,
                Category = request.Category,
                UnitPrice = request.UnitPrice,
                DgdaRegistered = request.DgdaRegistered,
                Active = true
            };

            await _medicineRepository.AddAsync(medicine);
            await _unitOfWork.SaveChangesAsync();

            return medicine.ToCatalogResponse(0);
        }
    }
}
