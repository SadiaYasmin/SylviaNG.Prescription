using MediatR;
using Microsoft.EntityFrameworkCore;
using SylviaNG.Prescription.Application.Common;
using SylviaNG.Prescription.Application.Features.Patients.Models;
using SylviaNG.Prescription.Application.Interfaces.Repositories;
using SylviaNG.Prescription.Application.Mappings;
using SylviaNG.Prescription.Domain.Enums;
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

            if (request.CompletedWithMeOnly && caller.Role == UserRoleEnum.Doctor && caller.DoctorId.HasValue)
            {
                var completedQuery = _unitOfWork.Context.Consultations
                    .Where(c => c.DoctorId == caller.DoctorId.Value && c.Status == ConsultationStatusEnum.Completed);
                if (request.From.HasValue && request.To.HasValue)
                {
                    var from = DateTime.SpecifyKind(request.From.Value, DateTimeKind.Utc);
                    var to = DateTime.SpecifyKind(request.To.Value, DateTimeKind.Utc);
                    completedQuery = completedQuery.Where(c => c.CheckInAt >= from && c.CheckInAt < to);
                }
                var completedPatientIds = await completedQuery.Select(c => c.PatientId).Distinct().ToListAsync(cancellationToken);
                scoped = scoped.Where(p => completedPatientIds.Contains(p.PatientId));
            }
            else if (request.ReturningOnly && request.From.HasValue && request.To.HasValue)
            {
                // Mirrors GetPatientAnalyticsHandler's "Returning" definition exactly: registered
                // BEFORE the range started AND has >=1 Completed consultation inside it — counted
                // as a distinct patient regardless of how many Completed consultations they have.
                var from = DateTime.SpecifyKind(request.From.Value, DateTimeKind.Utc);
                var to = DateTime.SpecifyKind(request.To.Value, DateTimeKind.Utc);
                var completedPatientIds = await _unitOfWork.Context.Consultations
                    .Where(c => c.CheckInAt >= from && c.CheckInAt < to && c.Status == ConsultationStatusEnum.Completed)
                    .Select(c => c.PatientId)
                    .Distinct()
                    .ToListAsync(cancellationToken);
                scoped = scoped.Where(p => completedPatientIds.Contains(p.PatientId) && p.RegisteredAt < from);
            }
            else if (request.From.HasValue && request.To.HasValue)
            {
                // Also backs NewOnly — "New" is simply registered inside [From,To), so it shares
                // this branch with the plain registration-date filter (same condition either way).
                var from = DateTime.SpecifyKind(request.From.Value, DateTimeKind.Utc);
                var to = DateTime.SpecifyKind(request.To.Value, DateTimeKind.Utc);
                scoped = scoped.Where(p => p.RegisteredAt >= from && p.RegisteredAt < to);
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
