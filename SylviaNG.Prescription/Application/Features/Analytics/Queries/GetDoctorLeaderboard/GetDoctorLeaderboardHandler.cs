using MediatR;
using Microsoft.EntityFrameworkCore;
using SylviaNG.Prescription.Application.Features.Analytics.Models;
using SylviaNG.Prescription.Application.Interfaces.Repositories;
using SylviaNG.Prescription.Application.Mappings;
using SylviaNG.Prescription.Domain.Entities;
using SylviaNG.Prescription.Domain.Enums;

namespace SylviaNG.Prescription.Application.Features.Analytics.Queries.GetDoctorLeaderboard
{
    /// <summary>
    /// US-073's leaderboard. Every doctor appears — including ones with zero activity — a
    /// true leaderboard, not "doctors who have prescribed something". Three bulk queries
    /// (doctors/consultations/finalized prescriptions), grouped in memory; no per-doctor N+1.
    /// </summary>
    public class GetDoctorLeaderboardHandler : IRequestHandler<GetDoctorLeaderboardQuery, List<DoctorLeaderboardEntry>>
    {
        private readonly IDoctorRepository _doctorRepository;
        private readonly IConsultationRepository _consultationRepository;
        private readonly IPrescriptionRepository _prescriptionRepository;

        public GetDoctorLeaderboardHandler(
            IDoctorRepository doctorRepository,
            IConsultationRepository consultationRepository,
            IPrescriptionRepository prescriptionRepository)
        {
            _doctorRepository = doctorRepository;
            _consultationRepository = consultationRepository;
            _prescriptionRepository = prescriptionRepository;
        }

        public async Task<List<DoctorLeaderboardEntry>> Handle(GetDoctorLeaderboardQuery query, CancellationToken cancellationToken)
        {
            var doctors = await _doctorRepository.Query().ToListAsync(cancellationToken);
            var consultations = await _consultationRepository.Query().ToListAsync(cancellationToken);
            var finalized = await _prescriptionRepository.Query()
                .Where(p => p.Status == PrescriptionStatusEnum.Finalized)
                .ToListAsync(cancellationToken);

            var consultationsByDoctor = consultations.GroupBy(c => c.DoctorId).ToDictionary(g => g.Key, g => g.ToList());
            var finalizedByDoctor = finalized.GroupBy(p => p.DoctorId).ToDictionary(g => g.Key, g => g.ToList());

            return doctors
                .Select(d =>
                {
                    var myConsultations = consultationsByDoctor.GetValueOrDefault(d.DoctorId, new List<Consultation>());
                    var myFinalized = finalizedByDoctor.GetValueOrDefault(d.DoctorId, new List<PrescriptionRecord>());
                    var completedConsultations = myConsultations.Count(c => c.Status == ConsultationStatusEnum.Completed);
                    var medicinesPrescribed = myFinalized.Sum(p => p.GetMedicines().Count);

                    return new DoctorLeaderboardEntry
                    {
                        DoctorId = d.DoctorId,
                        FullName = d.FullName,
                        PatientsConsulted = myConsultations.Select(c => c.PatientId).Distinct().Count(),
                        PrescriptionsCreated = myFinalized.Count,
                        MedicinesPrescribed = medicinesPrescribed,
                        AvgRxPerConsultation = AnalyticsMath.SafeDivide(myFinalized.Count, completedConsultations),
                        AvgMedsPerRx = AnalyticsMath.SafeDivide(medicinesPrescribed, myFinalized.Count)
                    };
                })
                .OrderByDescending(e => e.PrescriptionsCreated)
                .ToList();
        }
    }
}
