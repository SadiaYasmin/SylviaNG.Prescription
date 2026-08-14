using MediatR;
using SylviaNG.Prescription.Application.Common;
using SylviaNG.Prescription.Application.Common.Exceptions;
using SylviaNG.Prescription.Application.Features.Prescriptions.Models;
using SylviaNG.Prescription.Application.Interfaces.Repositories;
using SylviaNG.Prescription.Application.Mappings;
using SylviaNG.Prescription.Domain.Enums;
using SylviaNG.Prescription.SharedKernel.Generic;

namespace SylviaNG.Prescription.Application.Features.Prescriptions.Commands.FinalizePrescription
{
    /// <summary>
    /// Finalize (US-028). Validates the full US-026 checklist (listing every missing item
    /// together, not one at a time), then — in the same SaveChangesAsync call as the
    /// Prescription/Consultation status flip (the US-017 invariant) — overwrites the
    /// patient's Saved History (US-009) with whatever is currently in the History section.
    /// A finalized prescription is permanently read-only from then on (enforced by
    /// SaveDraftPrescriptionHandler's Status check, since there's no "un-finalize" path).
    /// </summary>
    public class FinalizePrescriptionHandler : IRequestHandler<FinalizePrescriptionCommand, PrescriptionDocumentResponse>
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

        public FinalizePrescriptionHandler(
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

        public async Task<PrescriptionDocumentResponse> Handle(FinalizePrescriptionCommand command, CancellationToken cancellationToken)
        {
            var caller = await CallerContextResolver.ResolveCallerAsync(
                command.KeycloakId, _userRepository, _staffRepository, _doctorRepository);
            var doctorId = caller.DoctorId!.Value;

            var prescription = await _prescriptionRepository.GetByIdAsync(command.PrescriptionId)
                ?? throw new NotFoundException("Prescription", command.PrescriptionId);
            if (prescription.DoctorId != doctorId)
                throw new NotFoundException("Prescription", command.PrescriptionId);
            if (prescription.Status == PrescriptionStatusEnum.Finalized)
                throw new BadRequestException("This prescription is already finalized.");

            var doctor = await _doctorRepository.GetByIdAsync(doctorId)
                ?? throw new NotFoundException("Doctor", doctorId);

            var content = command.Request.Content ?? new PrescriptionContent();
            MedicineDuplicateGuard.EnsureNoDuplicates(content.Medicines);

            var missing = new List<string>();
            if (content.Diagnoses.Count == 0)
                missing.Add("at least one diagnosis");
            if (content.Medicines.Count == 0)
                missing.Add("at least one medicine");
            if (string.IsNullOrWhiteSpace(doctor.SignatureBase64))
                missing.Add("your signature (set it in Prescription Preferences)");
            if (doctor.PreferredTemplateId is null)
                missing.Add("a preferred template (set it in Prescription Preferences)");

            if (missing.Count > 0)
                throw new BadRequestException("Cannot finalize — missing: " + string.Join("; ", missing) + ".");

            prescription.Language = command.Request.Language;
            prescription.SetContent(content);
            prescription.Status = PrescriptionStatusEnum.Finalized;
            prescription.FinalizedAt = DateTime.UtcNow;
            prescription.SavedAt = prescription.FinalizedAt;
            _prescriptionRepository.Update(prescription);

            var consultation = await _consultationRepository.GetByIdAsync(prescription.ConsultationId)
                ?? throw new NotFoundException("Consultation", prescription.ConsultationId);
            consultation.Status = ConsultationStatusEnum.Completed;
            _consultationRepository.Update(consultation);

            var patient = await _patientRepository.GetByIdAsync(prescription.PatientId)
                ?? throw new NotFoundException("Patient", prescription.PatientId);
            // US-009: verbatim replace, not merge/append — whatever's in History right now
            // becomes the patient's carried-forward Saved History for their next visit.
            patient.SavedHistory = content.History.Count > 0 ? string.Join("\n", content.History) : null;
            _patientRepository.Update(patient);

            await _unitOfWork.SaveChangesAsync();

            var template = await _templateRepository.GetByIdAsync(prescription.TemplateId)
                ?? throw new NotFoundException("PrescriptionTemplate", prescription.TemplateId);
            var hospitalSettings = (await _hospitalSettingsRepository.GetAllAsync()).FirstOrDefault()
                ?? new Domain.Entities.HospitalSettings();

            return prescription.ToDocumentResponse(patient, doctor, template, hospitalSettings);
        }
    }
}
