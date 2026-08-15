using FluentAssertions;
using MockQueryable;
using Moq;
using SylviaNG.Prescription.Application.Features.Analytics;
using SylviaNG.Prescription.Application.Features.Analytics.Queries.GetPrescriptionVolumeTrend;
using SylviaNG.Prescription.Application.Interfaces.Repositories;
using SylviaNG.Prescription.Domain.Entities;
using SylviaNG.Prescription.Domain.Enums;

namespace SylviaNG.Prescription.Tests.Handlers.Analytics;

public class GetPrescriptionVolumeTrendHandlerTests
{
    private readonly Mock<IPrescriptionRepository> _prescriptionRepositoryMock = new();
    private readonly GetPrescriptionVolumeTrendHandler _handler;

    public GetPrescriptionVolumeTrendHandlerTests()
    {
        _prescriptionRepositoryMock.Setup(r => r.Query(It.IsAny<bool>())).Returns(new List<PrescriptionRecord>
        {
            new() { Status = PrescriptionStatusEnum.Finalized, FinalizedAt = new DateTime(2026, 1, 1) },
            new() { Status = PrescriptionStatusEnum.Finalized, FinalizedAt = new DateTime(2026, 1, 15) },
            new() { Status = PrescriptionStatusEnum.Draft, FinalizedAt = null },
        }.BuildMock());

        _handler = new GetPrescriptionVolumeTrendHandler(_prescriptionRepositoryMock.Object);
    }

    [Theory]
    [InlineData(AnalyticsGranularity.Day)]
    [InlineData(AnalyticsGranularity.Week)]
    [InlineData(AnalyticsGranularity.Month)]
    public async Task Handle_ShouldDelegateToDateBucketingAndEchoRequestedGranularity(AnalyticsGranularity granularity)
    {
        var result = await _handler.Handle(new GetPrescriptionVolumeTrendQuery(granularity), default);

        result.Granularity.Should().Be(granularity);
        result.Points.Sum(p => p.Count).Should().Be(2); // draft excluded
    }

    [Fact]
    public async Task Handle_Month_ShouldBucketBothFinalizedDatesIntoOnePoint()
    {
        var result = await _handler.Handle(new GetPrescriptionVolumeTrendQuery(AnalyticsGranularity.Month), default);

        result.Points.Should().ContainSingle();
        result.Points[0].BucketKey.Should().Be("2026-01");
        result.Points[0].Count.Should().Be(2);
    }
}
