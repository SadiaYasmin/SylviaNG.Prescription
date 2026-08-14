using MediatR;
using Microsoft.EntityFrameworkCore;
using SylviaNG.Prescription.Application.Common;
using SylviaNG.Prescription.Application.Features.Prescriptions.Models;
using SylviaNG.Prescription.Application.Interfaces.Repositories;
using SylviaNG.Prescription.Application.Mappings;
using SylviaNG.Prescription.Domain.Enums;
using SylviaNG.Prescription.SharedKernel.Utils;

namespace SylviaNG.Prescription.Application.Features.Prescriptions.Queries.GetMyFinalizedPrescriptions
{
    public class GetMyFinalizedPrescriptionsHandler : IRequestHandler<GetMyFinalizedPrescriptionsQuery, PrescriptionListResponse>
    {
        private readonly IUserRepository _userRepository;
        private readonly IStaffRepository _staffRepository;
        private readonly IDoctorRepository _doctorRepository;
        private readonly IPatientRepository _patientRepository;
        private readonly IPrescriptionRepository _prescriptionRepository;

        public GetMyFinalizedPrescriptionsHandler(
            IUserRepository userRepository,
            IStaffRepository staffRepository,
            IDoctorRepository doctorRepository,
            IPatientRepository patientRepository,
            IPrescriptionRepository prescriptionRepository)
        {
            _userRepository = userRepository;
            _staffRepository = staffRepository;
            _doctorRepository = doctorRepository;
            _patientRepository = patientRepository;
            _prescriptionRepository = prescriptionRepository;
        }

        public async Task<PrescriptionListResponse> Handle(GetMyFinalizedPrescriptionsQuery query, CancellationToken cancellationToken)
        {
            var caller = await CallerContextResolver.ResolveCallerAsync(
                query.KeycloakId, _userRepository, _staffRepository, _doctorRepository);
            var doctorId = caller.DoctorId!.Value;
            var doctor = await _doctorRepository.GetByIdAsync(doctorId);
            var page = query.Page < 1 ? 1 : query.Page;
            var pageSize = query.PageSize < 1 ? 20 : query.PageSize;

            var joined =
                from p in _prescriptionRepository.Query()
                join pt in _patientRepository.Query() on p.PatientId equals pt.PatientId
                where p.DoctorId == doctorId && p.Status == PrescriptionStatusEnum.Finalized
                select new { p, pt };

            if (query.FromDate.HasValue)
            {
                var start = DateTimeUtility.StartOfDayUtc(query.FromDate.Value);
                joined = joined.Where(x => x.p.FinalizedAt != null && x.p.FinalizedAt >= start);
            }

            if (query.ToDate.HasValue)
            {
                var end = DateTimeUtility.EndOfDayUtc(query.ToDate.Value);
                joined = joined.Where(x => x.p.FinalizedAt != null && x.p.FinalizedAt < end);
            }

            if (!string.IsNullOrWhiteSpace(query.SearchTerm))
            {
                var term = query.SearchTerm.Trim().ToLower();
                joined = joined.Where(x =>
                    x.pt.Name.ToLower().Contains(term) ||
                    x.p.DisplayCode.ToLower().Contains(term));
            }

            var totalCount = await joined.CountAsync(cancellationToken);

            var pageItems = await joined
                .OrderByDescending(x => x.p.FinalizedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return new PrescriptionListResponse
            {
                Prescriptions = pageItems
                    .Select(x => x.p.ToListItemResponse(x.pt.Name, doctor?.FullName ?? string.Empty))
                    .ToList(),
                TotalCount = totalCount,
                PageNumber = page,
                PageSize = pageSize
            };
        }
    }
}
