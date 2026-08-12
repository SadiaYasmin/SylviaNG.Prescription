using MediatR;
using Microsoft.EntityFrameworkCore;
using SylviaNG.Prescription.Application.Common;
using SylviaNG.Prescription.Application.Features.Consultations.Models;
using SylviaNG.Prescription.Application.Interfaces.Repositories;
using SylviaNG.Prescription.Application.Mappings;
using SylviaNG.Prescription.Domain.Enums;
using SylviaNG.Prescription.SharedKernel.Utils;

namespace SylviaNG.Prescription.Application.Features.Consultations.Queries.GetMyQueue
{
    /// <summary>
    /// A staff member's own same-day queue of consultations they registered (Waiting/
    /// InConsultation only), oldest check-in first. Unlike GetTodaysQueue, also joins to
    /// Doctor for a per-row doctor name — a staff member may have consultations queued with
    /// multiple assigned doctors.
    /// </summary>
    public class GetMyQueueHandler : IRequestHandler<GetMyQueueQuery, List<QueueItemResponse>>
    {
        private readonly IUserRepository _userRepository;
        private readonly IStaffRepository _staffRepository;
        private readonly IDoctorRepository _doctorRepository;
        private readonly IPatientRepository _patientRepository;
        private readonly IConsultationRepository _consultationRepository;

        public GetMyQueueHandler(
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

        public async Task<List<QueueItemResponse>> Handle(GetMyQueueQuery query, CancellationToken cancellationToken)
        {
            var caller = await CallerContextResolver.ResolveCallerAsync(
                query.KeycloakId, _userRepository, _staffRepository, _doctorRepository);
            var staffId = caller.StaffId!.Value;
            var today = DateTimeUtility.TodayLocal();

            var joined =
                from c in _consultationRepository.Query()
                where c.RegisteredByStaffId == staffId
                    && c.VisitDate == today
                    && (c.Status == ConsultationStatusEnum.Waiting || c.Status == ConsultationStatusEnum.InConsultation)
                join p in _patientRepository.Query() on c.PatientId equals p.PatientId
                join d in _doctorRepository.Query() on c.DoctorId equals d.DoctorId
                orderby c.CheckInAt
                select new { c, p, d };

            var items = await joined.ToListAsync(cancellationToken);

            return items.Select(x => x.c.ToQueueItemResponse(x.p.Name, x.d.FullName)).ToList();
        }
    }
}
