using FluentAssertions;
using MockQueryable;
using Moq;
using SylviaNG.Prescription.Application.Features.Analytics.Queries.GetBusiestConsultationHours;
using SylviaNG.Prescription.Application.Interfaces.Repositories;
using SylviaNG.Prescription.Domain.Entities;

namespace SylviaNG.Prescription.Tests.Handlers.Analytics;

public class GetBusiestConsultationHoursHandlerTests
{
    private readonly Mock<IConsultationRepository> _consultationRepositoryMock = new();
    private readonly GetBusiestConsultationHoursHandler _handler;

    public GetBusiestConsultationHoursHandlerTests()
    {
        _handler = new GetBusiestConsultationHoursHandler(_consultationRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WithNoConsultations_ShouldReturnTwentyFourZeroBuckets()
    {
        _consultationRepositoryMock.Setup(r => r.Query(It.IsAny<bool>())).Returns(new List<Consultation>().BuildMock());

        var result = await _handler.Handle(new GetBusiestConsultationHoursQuery(), default);

        result.Hours.Should().HaveCount(24);
        result.Hours.Should().OnlyContain(h => h.Count == 0);
        result.Hours.Select(h => h.Hour).Should().ContainInOrder(Enumerable.Range(0, 24));
    }

    [Fact]
    public async Task Handle_ShouldAggregateAcrossEveryDoctorAndConvertUtcToBangladeshTime()
    {
        _consultationRepositoryMock.Setup(r => r.Query(It.IsAny<bool>())).Returns(new List<Consultation>
        {
            // 03:00 UTC -> 09:00 BDT
            new() { DoctorId = 1, PatientId = 100, CheckInAt = new DateTime(2026, 1, 1, 3, 15, 0, DateTimeKind.Utc) },
            new() { DoctorId = 2, PatientId = 200, CheckInAt = new DateTime(2026, 1, 2, 3, 45, 0, DateTimeKind.Utc) },
            // 20:00 UTC -> wraps past midnight to 02:00 BDT
            new() { DoctorId = 1, PatientId = 300, CheckInAt = new DateTime(2026, 1, 3, 20, 0, 0, DateTimeKind.Utc) },
        }.BuildMock());

        var result = await _handler.Handle(new GetBusiestConsultationHoursQuery(), default);

        result.Hours.Should().HaveCount(24);
        result.Hours.Single(h => h.Hour == 9).Count.Should().Be(2);
        result.Hours.Single(h => h.Hour == 2).Count.Should().Be(1);
        result.Hours.Where(h => h.Hour != 9 && h.Hour != 2).Should().OnlyContain(h => h.Count == 0);
    }
}
