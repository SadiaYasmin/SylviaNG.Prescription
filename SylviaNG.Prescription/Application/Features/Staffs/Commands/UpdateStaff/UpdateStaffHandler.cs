using MediatR;
using Microsoft.EntityFrameworkCore;
using SylviaNG.Prescription.Application.Common.Exceptions;
using SylviaNG.Prescription.Application.Features.Staffs.Models;
using SylviaNG.Prescription.Application.Interfaces.Externals;
using SylviaNG.Prescription.Application.Interfaces.Repositories;
using SylviaNG.Prescription.Application.Mappings;
using SylviaNG.Prescription.Domain.Entities;
using SylviaNG.Prescription.SharedKernel.Generic;
using SylviaNG.Prescription.SharedKernel.Utils;

namespace SylviaNG.Prescription.Application.Features.Staffs.Commands.UpdateStaff
{
    public class UpdateStaffHandler : IRequestHandler<UpdateStaffCommand, StaffSummaryResponse>
    {
        private readonly IStaffRepository _staffRepository;
        private readonly IUserRepository _userRepository;
        private readonly IDoctorRepository _doctorRepository;
        private readonly IKeycloakAdminClient _adminClient;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateStaffHandler(
            IStaffRepository staffRepository,
            IUserRepository userRepository,
            IDoctorRepository doctorRepository,
            IKeycloakAdminClient adminClient,
            IUnitOfWork unitOfWork)
        {
            _staffRepository = staffRepository;
            _userRepository = userRepository;
            _doctorRepository = doctorRepository;
            _adminClient = adminClient;
            _unitOfWork = unitOfWork;
        }

        public async Task<StaffSummaryResponse> Handle(UpdateStaffCommand command, CancellationToken cancellationToken)
        {
            var staff = await _staffRepository.GetByIdAsync(command.StaffId)
                ?? throw new NotFoundException("Staff", command.StaffId);
            var user = await _userRepository.GetByIdAsync(staff.UserId)
                ?? throw new NotFoundException("User", staff.UserId);

            var request = command.Request;
            var requestedDoctorIds = request.AssignedDoctorIds.Distinct().ToList();

            foreach (var doctorId in requestedDoctorIds)
            {
                if (await _doctorRepository.GetByIdAsync(doctorId) is null)
                    throw new NotFoundException("Doctor", doctorId);
            }

            staff.FullName = request.FullName;
            staff.Phone = request.Phone;

            if (!string.IsNullOrWhiteSpace(request.Email))
                user.Email = request.Email;

            if (user.IsActive != request.IsActive)
            {
                await _adminClient.SetUserEnabledAsync(user.KeycloakId, request.IsActive);
                user.IsActive = request.IsActive;
            }

            var existingLinks = await _unitOfWork.Context.StaffDoctors
                .Where(sd => sd.StaffId == staff.StaffId)
                .ToListAsync(cancellationToken);

            var existingDoctorIds = existingLinks.Select(l => l.DoctorId).ToHashSet();
            var requestedSet = requestedDoctorIds.ToHashSet();

            var toRemove = existingLinks.Where(l => !requestedSet.Contains(l.DoctorId)).ToList();
            if (toRemove.Count > 0)
                _unitOfWork.Context.StaffDoctors.RemoveRange(toRemove);

            var toAdd = requestedDoctorIds
                .Where(id => !existingDoctorIds.Contains(id))
                .Select(id => new StaffDoctor { StaffId = staff.StaffId, DoctorId = id, CreatedAt = DateTimeUtility.NowUtc() });
            _unitOfWork.Context.StaffDoctors.AddRange(toAdd);

            _staffRepository.Update(staff);
            _userRepository.Update(user);
            await _unitOfWork.SaveChangesAsync();

            var assignedDoctors = await _unitOfWork.Context.StaffDoctors
                .Where(sd => sd.StaffId == staff.StaffId)
                .Join(_unitOfWork.Context.Doctors, sd => sd.DoctorId, d => d.DoctorId,
                    (sd, d) => new AssignedDoctorSummary { DoctorId = d.DoctorId, FullName = d.FullName, Department = d.Department })
                .ToListAsync(cancellationToken);

            return staff.ToSummaryResponse(user, assignedDoctors);
        }
    }
}
