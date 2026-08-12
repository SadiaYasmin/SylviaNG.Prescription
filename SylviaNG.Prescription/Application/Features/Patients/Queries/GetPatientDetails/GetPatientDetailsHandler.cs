using MediatR;
using SylviaNG.Prescription.Application.Common;
using SylviaNG.Prescription.Application.Common.Exceptions;
using SylviaNG.Prescription.Application.Features.Patients.Models;
using SylviaNG.Prescription.Application.Interfaces.Repositories;
using SylviaNG.Prescription.Application.Mappings;
using SylviaNG.Prescription.SharedKernel.Generic;

namespace SylviaNG.Prescription.Application.Features.Patients.Queries.GetPatientDetails
{
    public class GetPatientDetailsHandler : IRequestHandler<GetPatientDetailsQuery, PatientDetailsResponse>
    {
        private readonly IPatientRepository _patientRepository;
        private readonly IUserRepository _userRepository;
        private readonly IStaffRepository _staffRepository;
        private readonly IDoctorRepository _doctorRepository;
        private readonly IUnitOfWork _unitOfWork;

        public GetPatientDetailsHandler(
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

        public async Task<PatientDetailsResponse> Handle(GetPatientDetailsQuery query, CancellationToken cancellationToken)
        {
            var patient = await _patientRepository.GetByIdAsync(query.PatientId)
                ?? throw new NotFoundException("Patient", query.PatientId);

            var caller = await CallerContextResolver.ResolveCallerAsync(
                query.KeycloakId, _userRepository, _staffRepository, _doctorRepository);

            var isVisible = await PatientVisibilityScope.IsVisibleAsync(patient, _unitOfWork.Context, caller, cancellationToken);
            if (!isVisible)
                throw new NotFoundException("Patient", query.PatientId);

            var registeredByStaff = await _staffRepository.GetByIdAsync(patient.RegisteredByStaffId);
            return patient.ToDetailsResponse(registeredByStaff?.FullName ?? string.Empty);
        }
    }
}
