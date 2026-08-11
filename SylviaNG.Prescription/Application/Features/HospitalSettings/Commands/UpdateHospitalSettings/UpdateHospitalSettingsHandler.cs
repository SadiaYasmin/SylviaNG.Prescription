using MediatR;
using Microsoft.EntityFrameworkCore;
using SylviaNG.Prescription.Application.Common.Exceptions;
using SylviaNG.Prescription.Application.Features.HospitalSettings.Models;
using SylviaNG.Prescription.Application.Interfaces.Repositories;
using SylviaNG.Prescription.Application.Mappings;
using SylviaNG.Prescription.SharedKernel.Generic;

namespace SylviaNG.Prescription.Application.Features.HospitalSettings.Commands.UpdateHospitalSettings
{
    /// <summary>
    /// US-045: single-record semantics. Updates whatever id the one existing row actually has —
    /// fetched via .FirstOrDefaultAsync(), never a hardcoded id=1.
    /// </summary>
    public class UpdateHospitalSettingsHandler : IRequestHandler<UpdateHospitalSettingsCommand, HospitalSettingsResponse>
    {
        private readonly IHospitalSettingsRepository _hospitalSettingsRepository;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateHospitalSettingsHandler(IHospitalSettingsRepository hospitalSettingsRepository, IUnitOfWork unitOfWork)
        {
            _hospitalSettingsRepository = hospitalSettingsRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<HospitalSettingsResponse> Handle(UpdateHospitalSettingsCommand command, CancellationToken cancellationToken)
        {
            var settings = await _hospitalSettingsRepository.Query(asNoTracking: false)
                .FirstOrDefaultAsync(cancellationToken)
                ?? throw new NotFoundException("HospitalSettings not found. The startup seeder should have created it.");

            var request = command.Request;

            settings.Name = request.Name;
            settings.LogoBase64 = request.LogoBase64;
            settings.Address = request.Address;
            settings.Phone = request.Phone;
            settings.EmergencyNumber = request.EmergencyNumber;
            settings.Email = request.Email;
            settings.Website = request.Website;
            settings.Slogan = request.Slogan;
            settings.SloganBn = request.SloganBn;
            settings.LicenseNumber = request.LicenseNumber;
            settings.SealBase64 = request.SealBase64;

            _hospitalSettingsRepository.Update(settings);
            await _unitOfWork.SaveChangesAsync();

            return settings.ToResponse();
        }
    }
}
