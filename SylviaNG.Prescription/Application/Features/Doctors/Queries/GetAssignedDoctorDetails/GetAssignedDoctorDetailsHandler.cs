using MediatR;
using Microsoft.EntityFrameworkCore;
using SylviaNG.Prescription.Application.Common;
using SylviaNG.Prescription.Application.Common.Exceptions;
using SylviaNG.Prescription.Application.Features.Doctors.Models;
using SylviaNG.Prescription.Application.Interfaces.Repositories;
using SylviaNG.Prescription.Domain.Enums;
using SylviaNG.Prescription.SharedKernel.Generic;
using SylviaNG.Prescription.SharedKernel.Utils;

namespace SylviaNG.Prescription.Application.Features.Doctors.Queries.GetAssignedDoctorDetails
{
    public class GetAssignedDoctorDetailsHandler : IRequestHandler<GetAssignedDoctorDetailsQuery, AssignedDoctorDetailsResponse>
    {
        private readonly IUserRepository _userRepository;
        private readonly IStaffRepository _staffRepository;
        private readonly IDoctorRepository _doctorRepository;
        private readonly IConsultationRepository _consultationRepository;
        private readonly IUnitOfWork _unitOfWork;

        public GetAssignedDoctorDetailsHandler(
            IUserRepository userRepository,
            IStaffRepository staffRepository,
            IDoctorRepository doctorRepository,
            IConsultationRepository consultationRepository,
            IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _staffRepository = staffRepository;
            _doctorRepository = doctorRepository;
            _consultationRepository = consultationRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<AssignedDoctorDetailsResponse> Handle(GetAssignedDoctorDetailsQuery query, CancellationToken cancellationToken)
        {
            var caller = await CallerContextResolver.ResolveCallerAsync(
                query.KeycloakId, _userRepository, _staffRepository, _doctorRepository);
            var staffId = caller.StaffId!.Value;

            // Not just "does this doctor exist" — must be assigned to THIS staff member. Not
            // distinguishing "doesn't exist" from "not yours" in the error (both 404) so a staff
            // user can't fish for which doctorIds exist by probing the endpoint.
            var assignment = await _unitOfWork.Context.StaffDoctors
                .FirstOrDefaultAsync(sd => sd.StaffId == staffId && sd.DoctorId == query.DoctorId, cancellationToken);
            if (assignment is null)
            {
                throw new NotFoundException("Doctor", query.DoctorId);
            }

            var doctor = await _doctorRepository.GetByIdAsync(query.DoctorId)
                ?? throw new NotFoundException("Doctor", query.DoctorId);
            var user = await _userRepository.GetByIdAsync(doctor.UserId)
                ?? throw new NotFoundException("User", doctor.UserId);

            var today = DateTimeUtility.TodayLocal();
            var todayAppointments = await _consultationRepository.Query()
                .CountAsync(c => c.DoctorId == doctor.DoctorId && c.VisitDate == today, cancellationToken);
            var completedConsultations = await _consultationRepository.Query()
                .CountAsync(c => c.DoctorId == doctor.DoctorId && c.VisitDate == today && c.Status == ConsultationStatusEnum.Completed, cancellationToken);

            return new AssignedDoctorDetailsResponse
            {
                DoctorId = doctor.DoctorId,
                FullName = doctor.FullName,
                Specialization = doctor.Specialization,
                Department = doctor.Department,
                Email = user.Email,
                Phone = doctor.Phone,
                IsActive = user.IsActive,
                PhotoUrl = doctor.PhotoUrl,
                AssignedDate = assignment.CreatedAt,
                TodayAppointments = todayAppointments,
                CompletedConsultations = completedConsultations
            };
        }
    }
}
