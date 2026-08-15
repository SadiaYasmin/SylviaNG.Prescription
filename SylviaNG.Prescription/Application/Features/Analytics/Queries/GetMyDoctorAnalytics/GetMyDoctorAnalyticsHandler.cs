using MediatR;
using Microsoft.EntityFrameworkCore;
using SylviaNG.Prescription.Application.Common;
using SylviaNG.Prescription.Application.Features.Analytics.Models;
using SylviaNG.Prescription.Application.Features.Patients;
using SylviaNG.Prescription.Application.Interfaces.Repositories;
using SylviaNG.Prescription.Application.Mappings;
using SylviaNG.Prescription.Domain.Enums;
using SylviaNG.Prescription.SharedKernel.Generic;

namespace SylviaNG.Prescription.Application.Features.Analytics.Queries.GetMyDoctorAnalytics
{
    /// <summary>
    /// US-077. <see cref="PatientVisibilityScope.ApplyAsync"/> is reused verbatim for
    /// <see cref="MyDoctorAnalyticsResponse.OwnPatientCount"/> rather than re-deriving the
    /// StaffDoctor join here — same reasoning every other feature in this codebase follows.
    /// </summary>
    public class GetMyDoctorAnalyticsHandler : IRequestHandler<GetMyDoctorAnalyticsQuery, MyDoctorAnalyticsResponse>
    {
        private readonly IUserRepository _userRepository;
        private readonly IStaffRepository _staffRepository;
        private readonly IDoctorRepository _doctorRepository;
        private readonly IPatientRepository _patientRepository;
        private readonly IConsultationRepository _consultationRepository;
        private readonly IPrescriptionRepository _prescriptionRepository;
        private readonly IUnitOfWork _unitOfWork;

        public GetMyDoctorAnalyticsHandler(
            IUserRepository userRepository,
            IStaffRepository staffRepository,
            IDoctorRepository doctorRepository,
            IPatientRepository patientRepository,
            IConsultationRepository consultationRepository,
            IPrescriptionRepository prescriptionRepository,
            IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _staffRepository = staffRepository;
            _doctorRepository = doctorRepository;
            _patientRepository = patientRepository;
            _consultationRepository = consultationRepository;
            _prescriptionRepository = prescriptionRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<MyDoctorAnalyticsResponse> Handle(GetMyDoctorAnalyticsQuery query, CancellationToken cancellationToken)
        {
            var caller = await CallerContextResolver.ResolveCallerAsync(
                query.KeycloakId, _userRepository, _staffRepository, _doctorRepository);
            var doctorId = caller.DoctorId!.Value;

            var ownPatientsQuery = await PatientVisibilityScope.ApplyAsync(
                _patientRepository.Query(), _unitOfWork.Context, caller, cancellationToken);
            var ownPatientCount = await ownPatientsQuery.CountAsync(cancellationToken);

            var consultations = await _consultationRepository.Query()
                .Where(c => c.DoctorId == doctorId)
                .ToListAsync(cancellationToken);
            var patientsConsulted = consultations.Select(c => c.PatientId).Distinct().Count();

            var myPrescriptions = await _prescriptionRepository.Query()
                .Where(p => p.DoctorId == doctorId)
                .ToListAsync(cancellationToken);
            var draftCount = myPrescriptions.Count(p => p.Status == PrescriptionStatusEnum.Draft);
            var finalizedList = myPrescriptions.Where(p => p.Status == PrescriptionStatusEnum.Finalized).ToList();

            var assignedStaffCount = await _unitOfWork.Context.StaffDoctors
                .CountAsync(sd => sd.DoctorId == doctorId, cancellationToken);

            var aggregation = MedicinePrescribingAggregator.Aggregate(finalizedList);
            var topMedicines = aggregation.CountsByKey
                .OrderByDescending(kvp => kvp.Value)
                .Take(5)
                .Select(kvp => new MedicineCountEntry { Name = aggregation.LabelByKey[kvp.Key], Count = kvp.Value })
                .ToList();

            return new MyDoctorAnalyticsResponse
            {
                OwnPatientCount = ownPatientCount,
                PatientsConsulted = patientsConsulted,
                DraftPrescriptionCount = draftCount,
                FinalizedPrescriptionCount = finalizedList.Count,
                AssignedStaffCount = assignedStaffCount,
                TopMedicines = topMedicines
            };
        }
    }
}
