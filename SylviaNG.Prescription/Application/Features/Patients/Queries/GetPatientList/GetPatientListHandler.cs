using MediatR;
using Microsoft.EntityFrameworkCore;
using SylviaNG.Prescription.Application.Common;
using SylviaNG.Prescription.Application.Features.Patients.Models;
using SylviaNG.Prescription.Application.Interfaces.Repositories;
using SylviaNG.Prescription.Application.Mappings;
using SylviaNG.Prescription.SharedKernel.Generic;

namespace SylviaNG.Prescription.Application.Features.Patients.Queries.GetPatientList
{
    public class GetPatientListHandler : IRequestHandler<GetPatientListQuery, PatientListResponse>
    {
        private readonly IPatientRepository _patientRepository;
        private readonly IUserRepository _userRepository;
        private readonly IStaffRepository _staffRepository;
        private readonly IDoctorRepository _doctorRepository;
        private readonly IUnitOfWork _unitOfWork;

        public GetPatientListHandler(
            IPatientRepository patientRepository,
            IUserRepository userRepository,
            IStaffRepository staffRepository,
            IDoctorRepository doctorRepository,
            IUnitOfWork unitOfWork)
        {
            _patientRepository = patientRepository;
            _userRepository = userRepository;
            _staffRepository = staffRepository;
            _doctorRepository = doctorRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<PatientListResponse> Handle(GetPatientListQuery query, CancellationToken cancellationToken)
        {
            var request = query.Request;
            var page = request.Page < 1 ? 1 : request.Page;
            var pageSize = request.PageSize < 1 ? 10 : request.PageSize;

            var caller = await CallerContextResolver.ResolveCallerAsync(
                query.KeycloakId, _userRepository, _staffRepository, _doctorRepository);

            var scoped = await PatientVisibilityScope.ApplyAsync(
                _patientRepository.Query(), _unitOfWork.Context, caller, cancellationToken);

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var term = request.SearchTerm.Trim().ToLower();
                scoped = scoped.Where(p => p.Name.ToLower().Contains(term) || p.Phone.ToLower().Contains(term));
            }

            var totalCount = await scoped.CountAsync(cancellationToken);

            // Newest-first, matching the list-ordering convention already established for
            // Staff (OrderByDescending on the primary key), not alphabetical.
            var pageItems = await scoped
                .OrderByDescending(p => p.PatientId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            var staffIds = pageItems.Select(p => p.RegisteredByStaffId).Distinct().ToList();
            var staffNamesById = await _unitOfWork.Context.Staff
                .Where(s => staffIds.Contains(s.StaffId))
                .ToDictionaryAsync(s => s.StaffId, s => s.FullName, cancellationToken);

            return new PatientListResponse
            {
                Patients = pageItems
                    .Select(p => p.ToSummaryResponse(staffNamesById.GetValueOrDefault(p.RegisteredByStaffId, string.Empty)))
                    .ToList(),
                TotalCount = totalCount,
                PageNumber = page,
                PageSize = pageSize
            };
        }
    }
}
