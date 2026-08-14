using MediatR;
using Microsoft.EntityFrameworkCore;
using SylviaNG.Prescription.Application.Common;
using SylviaNG.Prescription.Application.Common.Exceptions;
using SylviaNG.Prescription.Application.Common.Services;
using SylviaNG.Prescription.Application.Features.Prescriptions.Models;
using SylviaNG.Prescription.Application.Interfaces.Repositories;
using SylviaNG.Prescription.Application.Mappings;
using SylviaNG.Prescription.Domain.Entities;
using SylviaNG.Prescription.Domain.Enums;
using SylviaNG.Prescription.SharedKernel.Generic;
using SylviaNG.Prescription.SharedKernel.Utils;

namespace SylviaNG.Prescription.Application.Features.Prescriptions.Commands.StartOrResumePrescription
{
    /// <summary>
    /// The single authoring entry point (US-018/019/020) — see StartOrResumePrescriptionRequest's
    /// doc comment for the three disambiguated modes. Only Doctor can hit this (enforced by
    /// [Authorize] on the controller), so the caller is always resolved to a DoctorId.
    /// </summary>
    public class StartOrResumePrescriptionHandler : IRequestHandler<StartOrResumePrescriptionCommand, StartOrResumePrescriptionResponse>
    {
        private readonly IUserRepository _userRepository;
        private readonly IStaffRepository _staffRepository;
        private readonly IDoctorRepository _doctorRepository;
        private readonly IPatientRepository _patientRepository;
        private readonly IConsultationRepository _consultationRepository;
        private readonly IPrescriptionRepository _prescriptionRepository;
        private readonly ITemplateRepository _templateRepository;
        private readonly IHospitalSettingsRepository _hospitalSettingsRepository;
        private readonly ISequenceGenerator _sequenceGenerator;
        private readonly IUnitOfWork _unitOfWork;

        public StartOrResumePrescriptionHandler(
            IUserRepository userRepository,
            IStaffRepository staffRepository,
            IDoctorRepository doctorRepository,
            IPatientRepository patientRepository,
            IConsultationRepository consultationRepository,
            IPrescriptionRepository prescriptionRepository,
            ITemplateRepository templateRepository,
            IHospitalSettingsRepository hospitalSettingsRepository,
            ISequenceGenerator sequenceGenerator,
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
            _sequenceGenerator = sequenceGenerator;
            _unitOfWork = unitOfWork;
        }

        public async Task<StartOrResumePrescriptionResponse> Handle(StartOrResumePrescriptionCommand command, CancellationToken cancellationToken)
        {
            var caller = await CallerContextResolver.ResolveCallerAsync(
                command.KeycloakId, _userRepository, _staffRepository, _doctorRepository);
            var doctorId = caller.DoctorId!.Value;
            var request = command.Request;

            Domain.Entities.Consultation consultation;
            PrescriptionRecord? prescription;

            if (request.PrescriptionId.HasValue)
            {
                prescription = await _prescriptionRepository.GetByIdAsync(request.PrescriptionId.Value)
                    ?? throw new NotFoundException("Prescription", request.PrescriptionId.Value);
                if (prescription.DoctorId != doctorId)
                    throw new NotFoundException("Prescription", request.PrescriptionId.Value);

                consultation = await _consultationRepository.GetByIdAsync(prescription.ConsultationId)
                    ?? throw new NotFoundException("Consultation", prescription.ConsultationId);

                // Reopening a draft never creates a second consultation — just bring it back
                // into an active authoring state if it had been parked as Draft.
                if (consultation.Status == ConsultationStatusEnum.Draft)
                {
                    consultation.Status = ConsultationStatusEnum.InConsultation;
                    _consultationRepository.Update(consultation);
                    await _unitOfWork.SaveChangesAsync();
                }
            }
            else if (request.ConsultationId.HasValue)
            {
                consultation = await _consultationRepository.GetByIdAsync(request.ConsultationId.Value)
                    ?? throw new NotFoundException("Consultation", request.ConsultationId.Value);
                if (consultation.DoctorId != doctorId)
                    throw new NotFoundException("Consultation", request.ConsultationId.Value);

                if (consultation.Status == ConsultationStatusEnum.Completed)
                    throw new BadRequestException("This consultation is already completed.");

                if (consultation.Status != ConsultationStatusEnum.InConsultation)
                {
                    consultation.Status = ConsultationStatusEnum.InConsultation;
                    _consultationRepository.Update(consultation);
                    await _unitOfWork.SaveChangesAsync();
                }

                prescription = await _prescriptionRepository.Query()
                    .FirstOrDefaultAsync(p => p.ConsultationId == consultation.ConsultationId, cancellationToken);
            }
            else if (request.PatientId.HasValue)
            {
                var patientId = request.PatientId.Value;
                var today = DateTimeUtility.TodayLocal();

                if (!request.Force)
                {
                    // US-011: an existing active (Waiting/InConsultation) consultation for this
                    // patient today, regardless of doctor — mirrors CreateConsultationHandler's rule.
                    var existingActive = await _consultationRepository.Query()
                        .Where(c => c.PatientId == patientId && c.VisitDate == today
                            && (c.Status == ConsultationStatusEnum.Waiting || c.Status == ConsultationStatusEnum.InConsultation))
                        .FirstOrDefaultAsync(cancellationToken);
                    if (existingActive is not null)
                    {
                        return new StartOrResumePrescriptionResponse
                        {
                            DuplicateActiveFound = true,
                            ExistingActiveConsultation = existingActive.ToSummaryResponse()
                        };
                    }

                    // US-012: an unfinished draft for this patient with THIS doctor, any day.
                    var draftConsultationIds = await _consultationRepository.Query()
                        .Where(c => c.PatientId == patientId && c.DoctorId == doctorId && c.Status == ConsultationStatusEnum.Draft)
                        .Select(c => c.ConsultationId)
                        .ToListAsync(cancellationToken);
                    if (draftConsultationIds.Count > 0)
                    {
                        var drafts = await _prescriptionRepository.Query()
                            .Where(p => draftConsultationIds.Contains(p.ConsultationId))
                            .ToListAsync(cancellationToken);
                        var draftPatient = await _patientRepository.GetByIdAsync(patientId);
                        var draftDoctor = await _doctorRepository.GetByIdAsync(doctorId);
                        return new StartOrResumePrescriptionResponse
                        {
                            UnfinishedDraftFound = true,
                            UnfinishedDrafts = drafts
                                .Select(d => d.ToListItemResponse(draftPatient?.Name ?? string.Empty, draftDoctor?.FullName ?? string.Empty))
                                .ToList()
                        };
                    }
                }

                // Quick-create (US-019): always mints a real Consultation behind the scenes —
                // there is no Prescription without exactly one Consultation (see
                // PrescriptionRecord's doc comment). Goes straight to InConsultation since
                // there's no queue/check-in step to wait through.
                var yearPeriod = today.Year.ToString();
                var displaySeq = await _sequenceGenerator.GetNextAsync("ConsultationId", yearPeriod, cancellationToken);
                var displayCode = $"CN-{yearPeriod}-{displaySeq:0000}";
                var tokenPeriod = DateTimeUtility.FormatDate(today);
                var tokenSeq = await _sequenceGenerator.GetNextAsync($"ConsultationToken:{doctorId}", tokenPeriod, cancellationToken);
                var tokenNumber = $"T-{tokenSeq:00}";

                consultation = new Domain.Entities.Consultation
                {
                    DisplayCode = displayCode,
                    PatientId = patientId,
                    DoctorId = doctorId,
                    RegisteredByStaffId = null,
                    VisitDate = today,
                    TokenNumber = tokenNumber,
                    Status = ConsultationStatusEnum.InConsultation,
                    CheckInAt = DateTime.UtcNow
                };
                await _consultationRepository.AddAsync(consultation);
                await _unitOfWork.SaveChangesAsync();

                prescription = null;
            }
            else
            {
                throw new BadRequestException("One of consultationId, patientId, or prescriptionId is required.");
            }

            if (prescription is null)
            {
                prescription = await CreateBlankPrescriptionAsync(consultation, doctorId, cancellationToken);
            }

            var document = await BuildDocumentAsync(prescription, cancellationToken);
            return new StartOrResumePrescriptionResponse { Document = document };
        }

        private async Task<PrescriptionRecord> CreateBlankPrescriptionAsync(Domain.Entities.Consultation consultation, long doctorId, CancellationToken cancellationToken)
        {
            var doctor = await _doctorRepository.GetByIdAsync(doctorId)
                ?? throw new NotFoundException("Doctor", doctorId);

            var template = doctor.PreferredTemplateId.HasValue
                ? await _templateRepository.GetByIdAsync(doctor.PreferredTemplateId.Value)
                : null;
            if (template is null || !template.Enabled)
            {
                template = (await _templateRepository.GetAllAsync())
                    .FirstOrDefault(t => t.IsSystemDefault && t.Enabled)
                    ?? throw new BadRequestException("No enabled prescription template is available.");
            }

            var patient = await _patientRepository.GetByIdAsync(consultation.PatientId)
                ?? throw new NotFoundException("Patient", consultation.PatientId);

            var yearPeriod = DateTimeUtility.TodayLocal().Year.ToString();
            var seq = await _sequenceGenerator.GetNextAsync("PrescriptionId", yearPeriod, cancellationToken);
            var displayCode = $"RX-{yearPeriod}-{seq:0000}";

            var prescription = new PrescriptionRecord
            {
                DisplayCode = displayCode,
                ConsultationId = consultation.ConsultationId,
                PatientId = consultation.PatientId,
                DoctorId = doctorId,
                TemplateId = template.TemplateId,
                Language = doctor.PreferredLanguage ?? TemplateLanguageEnum.En,
                Status = PrescriptionStatusEnum.Draft
            };

            // US-009: a brand-new prescription (never a resumed draft) preloads the patient's
            // carried-forward Saved History as a single entry — never overwrites/merges, just seeds.
            if (!string.IsNullOrWhiteSpace(patient.SavedHistory))
                prescription.SetHistory(new List<string> { patient.SavedHistory! });

            await _prescriptionRepository.AddAsync(prescription);
            await _unitOfWork.SaveChangesAsync();

            return prescription;
        }

        private async Task<PrescriptionDocumentResponse> BuildDocumentAsync(PrescriptionRecord prescription, CancellationToken cancellationToken)
        {
            var patient = await _patientRepository.GetByIdAsync(prescription.PatientId)
                ?? throw new NotFoundException("Patient", prescription.PatientId);
            var doctor = await _doctorRepository.GetByIdAsync(prescription.DoctorId)
                ?? throw new NotFoundException("Doctor", prescription.DoctorId);
            var template = await _templateRepository.GetByIdAsync(prescription.TemplateId)
                ?? throw new NotFoundException("PrescriptionTemplate", prescription.TemplateId);
            // "Domain.Entities.HospitalSettings" spelled out — bare "HospitalSettings" resolves
            // to the Application.Features.HospitalSettings namespace instead of the entity type
            // from anywhere under Application.Features.*, same class of clash as PrescriptionRecord's.
            var hospitalSettings = (await _hospitalSettingsRepository.GetAllAsync()).FirstOrDefault()
                ?? new Domain.Entities.HospitalSettings();

            return prescription.ToDocumentResponse(patient, doctor, template, hospitalSettings);
        }
    }
}
