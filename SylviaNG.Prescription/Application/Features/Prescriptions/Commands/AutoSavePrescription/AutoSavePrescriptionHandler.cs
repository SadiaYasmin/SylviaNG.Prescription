using MediatR;
using SylviaNG.Prescription.Application.Common;
using SylviaNG.Prescription.Application.Common.Exceptions;
using SylviaNG.Prescription.Application.Features.Prescriptions.Models;
using SylviaNG.Prescription.Application.Interfaces.Repositories;
using SylviaNG.Prescription.Application.Mappings;
using SylviaNG.Prescription.Domain.Enums;
using SylviaNG.Prescription.SharedKernel.Generic;

namespace SylviaNG.Prescription.Application.Features.Prescriptions.Commands.AutoSavePrescription
{
    /// <summary>
    /// Persists the current section payload for an actively-authored prescription without
    /// changing its lifecycle: Status is left untouched (InProgress stays InProgress; a
    /// reopened Draft stays Draft), SavedAt is not stamped, and the linked Consultation is
    /// not moved. This is what lets an InProgress prescription be safe against data loss
    /// while still being kept out of the Draft Prescriptions list until the doctor leaves it
    /// or explicitly saves it.
    /// </summary>
    public class AutoSavePrescriptionHandler : IRequestHandler<AutoSavePrescriptionCommand, AutoSavePrescriptionResponse>
    {
        private readonly IUserRepository _userRepository;
        private readonly IStaffRepository _staffRepository;
        private readonly IDoctorRepository _doctorRepository;
        private readonly IPrescriptionRepository _prescriptionRepository;
        private readonly IUnitOfWork _unitOfWork;

        public AutoSavePrescriptionHandler(
            IUserRepository userRepository,
            IStaffRepository staffRepository,
            IDoctorRepository doctorRepository,
            IPrescriptionRepository prescriptionRepository,
            IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _staffRepository = staffRepository;
            _doctorRepository = doctorRepository;
            _prescriptionRepository = prescriptionRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<AutoSavePrescriptionResponse> Handle(AutoSavePrescriptionCommand command, CancellationToken cancellationToken)
        {
            var caller = await CallerContextResolver.ResolveCallerAsync(
                command.KeycloakId, _userRepository, _staffRepository, _doctorRepository);
            var doctorId = caller.DoctorId!.Value;

            var prescription = await _prescriptionRepository.GetByIdAsync(command.PrescriptionId)
                ?? throw new NotFoundException("Prescription", command.PrescriptionId);
            if (prescription.DoctorId != doctorId)
                throw new NotFoundException("Prescription", command.PrescriptionId);
            if (prescription.Status == PrescriptionStatusEnum.Finalized)
                throw new BadRequestException("A finalized prescription can no longer be edited.");

            var content = command.Request.Content ?? new PrescriptionContent();
            MedicineDuplicateGuard.EnsureNoDuplicates(content.Medicines);

            prescription.Language = command.Request.Language;
            prescription.SetContent(content);
            // Deliberately NOT touching Status / SavedAt / the Consultation — see class summary.
            _prescriptionRepository.Update(prescription);
            await _unitOfWork.SaveChangesAsync();

            return new AutoSavePrescriptionResponse
            {
                PrescriptionId = prescription.PrescriptionId,
                Status = prescription.Status,
                AutoSavedAt = DateTime.UtcNow
            };
        }
    }
}
