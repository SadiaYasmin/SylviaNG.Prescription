using MediatR;
using Microsoft.EntityFrameworkCore;
using SylviaNG.Prescription.Application.Features.Analytics.Models;
using SylviaNG.Prescription.Application.Interfaces.Repositories;
using SylviaNG.Prescription.Domain.Enums;

namespace SylviaNG.Prescription.Application.Features.Analytics.Queries.GetExecutiveSummary
{
    /// <summary>
    /// US-076. Current/previous are the caller-selected date range vs. the immediately
    /// preceding period of the same duration (see <see cref="AnalyticsDateBucketing.ResolvePreviousPeriod"/>).
    /// Master counts (TotalPatients/TotalDoctors/TotalStaff) and <see cref="ExecutiveSummaryResponse.TotalMedicines"/>
    /// (the active catalog row count, consistent with the catalog-count convention used
    /// elsewhere in the app — not "distinct medicines ever prescribed") are always hospital-wide/all-time,
    /// unaffected by the query's date range.
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
            var from = DateTime.SpecifyKind(query.From, DateTimeKind.Utc);
            var to = DateTime.SpecifyKind(query.To, DateTimeKind.Utc);
            var (previousFrom, previousTo) = AnalyticsDateBucketing.ResolvePreviousPeriod(from, to);

            var patients = await _patientRepository.Query().ToListAsync(cancellationToken);
            var doctors = await _doctorRepository.Query().ToListAsync(cancellationToken);
            var totalMedicines = await _medicineRepository.Query().CountAsync(cancellationToken);
            var totalStaff = await _staffRepository.Query().CountAsync(cancellationToken);
            var allFinalized = await _prescriptionRepository.Query()
                .Where(p => p.Status == PrescriptionStatusEnum.Finalized)
                .ToListAsync(cancellationToken);

            var finalized = allFinalized.Where(p => p.FinalizedAt >= from && p.FinalizedAt < to).ToList();

            var rxCurrent = finalized.Count;
            var rxPrevious = allFinalized.Count(p => p.FinalizedAt >= previousFrom && p.FinalizedAt < previousTo);

            var patCurrent = patients.Count(p => p.RegisteredAt >= from && p.RegisteredAt < to);
            var patPrevious = patients.Count(p => p.RegisteredAt >= previousFrom && p.RegisteredAt < previousTo);

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
                TotalPrescriptions = rxCurrent,
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
