using MediatR;
using SylviaNG.Prescription.Application.Common;
using SylviaNG.Prescription.Application.Common.Exceptions;
using SylviaNG.Prescription.Application.Features.Consultations.Models;
using SylviaNG.Prescription.Application.Interfaces.Repositories;
using SylviaNG.Prescription.Application.Mappings;
using SylviaNG.Prescription.Domain.Enums;
using SylviaNG.Prescription.SharedKernel.Generic;

namespace SylviaNG.Prescription.Application.Features.Consultations.Commands.OpenConsultation
{
    /// <summary>
    /// A doctor opening a queued consultation (Waiting -> InConsultation). Only Doctor can
    /// hit this (enforced by [Authorize] on the controller).
    /// </summary>
    public class OpenConsultationHandler : IRequestHandler<OpenConsultationCommand, OpenConsultationResponse>
    {
        private readonly IUserRepository _userRepository;
        private readonly IStaffRepository _staffRepository;
        private readonly IDoctorRepository _doctorRepository;
        private readonly IPatientRepository _patientRepository;
        private readonly IConsultationRepository _consultationRepository;
        private readonly IUnitOfWork _unitOfWork;

        public OpenConsultationHandler(
            IUserRepository userRepository,
            IStaffRepository staffRepository,
            IDoctorRepository doctorRepository,
            IPatientRepository patientRepository,
            IConsultationRepository consultationRepository,
            IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _staffRepository = staffRepository;
            _doctorRepository = doctorRepository;
            _patientRepository = patientRepository;
            _consultationRepository = consultationRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<OpenConsultationResponse> Handle(OpenConsultationCommand command, CancellationToken cancellationToken)
        {
            var consultation = await _consultationRepository.GetByIdAsync(command.ConsultationId)
                ?? throw new NotFoundException("Consultation", command.ConsultationId);

            var caller = await CallerContextResolver.ResolveCallerAsync(
                command.KeycloakId, _userRepository, _staffRepository, _doctorRepository);
            var doctorId = caller.DoctorId!.Value;

            // Out-of-scope is reported the same way as a non-existent id (404, not 403) —
            // same reasoning as Patient's visibility scope: this codebase has no dedicated
            // forbidden-access exception type, and not distinguishing the two avoids leaking
            // whether a given consultation id exists at all to a doctor it isn't queued for.
            if (consultation.DoctorId != doctorId)
                throw new NotFoundException("Consultation", command.ConsultationId);

            if (consultation.Status != ConsultationStatusEnum.Waiting)
                throw new BadRequestException($"Consultation is already {consultation.Status}.");

            consultation.Status = ConsultationStatusEnum.InConsultation;
            _consultationRepository.Update(consultation);
            await _unitOfWork.SaveChangesAsync();

            var patient = await _patientRepository.GetByIdAsync(consultation.PatientId);
            var doctor = await _doctorRepository.GetByIdAsync(consultation.DoctorId);

            return consultation.ToOpenResponse(patient?.Name ?? string.Empty, doctor?.FullName ?? string.Empty);
        }
    }
}
