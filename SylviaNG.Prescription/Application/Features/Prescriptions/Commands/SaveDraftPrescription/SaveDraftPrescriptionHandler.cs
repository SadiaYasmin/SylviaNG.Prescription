using MediatR;
using SylviaNG.Prescription.Application.Common;
using SylviaNG.Prescription.Application.Common.Exceptions;
using SylviaNG.Prescription.Application.Features.Prescriptions.Models;
using SylviaNG.Prescription.Application.Interfaces.Repositories;
using SylviaNG.Prescription.Application.Mappings;
using SylviaNG.Prescription.Domain.Enums;
using SylviaNG.Prescription.SharedKernel.Generic;

namespace SylviaNG.Prescription.Application.Features.Prescriptions.Commands.SaveDraftPrescription
{
    /// <summary>
    /// Save as Draft (US-027): persists the full section payload, stamps SavedAt, and — the
    /// transactional half of the US-017 status invariant — sets the linked Consultation to
    /// Draft (out of the live queue) in the SAME SaveChangesAsync call, never a second save.
    /// </summary>
    public class SaveDraftPrescriptionHandler : IRequestHandler<SaveDraftPrescriptionCommand, PrescriptionDocumentResponse>
    {
        private readonly IUserRepository _userRepository;
        private readonly IStaffRepository _staffRepository;
        private readonly IDoctorRepository _doctorRepository;
        private readonly IPatientRepository _patientRepository;
        private readonly IConsultationRepository _consultationRepository;
        private readonly IPrescriptionRepository _prescriptionRepository;
        private readonly ITemplateRepository _templateRepository;
        private readonly IHospitalSettingsRepository _hospitalSettingsRepository;
        private readonly IUnitOfWork _unitOfWork;

        public SaveDraftPrescriptionHandler(
            IUserRepository userRepository,
            IStaffRepository staffRepository,
            IDoctorRepository doctorRepository,
            IPatientRepository patientRepository,
            IConsultationRepository consultationRepository,
            IPrescriptionRepository prescriptionRepository,
            ITemplateRepository templateRepository,
            IHospitalSettingsRepository hospitalSettingsRepository,
            IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _staffRepository = staffRepository;
            _doctorRepository = doctorRepository;
            _patientRepository = patientRepository;
            _consultationRepository = consultationRepository;
            _prescriptionRepository = prescriptionRepository;
            _templateRepository = templateRepository;
            _hospitalSettingsRepository = hospitalSettingsRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<PrescriptionDocumentResponse> Handle(SaveDraftPrescriptionCommand command, CancellationToken cancellationToken)
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
            // Promote InProgress -> Draft: this endpoint is hit both by an explicit "Save as
            // Draft" and by the auto-park when the doctor leaves an unfinished prescription.
            // (A reopened Draft is already Draft; a Finalized one was rejected above.)
            prescription.Status = PrescriptionStatusEnum.Draft;
            prescription.SavedAt = DateTime.UtcNow;
            _prescriptionRepository.Update(prescription);

            var consultation = await _consultationRepository.GetByIdAsync(prescription.ConsultationId)
                ?? throw new NotFoundException("Consultation", prescription.ConsultationId);
            consultation.Status = ConsultationStatusEnum.Draft;
            _consultationRepository.Update(consultation);

            await _unitOfWork.SaveChangesAsync();

            var patient = await _patientRepository.GetByIdAsync(prescription.PatientId)
                ?? throw new NotFoundException("Patient", prescription.PatientId);
            var doctor = await _doctorRepository.GetByIdAsync(prescription.DoctorId)
                ?? throw new NotFoundException("Doctor", prescription.DoctorId);
            var template = await _templateRepository.GetByIdAsync(prescription.TemplateId)
                ?? throw new NotFoundException("PrescriptionTemplate", prescription.TemplateId);
            var hospitalSettings = (await _hospitalSettingsRepository.GetAllAsync()).FirstOrDefault()
                ?? new Domain.Entities.HospitalSettings();

            return prescription.ToDocumentResponse(patient, doctor, template, hospitalSettings);
        }
    }
}
