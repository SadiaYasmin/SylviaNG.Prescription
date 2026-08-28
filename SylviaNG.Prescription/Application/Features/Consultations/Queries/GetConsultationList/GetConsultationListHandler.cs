using MediatR;
using Microsoft.EntityFrameworkCore;
using SylviaNG.Prescription.Application.Features.Consultations.Models;
using SylviaNG.Prescription.Application.Interfaces.Repositories;
using SylviaNG.Prescription.Application.Mappings;
using SylviaNG.Prescription.Domain.Enums;
using SylviaNG.Prescription.SharedKernel.Utils;

namespace SylviaNG.Prescription.Application.Features.Consultations.Queries.GetConsultationList
{
    public class GetConsultationListHandler : IRequestHandler<GetConsultationListQuery, ConsultationListResponse>
    {
        private readonly IConsultationRepository _consultationRepository;
        private readonly IPatientRepository _patientRepository;
        private readonly IDoctorRepository _doctorRepository;

        public GetConsultationListHandler(
            IConsultationRepository consultationRepository,
            IPatientRepository patientRepository,
            IDoctorRepository doctorRepository)
        {
            _consultationRepository = consultationRepository;
            _patientRepository = patientRepository;
            _doctorRepository = doctorRepository;
        }

        public async Task<ConsultationListResponse> Handle(GetConsultationListQuery query, CancellationToken cancellationToken)
        {
            var request = query.Request;
            var page = request.Page < 1 ? 1 : request.Page;
            var pageSize = request.PageSize < 1 ? 10 : request.PageSize;

            var joined =
                from c in _consultationRepository.Query()
                join p in _patientRepository.Query() on c.PatientId equals p.PatientId
                join d in _doctorRepository.Query() on c.DoctorId equals d.DoctorId
                select new { c, p, d };

            switch (request.DateMode)
            {
                case ConsultationDateModeEnum.Today:
                    var today = DateTimeUtility.TodayLocal();
                    joined = joined.Where(x => x.c.VisitDate == today);
                    break;

                case ConsultationDateModeEnum.Yesterday:
                    var yesterday = DateTimeUtility.TodayLocal().AddDays(-1);
                    joined = joined.Where(x => x.c.VisitDate == yesterday);
                    break;

                case ConsultationDateModeEnum.Custom:
                    if (request.Date.HasValue)
                        joined = joined.Where(x => x.c.VisitDate == request.Date.Value);
                    break;

                case ConsultationDateModeEnum.Range:
                    if (request.FromDate.HasValue)
                        joined = joined.Where(x => x.c.VisitDate >= request.FromDate.Value);
                    if (request.ToDate.HasValue)
                        joined = joined.Where(x => x.c.VisitDate <= request.ToDate.Value);
                    break;
            }

            if (request.DoctorId.HasValue)
                joined = joined.Where(x => x.c.DoctorId == request.DoctorId.Value);

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var term = request.SearchTerm.Trim().ToLower();
                joined = joined.Where(x =>
                    x.p.Name.ToLower().Contains(term) ||
                    x.c.TokenNumber.ToLower().Contains(term) ||
                    x.p.Phone.ToLower().Contains(term));
            }

            // Summary tiles reflect date/doctor/search but NOT the status filter — they're an
            // always-on breakdown, not a mirror of whatever status the table is currently
            // narrowed to (a Status-filtered view would otherwise make 4 of the 5 tiles read 0).
            var waitingCount = await joined.CountAsync(x => x.c.Status == ConsultationStatusEnum.Waiting, cancellationToken);
            var inProgressCount = await joined.CountAsync(x => x.c.Status == ConsultationStatusEnum.InConsultation, cancellationToken);
            var completedCount = await joined.CountAsync(x => x.c.Status == ConsultationStatusEnum.Completed, cancellationToken);
            var draftCount = await joined.CountAsync(x => x.c.Status == ConsultationStatusEnum.Draft, cancellationToken);
            var summaryTotal = waitingCount + inProgressCount + completedCount + draftCount;

            if (request.Status.HasValue)
                joined = joined.Where(x => x.c.Status == request.Status.Value);

            var totalCount = await joined.CountAsync(cancellationToken);

            var pageItems = await joined
                .OrderByDescending(x => x.c.ConsultationId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return new ConsultationListResponse
            {
                Consultations = pageItems
                    .Select(x => x.c.ToListItemResponse(x.p.Name, x.p.Phone, x.d.FullName))
                    .ToList(),
                TotalCount = totalCount,
                PageNumber = page,
                PageSize = pageSize,
                Summary = new ConsultationListSummary
                {
                    Total = summaryTotal,
                    Waiting = waitingCount,
                    InProgress = inProgressCount,
                    Completed = completedCount,
                    Draft = draftCount
                }
            };
        }
    }
}
