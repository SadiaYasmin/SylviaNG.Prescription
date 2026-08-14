using MediatR;
using Microsoft.EntityFrameworkCore;
using SylviaNG.Prescription.Application.Common;
using SylviaNG.Prescription.Application.Features.Patients.Models;
using SylviaNG.Prescription.Application.Interfaces.Repositories;
using SylviaNG.Prescription.Application.Mappings;
using SylviaNG.Prescription.Domain.Enums;
using SylviaNG.Prescription.SharedKernel.Generic;
using SylviaNG.Prescription.SharedKernel.Utils;

namespace SylviaNG.Prescription.Application.Features.Patients.Queries.GetDoctorPatientQueue
{
    /// <summary>
    /// Backs Create Prescription's "Patient Filter" (US-019 follow-up): a doctor's
    /// staff-scoped patient roster (reusing <see cref="PatientVisibilityScope"/>, same as
    /// GetPatientList), left-joined to that patient's own consultation with THIS doctor for
    /// today (if any), so every row carries a today-status the frontend uses to pick the
    /// right action button. Unpaged, same reasoning as GetTodaysQueue/GetMyQueue.
    /// </summary>
    public class GetDoctorPatientQueueHandler : IRequestHandler<GetDoctorPatientQueueQuery, DoctorPatientQueueResponse>
    {
        private readonly IUserRepository _userRepository;
        private readonly IStaffRepository _staffRepository;
        private readonly IDoctorRepository _doctorRepository;
        private readonly IPatientRepository _patientRepository;
        private readonly IConsultationRepository _consultationRepository;
        private readonly IPrescriptionRepository _prescriptionRepository;
        private readonly IUnitOfWork _unitOfWork;

        public GetDoctorPatientQueueHandler(
            IUserRepository userRepository,
            IStaffRepository staffRepository,
            IDoctorRepository doctorRepository,
            IPatientRepository patientRepository,
            IConsultationRepository consultationRepository,
            IPrescriptionRepository prescriptionRepository,
            IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _staffRepository = staffRepository;
            _doctorRepository = doctorRepository;
            _patientRepository = patientRepository;
            _consultationRepository = consultationRepository;
            _prescriptionRepository = prescriptionRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<DoctorPatientQueueResponse> Handle(GetDoctorPatientQueueQuery query, CancellationToken cancellationToken)
        {
            var request = query.Request;
            var caller = await CallerContextResolver.ResolveCallerAsync(
                query.KeycloakId, _userRepository, _staffRepository, _doctorRepository);
            var doctorId = caller.DoctorId!.Value;
            var today = DateTimeUtility.TodayLocal();

            var scopedPatients = await PatientVisibilityScope.ApplyAsync(
                _patientRepository.Query(), _unitOfWork.Context, caller, cancellationToken);

            var todaysConsultations = _consultationRepository.Query()
                .Where(c => c.DoctorId == doctorId && c.VisitDate == today);

            var joined =
                from p in scopedPatients
                join c in todaysConsultations on p.PatientId equals c.PatientId into consultGroup
                from c in consultGroup.DefaultIfEmpty()
                select new { p, c };

            switch (request.QueueFilter)
            {
                case PatientQueueFilterEnum.TodayQueue:
                    joined = joined.Where(x =>
                        x.c != null && (x.c.Status == ConsultationStatusEnum.Waiting || x.c.Status == ConsultationStatusEnum.InConsultation));
                    break;

                case PatientQueueFilterEnum.NotConsultedToday:
                    joined = joined.Where(x => x.c == null || x.c.Status != ConsultationStatusEnum.Completed);
                    break;

                case PatientQueueFilterEnum.CompletedToday:
                    joined = joined.Where(x => x.c != null && x.c.Status == ConsultationStatusEnum.Completed);
                    break;

                case PatientQueueFilterEnum.AllRegistered:
                default:
                    break;
            }

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var term = request.SearchTerm.Trim().ToLower();
                joined = joined.Where(x => x.p.Name.ToLower().Contains(term) || x.p.Phone.ToLower().Contains(term));
            }

            var raw = await joined.ToListAsync(cancellationToken);

            // A patient could in principle have more than one Consultation row with this
            // doctor today (e.g. Completed, then checked in again) — collapse to one row per
            // patient, preferring the most recently checked-in consultation, so the picker
            // never shows the same patient twice.
            var deduped = raw
                .GroupBy(x => x.p.PatientId)
                .Select(g => g.Count() == 1 ? g.First() : g.OrderByDescending(x => x.c?.CheckInAt ?? DateTime.MinValue).First())
                .ToList();

            var ordered = request.QueueFilter == PatientQueueFilterEnum.TodayQueue
                ? deduped.OrderBy(x => x.c?.CheckInAt ?? DateTime.MaxValue).ToList()
                : deduped.OrderBy(x => x.p.Name).ToList();

            var completedConsultationIds = ordered
                .Where(x => x.c != null && x.c.Status == ConsultationStatusEnum.Completed)
                .Select(x => x.c!.ConsultationId)
                .ToList();

            var prescriptionIdByConsultationId = completedConsultationIds.Count > 0
                ? await _prescriptionRepository.Query()
                    .Where(pr => completedConsultationIds.Contains(pr.ConsultationId))
                    .ToDictionaryAsync(pr => pr.ConsultationId, pr => pr.PrescriptionId, cancellationToken)
                : new Dictionary<long, long>();

            var staffIds = ordered.Select(x => x.p.RegisteredByStaffId).Distinct().ToList();
            var staffNamesById = await _unitOfWork.Context.Staff
                .Where(s => staffIds.Contains(s.StaffId))
                .ToDictionaryAsync(s => s.StaffId, s => s.FullName, cancellationToken);

            var patients = ordered.Select(x => x.p.ToDoctorQueueItemResponse(
                staffNamesById.GetValueOrDefault(x.p.RegisteredByStaffId, string.Empty),
                x.c?.ConsultationId,
                x.c?.Status,
                x.c != null && x.c.Status == ConsultationStatusEnum.Completed
                    ? prescriptionIdByConsultationId.GetValueOrDefault(x.c.ConsultationId)
                    : (long?)null))
                .ToList();

            return new DoctorPatientQueueResponse { Patients = patients };
        }
    }
}
