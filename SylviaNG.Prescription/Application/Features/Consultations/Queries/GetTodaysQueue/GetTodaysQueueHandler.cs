using MediatR;
using Microsoft.EntityFrameworkCore;
using SylviaNG.Prescription.Application.Common;
using SylviaNG.Prescription.Application.Features.Consultations.Models;
using SylviaNG.Prescription.Application.Interfaces.Repositories;
using SylviaNG.Prescription.Application.Mappings;
using SylviaNG.Prescription.Domain.Enums;
using SylviaNG.Prescription.SharedKernel.Utils;

namespace SylviaNG.Prescription.Application.Features.Consultations.Queries.GetTodaysQueue
{
    /// <summary>
    /// A doctor's own same-day queue (Waiting/InConsultation/Draft), oldest check-in first.
    /// Draft is included so a saved-but-unfinished prescription still shows up today with its
    /// own "Continue Draft" action, rather than disappearing from the doctor's view entirely.
    /// No pagination — a single day's queue for one doctor is expected to be small.
    /// </summary>
    public class GetTodaysQueueHandler : IRequestHandler<GetTodaysQueueQuery, List<QueueItemResponse>>
    {
        private readonly IUserRepository _userRepository;
        private readonly IStaffRepository _staffRepository;
        private readonly IDoctorRepository _doctorRepository;
        private readonly IPatientRepository _patientRepository;
        private readonly IConsultationRepository _consultationRepository;
        private readonly IPrescriptionRepository _prescriptionRepository;

        public GetTodaysQueueHandler(
            IUserRepository userRepository,
            IStaffRepository staffRepository,
            IDoctorRepository doctorRepository,
            IPatientRepository patientRepository,
            IConsultationRepository consultationRepository,
            IPrescriptionRepository prescriptionRepository)
        {
            _userRepository = userRepository;
            _staffRepository = staffRepository;
            _doctorRepository = doctorRepository;
            _patientRepository = patientRepository;
            _consultationRepository = consultationRepository;
            _prescriptionRepository = prescriptionRepository;
        }

        public async Task<List<QueueItemResponse>> Handle(GetTodaysQueueQuery query, CancellationToken cancellationToken)
        {
            var caller = await CallerContextResolver.ResolveCallerAsync(
                query.KeycloakId, _userRepository, _staffRepository, _doctorRepository);
            var doctorId = caller.DoctorId!.Value;
            var today = DateTimeUtility.TodayLocal();

            var joined =
                from c in _consultationRepository.Query()
                where c.DoctorId == doctorId
                    && c.VisitDate == today
                    && (c.Status == ConsultationStatusEnum.Waiting || c.Status == ConsultationStatusEnum.InConsultation || c.Status == ConsultationStatusEnum.Draft)
                join p in _patientRepository.Query() on c.PatientId equals p.PatientId
                orderby c.CheckInAt
                select new { c, p };

            var items = await joined.ToListAsync(cancellationToken);
            var doctor = await _doctorRepository.GetByIdAsync(doctorId);
            var doctorName = doctor?.FullName ?? string.Empty;

            var consultationIds = items.Select(x => x.c.ConsultationId).ToList();
            var savedConsultationIds = (await _prescriptionRepository.Query()
                .Where(p => consultationIds.Contains(p.ConsultationId) && p.SavedAt != null)
                .Select(p => p.ConsultationId)
                .ToListAsync(cancellationToken))
                .ToHashSet();

            return items
                .Select(x => x.c.ToQueueItemResponse(x.p.Name, doctorName, savedConsultationIds.Contains(x.c.ConsultationId)))
                .ToList();
        }
    }
}
