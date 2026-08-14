using MediatR;
using SylviaNG.Prescription.Application.Common.Exceptions;
using SylviaNG.Prescription.Application.Features.Consultations.Models;
using SylviaNG.Prescription.Application.Interfaces.Repositories;
using SylviaNG.Prescription.Application.Mappings;

namespace SylviaNG.Prescription.Application.Features.Consultations.Queries.GetConsultationDetails
{
    /// <summary>Admin-only single-consultation details for a details modal.</summary>
    public class GetConsultationDetailsHandler : IRequestHandler<GetConsultationDetailsQuery, ConsultationDetailsResponse>
    {
        private readonly IConsultationRepository _consultationRepository;
        private readonly IPatientRepository _patientRepository;
        private readonly IDoctorRepository _doctorRepository;
        private readonly IStaffRepository _staffRepository;

        public GetConsultationDetailsHandler(
            IConsultationRepository consultationRepository,
            IPatientRepository patientRepository,
            IDoctorRepository doctorRepository,
            IStaffRepository staffRepository)
        {
            _consultationRepository = consultationRepository;
            _patientRepository = patientRepository;
            _doctorRepository = doctorRepository;
            _staffRepository = staffRepository;
        }

        public async Task<ConsultationDetailsResponse> Handle(GetConsultationDetailsQuery query, CancellationToken cancellationToken)
        {
            var consultation = await _consultationRepository.GetByIdAsync(query.ConsultationId)
                ?? throw new NotFoundException("Consultation", query.ConsultationId);

            var patient = await _patientRepository.GetByIdAsync(consultation.PatientId);
            var doctor = await _doctorRepository.GetByIdAsync(consultation.DoctorId);

            // Null for a quick-create walk-in (Epic D) — no staff check-in involved.
            var staff = consultation.RegisteredByStaffId.HasValue
                ? await _staffRepository.GetByIdAsync(consultation.RegisteredByStaffId.Value)
                : null;

            return consultation.ToDetailsResponse(
                patient?.Name ?? string.Empty,
                patient?.Phone ?? string.Empty,
                doctor?.FullName ?? string.Empty,
                staff?.FullName ?? "Quick Create (Doctor)");
        }
    }
}
