using FluentAssertions;
using MockQueryable;
using Moq;
using SylviaNG.Prescription.Application.Features.Analytics.Queries.GetPatientAnalytics;
using SylviaNG.Prescription.Application.Features.Prescriptions.Models;
using SylviaNG.Prescription.Application.Interfaces.Repositories;
using SylviaNG.Prescription.Application.Mappings;
using SylviaNG.Prescription.Domain.Entities;
using SylviaNG.Prescription.Domain.Enums;

namespace SylviaNG.Prescription.Tests.Handlers.Analytics;

public class GetPatientAnalyticsHandlerTests
{
    private readonly Mock<IPatientRepository> _patientRepositoryMock = new();
    private readonly Mock<IConsultationRepository> _consultationRepositoryMock = new();
    private readonly Mock<IPrescriptionRepository> _prescriptionRepositoryMock = new();
    private readonly GetPatientAnalyticsHandler _handler;

    public GetPatientAnalyticsHandlerTests()
    {
        _handler = new GetPatientAnalyticsHandler(
            _patientRepositoryMock.Object, _consultationRepositoryMock.Object, _prescriptionRepositoryMock.Object);
    }

    private static PrescriptionRecord FinalizedRxFor(long patientId, params string[] diagnoses)
    {
        var rx = new PrescriptionRecord { PatientId = patientId, Status = PrescriptionStatusEnum.Finalized };
        rx.SetDiagnoses(diagnoses.Select(d => new DiagnosisItem { Text = d }).ToList());
        return rx;
    }

    [Fact]
    public async Task Handle_NewVsReturning_OneVisitIsNewMoreThanOneIsReturning()
    {
        _patientRepositoryMock.Setup(r => r.Query(It.IsAny<bool>())).Returns(new List<Patient>
        {
            new() { PatientId = 1, Name = "Zero Visits" },
            new() { PatientId = 2, Name = "One Visit" },
            new() { PatientId = 3, Name = "Two Visits" },
        }.BuildMock());
        _consultationRepositoryMock.Setup(r => r.Query(It.IsAny<bool>())).Returns(new List<Consultation>
        {
            new() { PatientId = 2 },
            new() { PatientId = 3 },
            new() { PatientId = 3 },
        }.BuildMock());
        _prescriptionRepositoryMock.Setup(r => r.Query(It.IsAny<bool>())).Returns(new List<PrescriptionRecord>().BuildMock());

        var result = await _handler.Handle(new GetPatientAnalyticsQuery(), default);

        result.NewPatients.Should().Be(2); // 0 visits + 1 visit
        result.ReturningPatients.Should().Be(1); // 2 visits
    }

    [Fact]
    public async Task Handle_ChronicDiagnosis_ExactlyTwoOccurrencesQualifiesAsChronic()
    {
        _patientRepositoryMock.Setup(r => r.Query(It.IsAny<bool>())).Returns(new List<Patient>
        {
            new() { PatientId = 1, Name = "Alice" },
        }.BuildMock());
        _consultationRepositoryMock.Setup(r => r.Query(It.IsAny<bool>())).Returns(new List<Consultation>().BuildMock());
        _prescriptionRepositoryMock.Setup(r => r.Query(It.IsAny<bool>())).Returns(new List<PrescriptionRecord>
        {
            FinalizedRxFor(1, "Hypertension"),
            FinalizedRxFor(1, "hypertension"), // case-insensitive match against the first
        }.BuildMock());

        var result = await _handler.Handle(new GetPatientAnalyticsQuery(), default);

        result.ChronicDiagnosisPatterns.Should().ContainSingle();
        result.ChronicDiagnosisPatterns[0].PatientName.Should().Be("Alice");
        result.ChronicDiagnosisPatterns[0].Occurrences.Should().Be(2);
    }

    [Fact]
    public async Task Handle_SingleDiagnosisOccurrence_ShouldNotBeChronic()
    {
        _patientRepositoryMock.Setup(r => r.Query(It.IsAny<bool>())).Returns(new List<Patient>
        {
            new() { PatientId = 1, Name = "Alice" },
        }.BuildMock());
        _consultationRepositoryMock.Setup(r => r.Query(It.IsAny<bool>())).Returns(new List<Consultation>().BuildMock());
        _prescriptionRepositoryMock.Setup(r => r.Query(It.IsAny<bool>())).Returns(new List<PrescriptionRecord>
        {
            FinalizedRxFor(1, "Migraine"),
        }.BuildMock());

        var result = await _handler.Handle(new GetPatientAnalyticsQuery(), default);

        result.ChronicDiagnosisPatterns.Should().BeEmpty();
    }
}
