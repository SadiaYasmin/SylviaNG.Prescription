using MediatR;
using Microsoft.EntityFrameworkCore;
using SylviaNG.Prescription.Application.Features.Analytics.Models;
using SylviaNG.Prescription.Application.Interfaces.Repositories;
using SylviaNG.Prescription.Domain.Enums;

namespace SylviaNG.Prescription.Application.Features.Analytics.Queries.GetExecutiveSummary
{
    /// <summary>
    /// US-076. Current/previous month are UTC calendar months (everything in this codebase
    /// is stored UTC via <c>UtcDateTimeInterceptor</c>) — <c>AddMonths(-1)</c> correctly
    /// rolls January back into December of the prior year. <see cref="ExecutiveSummaryResponse.TotalMedicines"/>
    /// is the active catalog row count (consistent with the catalog-count convention used
    /// elsewhere in the app), not "distinct medicines ever prescribed".
    /// </summary>
    public class GetExecutiveSummaryHandler : IRequestHandler<GetExecutiveSummaryQuery, ExecutiveSummaryResponse>
    {
        private readonly IPatientRepository _patientRepository;
        private readonly IPrescriptionRepository _prescriptionRepository;
        private readonly IMedicineRepository _medicineRepository;
        private readonly IDoctorRepository _doctorRepository;
        private readonly IStaffRepository _staffRepository;

        public GetExecutiveSummaryHandler(
            IPatientRepository patientRepository,
            IPrescriptionRepository prescriptionRepository,
            IMedicineRepository medicineRepository,
            IDoctorRepository doctorRepository,
            IStaffRepository staffRepository)
        {
            _patientRepository = patientRepository;
            _prescriptionRepository = prescriptionRepository;
            _medicineRepository = medicineRepository;
            _doctorRepository = doctorRepository;
            _staffRepository = staffRepository;
        }

        public async Task<ExecutiveSummaryResponse> Handle(GetExecutiveSummaryQuery query, CancellationToken cancellationToken)
        {
            var now = DateTime.UtcNow;
            var currentMonthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var previousMonthStart = currentMonthStart.AddMonths(-1);
            var nextMonthStart = currentMonthStart.AddMonths(1);

            var patients = await _patientRepository.Query().ToListAsync(cancellationToken);
            var doctors = await _doctorRepository.Query().ToListAsync(cancellationToken);
            var totalMedicines = await _medicineRepository.Query().CountAsync(cancellationToken);
            var totalStaff = await _staffRepository.Query().CountAsync(cancellationToken);
            var finalized = await _prescriptionRepository.Query()
                .Where(p => p.Status == PrescriptionStatusEnum.Finalized)
                .ToListAsync(cancellationToken);

            var rxCurrent = finalized.Count(p => p.FinalizedAt >= currentMonthStart && p.FinalizedAt < nextMonthStart);
            var rxPrevious = finalized.Count(p => p.FinalizedAt >= previousMonthStart && p.FinalizedAt < currentMonthStart);

            var patCurrent = patients.Count(p => p.RegisteredAt >= currentMonthStart && p.RegisteredAt < nextMonthStart);
            var patPrevious = patients.Count(p => p.RegisteredAt >= previousMonthStart && p.RegisteredAt < currentMonthStart);

            var aggregation = MedicinePrescribingAggregator.Aggregate(finalized);
            var topMedicines = aggregation.CountsByKey
                .OrderByDescending(kvp => kvp.Value)
                .Take(5)
                .Select(kvp => new MedicineCountEntry { Name = aggregation.LabelByKey[kvp.Key], Count = kvp.Value })
                .ToList();

            var topDiagnoses = AnalyticsDiagnosisAggregator.TopDiagnoses(finalized, 5);

            var doctorNames = doctors.ToDictionary(d => d.DoctorId, d => d.FullName);
            var topActiveDoctors = finalized
                .GroupBy(p => p.DoctorId)
                .Select(g => new DoctorCountEntry
                {
                    DoctorId = g.Key,
                    FullName = doctorNames.GetValueOrDefault(g.Key, string.Empty),
                    Count = g.Count()
                })
                .OrderByDescending(e => e.Count)
                .Take(5)
                .ToList();

            return new ExecutiveSummaryResponse
            {
                TotalPatients = patients.Count,
                TotalPrescriptions = finalized.Count,
                TotalMedicines = totalMedicines,
                TotalDoctors = doctors.Count,
                TotalStaff = totalStaff,
                PrescriptionTrend = new MonthOverMonthMetric
                {
                    Current = rxCurrent,
                    Previous = rxPrevious,
                    PercentChange = AnalyticsMath.PercentChange(rxCurrent, rxPrevious)
                },
                NewPatientTrend = new MonthOverMonthMetric
                {
                    Current = patCurrent,
                    Previous = patPrevious,
                    PercentChange = AnalyticsMath.PercentChange(patCurrent, patPrevious)
                },
                TopMedicines = topMedicines,
                TopDiagnoses = topDiagnoses,
                TopActiveDoctors = topActiveDoctors
            };
        }
    }
}
