using MediatR;
using Microsoft.EntityFrameworkCore;
using SylviaNG.Prescription.Application.Features.Doctors.Models;
using SylviaNG.Prescription.Application.Interfaces.Repositories;
using SylviaNG.Prescription.Application.Mappings;

namespace SylviaNG.Prescription.Application.Features.Doctors.Queries.GetDoctorList
{
    public class GetDoctorListHandler : IRequestHandler<GetDoctorListQuery, DoctorListResponse>
    {
        private readonly IDoctorRepository _doctorRepository;
        private readonly IUserRepository _userRepository;

        public GetDoctorListHandler(IDoctorRepository doctorRepository, IUserRepository userRepository)
        {
            _doctorRepository = doctorRepository;
            _userRepository = userRepository;
        }

        public async Task<DoctorListResponse> Handle(GetDoctorListQuery query, CancellationToken cancellationToken)
        {
            var request = query.Request;
            var page = request.Page < 1 ? 1 : request.Page;
            var pageSize = request.PageSize < 1 ? 10 : request.PageSize;

            var joined =
                from d in _doctorRepository.Query()
                join u in _userRepository.Query() on d.UserId equals u.UserId
                select new { d, u };

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var term = request.SearchTerm.Trim().ToLower();
                joined = joined.Where(x =>
                    x.d.FullName.ToLower().Contains(term) ||
                    (x.d.Specialization != null && x.d.Specialization.ToLower().Contains(term)) ||
                    (x.d.LicenseNumber != null && x.d.LicenseNumber.ToLower().Contains(term)));
            }

            if (!string.IsNullOrWhiteSpace(request.Department))
                joined = joined.Where(x => x.d.Department == request.Department);

            if (request.IsActive.HasValue)
                joined = joined.Where(x => x.u.IsActive == request.IsActive.Value);

            var totalCount = await joined.CountAsync(cancellationToken);

            var pageItems = await joined
                .OrderByDescending(x => x.d.DoctorId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            var totalDoctors = await _doctorRepository.Query().CountAsync(cancellationToken);
            var activeDoctors = await _userRepository.Query()
                .Join(_doctorRepository.Query(), u => u.UserId, d => d.UserId, (u, d) => u)
                .CountAsync(u => u.IsActive, cancellationToken);

            return new DoctorListResponse
            {
                Doctors = pageItems.Select(x => x.d.ToSummaryResponse(x.u)).ToList(),
                TotalCount = totalCount,
                PageNumber = page,
                PageSize = pageSize,
                Summary = new DoctorListSummary
                {
                    TotalDoctors = totalDoctors,
                    ActiveDoctors = activeDoctors,
                    // Prescription/Medicine entities don't exist yet (Epic D/F) — real
                    // counts land when those tables do, not before.
                    TotalPrescriptions = 0,
                    TotalMedicineEntries = 0
                }
            };
        }
    }
}
