using FluentAssertions;
using MockQueryable;
using Moq;
using SylviaNG.Prescription.Application.Features.Consultations.Models;
using SylviaNG.Prescription.Application.Features.Consultations.Queries.GetConsultationList;
using SylviaNG.Prescription.Application.Interfaces.Repositories;
using SylviaNG.Prescription.Domain.Entities;
using SylviaNG.Prescription.Domain.Enums;
using SylviaNG.Prescription.SharedKernel.Utils;

namespace SylviaNG.Prescription.Tests.Handlers.Consultations;

public class GetConsultationListHandlerTests
{
    private readonly Mock<IConsultationRepository> _consultationRepositoryMock = new();
    private readonly Mock<IPatientRepository> _patientRepositoryMock = new();
    private readonly Mock<IDoctorRepository> _doctorRepositoryMock = new();
    private readonly GetConsultationListHandler _handler;

    private static readonly DateOnly Today = DateTimeUtility.TodayLocal();
    private static readonly DateOnly Yesterday = Today.AddDays(-1);
    private static readonly DateOnly TwoDaysAgo = Today.AddDays(-2);

    public GetConsultationListHandlerTests()
    {
        _patientRepositoryMock.Setup(r => r.Query(It.IsAny<bool>())).Returns(new List<Patient>
        {
            new() { PatientId = 1, Name = "Alice Ahmed", Phone = "01711111111" },
            new() { PatientId = 2, Name = "Bilal Rahman", Phone = "01722222222" },
        }.BuildMock());

        _doctorRepositoryMock.Setup(r => r.Query(It.IsAny<bool>())).Returns(new List<Doctor>
        {
            new() { DoctorId = 10, FullName = "Dr. Ten" },
            new() { DoctorId = 20, FullName = "Dr. Twenty" },
        }.BuildMock());

        _handler = new GetConsultationListHandler(_consultationRepositoryMock.Object, _patientRepositoryMock.Object, _doctorRepositoryMock.Object);
    }

    private void SetUpConsultations(params Consultation[] consultations)
    {
        _consultationRepositoryMock.Setup(r => r.Query(It.IsAny<bool>())).Returns(consultations.BuildMock());
    }

    private static Consultation Make(long id, long patientId, long doctorId, DateOnly visitDate, ConsultationStatusEnum status, string token = "T-01") => new()
    {
        ConsultationId = id,
        PatientId = patientId,
        DoctorId = doctorId,
        VisitDate = visitDate,
        Status = status,
        DisplayCode = $"CN-{id}",
        TokenNumber = token,
        CheckInAt = DateTime.UtcNow
    };

    [Fact]
    public async Task Handle_WithDefaultTodayMode_ShouldReturnOnlyTodaysConsultations()
    {
        // Arrange
        SetUpConsultations(
            Make(1, 1, 10, Today, ConsultationStatusEnum.Waiting),
            Make(2, 2, 20, Yesterday, ConsultationStatusEnum.Waiting));

        // Act
        var result = await _handler.Handle(new GetConsultationListQuery(new ConsultationListRequest()), default);

        // Assert
        result.Consultations.Should().ContainSingle(c => c.ConsultationId == 1);
        result.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task Handle_WithYesterdayMode_ShouldReturnOnlyYesterdaysConsultations()
    {
        // Arrange
        SetUpConsultations(
            Make(1, 1, 10, Today, ConsultationStatusEnum.Waiting),
            Make(2, 2, 20, Yesterday, ConsultationStatusEnum.Waiting));

        // Act
        var result = await _handler.Handle(new GetConsultationListQuery(
            new ConsultationListRequest { DateMode = ConsultationDateModeEnum.Yesterday }), default);

        // Assert
        result.Consultations.Should().ContainSingle(c => c.ConsultationId == 2);
    }

    [Fact]
    public async Task Handle_WithCustomMode_ShouldFilterByExactDate()
    {
        // Arrange
        SetUpConsultations(
            Make(1, 1, 10, Today, ConsultationStatusEnum.Waiting),
            Make(2, 2, 20, TwoDaysAgo, ConsultationStatusEnum.Waiting));

        // Act
        var result = await _handler.Handle(new GetConsultationListQuery(
            new ConsultationListRequest { DateMode = ConsultationDateModeEnum.Custom, Date = TwoDaysAgo }), default);

        // Assert
        result.Consultations.Should().ContainSingle(c => c.ConsultationId == 2);
    }

    [Fact]
    public async Task Handle_WithRangeMode_ShouldFilterByFromAndToDate()
    {
        // Arrange
        SetUpConsultations(
            Make(1, 1, 10, Today, ConsultationStatusEnum.Waiting),
            Make(2, 2, 20, Yesterday, ConsultationStatusEnum.Waiting),
            Make(3, 1, 10, TwoDaysAgo, ConsultationStatusEnum.Waiting));

        // Act
        var result = await _handler.Handle(new GetConsultationListQuery(
            new ConsultationListRequest { DateMode = ConsultationDateModeEnum.Range, FromDate = Yesterday, ToDate = Today }), default);

        // Assert
        result.Consultations.Select(c => c.ConsultationId).Should().BeEquivalentTo(new[] { 1L, 2L });
    }

    [Fact]
    public async Task Handle_WithDoctorFilter_ShouldReturnOnlyThatDoctorsConsultations()
    {
        // Arrange
        SetUpConsultations(
            Make(1, 1, 10, Today, ConsultationStatusEnum.Waiting),
            Make(2, 2, 20, Today, ConsultationStatusEnum.Waiting));

        // Act
        var result = await _handler.Handle(new GetConsultationListQuery(
            new ConsultationListRequest { DoctorId = 20 }), default);

        // Assert
        result.Consultations.Should().ContainSingle(c => c.ConsultationId == 2 && c.DoctorName == "Dr. Twenty");
    }

    [Fact]
    public async Task Handle_WithStatusFilter_ShouldReturnOnlyThatStatus()
    {
        // Arrange
        SetUpConsultations(
            Make(1, 1, 10, Today, ConsultationStatusEnum.Waiting),
            Make(2, 2, 20, Today, ConsultationStatusEnum.Completed));

        // Act
        var result = await _handler.Handle(new GetConsultationListQuery(
            new ConsultationListRequest { Status = ConsultationStatusEnum.Completed }), default);

        // Assert
        result.Consultations.Should().ContainSingle(c => c.ConsultationId == 2);
    }

    [Theory]
    [InlineData("alice")]
    [InlineData("T-99")]
    [InlineData("01711111111")]
    public async Task Handle_WithSearchTerm_ShouldMatchPatientNameOrTokenOrPhone(string term)
    {
        // Arrange
        SetUpConsultations(
            Make(1, 1, 10, Today, ConsultationStatusEnum.Waiting, token: "T-99"),
            Make(2, 2, 20, Today, ConsultationStatusEnum.Waiting, token: "T-01"));

        // Act
        var result = await _handler.Handle(new GetConsultationListQuery(
            new ConsultationListRequest { SearchTerm = term }), default);

        // Assert
        result.Consultations.Should().ContainSingle(c => c.ConsultationId == 1);
    }

    [Fact]
    public async Task Handle_SummaryCounts_ShouldReflectFilteredSetNotGlobalRoster()
    {
        // Arrange: mix of statuses today, plus an unrelated Waiting consultation yesterday
        // that must NOT be counted once the Today filter is applied.
        SetUpConsultations(
            Make(1, 1, 10, Today, ConsultationStatusEnum.Waiting),
            Make(2, 2, 20, Today, ConsultationStatusEnum.InConsultation),
            Make(3, 1, 10, Today, ConsultationStatusEnum.Completed),
            Make(4, 2, 20, Today, ConsultationStatusEnum.Completed),
            Make(5, 1, 10, Yesterday, ConsultationStatusEnum.Waiting));

        // Act
        var result = await _handler.Handle(new GetConsultationListQuery(new ConsultationListRequest()), default);

        // Assert
        result.Summary.Total.Should().Be(4);
        result.Summary.Waiting.Should().Be(1);
        result.Summary.InProgress.Should().Be(1);
        result.Summary.Completed.Should().Be(2);
    }

    [Fact]
    public async Task Handle_WithStatusFilter_SummaryShouldStillReflectFullDateFilteredBreakdown()
    {
        // Arrange: filtering the table down to Completed must not collapse the summary tiles
        // to just that status — Waiting/InProgress/Draft should still show their true counts.
        SetUpConsultations(
            Make(1, 1, 10, Today, ConsultationStatusEnum.Waiting),
            Make(2, 2, 20, Today, ConsultationStatusEnum.InConsultation),
            Make(3, 1, 10, Today, ConsultationStatusEnum.Completed),
            Make(4, 2, 20, Today, ConsultationStatusEnum.Completed));

        // Act
        var result = await _handler.Handle(new GetConsultationListQuery(
            new ConsultationListRequest { Status = ConsultationStatusEnum.Completed }), default);

        // Assert
        result.Consultations.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
        result.Summary.Total.Should().Be(4);
        result.Summary.Waiting.Should().Be(1);
        result.Summary.InProgress.Should().Be(1);
        result.Summary.Completed.Should().Be(2);
    }

    [Fact]
    public async Task Handle_ShouldPaginateAndOrderNewestFirst()
    {
        // Arrange
        SetUpConsultations(
            Make(1, 1, 10, Today, ConsultationStatusEnum.Waiting),
            Make(2, 2, 20, Today, ConsultationStatusEnum.Waiting),
            Make(3, 1, 10, Today, ConsultationStatusEnum.Waiting));

        // Act
        var result = await _handler.Handle(new GetConsultationListQuery(
            new ConsultationListRequest { Page = 1, PageSize = 2 }), default);

        // Assert
        result.TotalCount.Should().Be(3);
        result.Consultations.Select(c => c.ConsultationId).Should().ContainInOrder(3L, 2L);
    }
}
