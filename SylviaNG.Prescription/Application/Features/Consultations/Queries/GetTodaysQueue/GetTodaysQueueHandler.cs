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
    /// A doctor's own same-day queue (Waiting/InConsultation only), oldest check-in first.
    /// No pagination — a single day's queue for one doctor is expected to be small.
    /// </summary>
    public class GetTodaysQueueHandler : IRequestHandler<GetTodaysQueueQuery, List<QueueItemResponse>>
    {
        private readonly IUserRepository _userRepository;
        private readonly IStaffRepository _staffRepository;
        private readonly IDoctorRepository _doctorRepository;
        private readonly IPatientRepository _patientRepository;
        private readonly IConsultationRepository _consultationRepository;

        public GetTodaysQueueHandler(
            IUserRepository userRepository,
            IStaffRepository staffRepository,
            IDoctorRepository doctorRepository,
            IPatientRepository patientRepository,
            IConsultationRepository consultationRepository)
        {
            _userRepository = userRepository;
            _staffRepository = staffRepository;
            _doctorRepository = doctorRepository;
            _patientRepository = patientRepository;
            _consultationRepository = consultationRepository;
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
                    && (c.Status == ConsultationStatusEnum.Waiting || c.Status == ConsultationStatusEnum.InConsultation)
                join p in _patientRepository.Query() on c.PatientId equals p.PatientId
                orderby c.CheckInAt
                select new { c, p };

            var items = await joined.ToListAsync(cancellationToken);
            var doctor = await _doctorRepository.GetByIdAsync(doctorId);
            var doctorName = doctor?.FullName ?? string.Empty;

            return items.Select(x => x.c.ToQueueItemResponse(x.p.Name, doctorName)).ToList();
        }
    }
}
