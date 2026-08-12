using MediatR;
using SylviaNG.Prescription.Application.Common;
using SylviaNG.Prescription.Application.Common.Exceptions;
using SylviaNG.Prescription.Application.Features.Patients.Models;
using SylviaNG.Prescription.Application.Interfaces.Repositories;
using SylviaNG.Prescription.Application.Mappings;
using SylviaNG.Prescription.SharedKernel.Generic;

namespace SylviaNG.Prescription.Application.Features.Patients.Commands.UpdatePatient
{
    public class UpdatePatientHandler : IRequestHandler<UpdatePatientCommand, PatientSummaryResponse>
    {
        private readonly IPatientRepository _patientRepository;
        private readonly IUserRepository _userRepository;
        private readonly IStaffRepository _staffRepository;
        private readonly IDoctorRepository _doctorRepository;
        private readonly IUnitOfWork _unitOfWork;

        public UpdatePatientHandler(
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

        public async Task<PatientSummaryResponse> Handle(UpdatePatientCommand command, CancellationToken cancellationToken)
        {
            var patient = await _patientRepository.GetByIdAsync(command.PatientId)
                ?? throw new NotFoundException("Patient", command.PatientId);

            var caller = await CallerContextResolver.ResolveCallerAsync(
                command.KeycloakId, _userRepository, _staffRepository, _doctorRepository);

            // Out-of-scope is reported the same way as a non-existent id (404, not 403) —
            // this codebase has no dedicated forbidden-access exception type, and not
            // distinguishing the two avoids leaking whether a given patient id exists at all.
            var isVisible = await PatientVisibilityScope.IsVisibleAsync(patient, _unitOfWork.Context, caller, cancellationToken);
            if (!isVisible)
                throw new NotFoundException("Patient", command.PatientId);

            var request = command.Request;
            patient.Name = request.Name;
            patient.Phone = request.Phone;
            patient.DateOfBirth = request.DateOfBirth;
            patient.Age = request.Age;
            patient.Gender = request.Gender;
            patient.Address = request.Address;
            patient.BloodGroup = request.BloodGroup;
            patient.AllergyPresetId = request.AllergyPresetId;
            patient.AllergyOtherText = request.AllergyOtherText;
            patient.SavedHistory = request.SavedHistory;

            _patientRepository.Update(patient);
            await _unitOfWork.SaveChangesAsync();

            var registeredByStaff = await _staffRepository.GetByIdAsync(patient.RegisteredByStaffId);
            return patient.ToSummaryResponse(registeredByStaff?.FullName ?? string.Empty);
        }
    }
}
