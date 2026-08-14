using MediatR;
using Microsoft.EntityFrameworkCore;
using SylviaNG.Prescription.Application.Common;
using SylviaNG.Prescription.Application.Common.Exceptions;
using SylviaNG.Prescription.Application.Common.Services;
using SylviaNG.Prescription.Application.Features.Consultations.Models;
using SylviaNG.Prescription.Application.Features.Prescriptions;
using SylviaNG.Prescription.Application.Interfaces.Repositories;
using SylviaNG.Prescription.Application.Mappings;
using SylviaNG.Prescription.Domain.Entities;
using SylviaNG.Prescription.Domain.Enums;
using SylviaNG.Prescription.SharedKernel.Generic;
using SylviaNG.Prescription.SharedKernel.Utils;

namespace SylviaNG.Prescription.Application.Features.Consultations.Commands.CreateConsultation
{
    /// <summary>
    /// Checks a patient in for a queued consultation with one of the calling Staff member's
    /// assigned doctors. Only Staff can hit this (enforced by [Authorize] on the
    /// controller) so the handler resolves the caller via CallerContextResolver and assumes
    /// Staff — it never needs to branch on role itself.
    /// </summary>
    public class CreateConsultationHandler : IRequestHandler<CreateConsultationCommand, CreateConsultationResponse>
    {
        private readonly IUserRepository _userRepository;
        private readonly IStaffRepository _staffRepository;
        private readonly IDoctorRepository _doctorRepository;
        private readonly IPatientRepository _patientRepository;
        private readonly IConsultationRepository _consultationRepository;
        private readonly IPrescriptionRepository _prescriptionRepository;
        private readonly ISequenceGenerator _sequenceGenerator;
        private readonly IUnitOfWork _unitOfWork;

        public CreateConsultationHandler(
            IUserRepository userRepository,
            IStaffRepository staffRepository,
            IDoctorRepository doctorRepository,
            IPatientRepository patientRepository,
            IConsultationRepository consultationRepository,
            IPrescriptionRepository prescriptionRepository,
            ISequenceGenerator sequenceGenerator,
            IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _staffRepository = staffRepository;
            _doctorRepository = doctorRepository;
            _patientRepository = patientRepository;
            _consultationRepository = consultationRepository;
            _prescriptionRepository = prescriptionRepository;
            _sequenceGenerator = sequenceGenerator;
            _unitOfWork = unitOfWork;
        }

        public async Task<CreateConsultationResponse> Handle(CreateConsultationCommand command, CancellationToken cancellationToken)
        {
            var caller = await CallerContextResolver.ResolveCallerAsync(
                command.KeycloakId, _userRepository, _staffRepository, _doctorRepository);
            var staffId = caller.StaffId!.Value;

            var request = command.Request;

            var isAssignedDoctor = await _unitOfWork.Context.StaffDoctors
                .AnyAsync(sd => sd.StaffId == staffId && sd.DoctorId == request.DoctorId, cancellationToken);
            if (!isAssignedDoctor)
                throw new BadRequestException("The selected doctor is not assigned to you.");

            var today = DateTimeUtility.TodayLocal();
            if (request.VisitDate.HasValue && request.VisitDate.Value != today)
                throw new BadRequestException("Consultations can only be created for today.");

            var visitDate = today;

            var existing = await _consultationRepository.Query()
                .Where(c => c.PatientId == request.PatientId
                    && c.VisitDate == visitDate
                    && (c.Status == ConsultationStatusEnum.Waiting || c.Status == ConsultationStatusEnum.InConsultation))
                .FirstOrDefaultAsync(cancellationToken);

            // One active consultation per patient per day, regardless of doctor.
            // Force is intentionally ignored so staff cannot create a second queue entry.
            if (existing is not null)
            {
                return new CreateConsultationResponse
                {
                    DuplicateFound = true,
                    ExistingConsultation = existing.ToSummaryResponse()
                };
            }

            // US-012: an unfinished draft for this patient with this doctor, from any day —
            // unlike the active-duplicate check above, this one respects Force so staff can
            // explicitly proceed after seeing the prompt (feature.md: "resume the existing
            // draft, view all of that patient's drafts, or explicitly start fresh anyway").
            if (!request.Force)
            {
                var draftConsultationIds = await _consultationRepository.Query()
                    .Where(c => c.PatientId == request.PatientId && c.DoctorId == request.DoctorId && c.Status == ConsultationStatusEnum.Draft)
                    .Select(c => c.ConsultationId)
                    .ToListAsync(cancellationToken);
                if (draftConsultationIds.Count > 0)
                {
                    var drafts = await _prescriptionRepository.Query()
                        .Where(p => draftConsultationIds.Contains(p.ConsultationId))
                        .ToListAsync(cancellationToken);
                    var draftPatient = await _patientRepository.GetByIdAsync(request.PatientId);
                    var draftDoctor = await _doctorRepository.GetByIdAsync(request.DoctorId);
                    return new CreateConsultationResponse
                    {
                        UnfinishedDraftFound = true,
                        UnfinishedDrafts = drafts
                            .Select(d => d.ToListItemResponse(draftPatient?.Name ?? string.Empty, draftDoctor?.FullName ?? string.Empty))
                            .ToList()
                    };
                }
            }

            var yearPeriod = DateTimeUtility.TodayLocal().Year.ToString();
            var displaySeq = await _sequenceGenerator.GetNextAsync("ConsultationId", yearPeriod, cancellationToken);
            var displayCode = $"CN-{yearPeriod}-{displaySeq:0000}";

            var tokenPeriod = DateTimeUtility.FormatDate(visitDate);
            var tokenSeq = await _sequenceGenerator.GetNextAsync($"ConsultationToken:{request.DoctorId}", tokenPeriod, cancellationToken);
            var tokenNumber = $"T-{tokenSeq:00}";

            var consultation = new Consultation
            {
                DisplayCode = displayCode,
                PatientId = request.PatientId,
                DoctorId = request.DoctorId,
                RegisteredByStaffId = staffId,
                VisitDate = visitDate,
                TokenNumber = tokenNumber,
                Status = ConsultationStatusEnum.Waiting,
                CheckInAt = DateTime.UtcNow
            };

            await _consultationRepository.AddAsync(consultation);
            await _unitOfWork.SaveChangesAsync();

            return new CreateConsultationResponse
            {
                DuplicateFound = false,
                Consultation = consultation.ToSummaryResponse()
            };
        }
    }
}
