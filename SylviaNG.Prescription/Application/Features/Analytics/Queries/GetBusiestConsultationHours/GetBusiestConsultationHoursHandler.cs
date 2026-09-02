using MediatR;
using Microsoft.EntityFrameworkCore;
using SylviaNG.Prescription.Application.Features.Analytics.Models;
using SylviaNG.Prescription.Application.Interfaces.Repositories;

namespace SylviaNG.Prescription.Application.Features.Analytics.Queries.GetBusiestConsultationHours
{
    /// <summary>
    /// US-073's hospital-wide busiest-hours histogram — aggregates every doctor's
    /// consultations (no per-doctor filter, unlike <c>GetDoctorDetailsHandler</c>) and
    /// converts each <c>CheckInAt</c> (stored UTC) to Bangladesh Time (UTC+6, no DST) before
    /// bucketing, since the hospital operates in Dhaka.
    /// </summary>
    public class GetBusiestConsultationHoursHandler : IRequestHandler<GetBusiestConsultationHoursQuery, BusiestConsultationHoursResponse>
    {
        private const int BangladeshUtcOffsetHours = 6;

        private readonly IConsultationRepository _consultationRepository;

        public GetBusiestConsultationHoursHandler(IConsultationRepository consultationRepository)
        {
            _consultationRepository = consultationRepository;
        }

        public async Task<BusiestConsultationHoursResponse> Handle(GetBusiestConsultationHoursQuery query, CancellationToken cancellationToken)
        {
            var from = DateTime.SpecifyKind(query.From, DateTimeKind.Utc);
            var to = DateTime.SpecifyKind(query.To, DateTimeKind.Utc);

            var consultations = (await _consultationRepository.Query().ToListAsync(cancellationToken))
                .Where(c => c.CheckInAt >= from && c.CheckInAt < to)
                .ToList();

            var countsByBdtHour = consultations
                .GroupBy(c => (c.CheckInAt.Hour + BangladeshUtcOffsetHours) % 24)
                .ToDictionary(g => g.Key, g => g.Count());

            var hours = Enumerable.Range(0, 24)
                .Select(hour => new HourBucket { Hour = hour, Count = countsByBdtHour.GetValueOrDefault(hour) })
                .ToList();

            return new BusiestConsultationHoursResponse { Hours = hours };
        }
    }
}
