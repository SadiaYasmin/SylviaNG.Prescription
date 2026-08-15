using FluentAssertions;
using MockQueryable;
using Moq;
using SylviaNG.Prescription.Application.Features.Analytics.Queries.GetDoctorLeaderboard;
using SylviaNG.Prescription.Application.Interfaces.Repositories;
using SylviaNG.Prescription.Domain.Entities;
using SylviaNG.Prescription.Domain.Enums;

namespace SylviaNG.Prescription.Tests.Handlers.Analytics;

public class GetDoctorLeaderboardHandlerTests
{
    private readonly Mock<IDoctorRepository> _doctorRepositoryMock = new();
    private readonly Mock<IConsultationRepository> _consultationRepositoryMock = new();
    private readonly Mock<IPrescriptionRepository> _prescriptionRepositoryMock = new();
    private readonly GetDoctorLeaderboardHandler _handler;

    public GetDoctorLeaderboardHandlerTests()
    {
        _handler = new GetDoctorLeaderboardHandler(
            _doctorRepositoryMock.Object, _consultationRepositoryMock.Object, _prescriptionRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_DoctorWithNoActivity_ShouldStillAppearZeroValued()
    {
        _doctorRepositoryMock.Setup(r => r.Query(It.IsAny<bool>())).Returns(new List<Doctor>
        {
            new() { DoctorId = 1, FullName = "Dr. Active" },
            new() { DoctorId = 2, FullName = "Dr. Idle" },
        }.BuildMock());
        _consultationRepositoryMock.Setup(r => r.Query(It.IsAny<bool>())).Returns(new List<Consultation>
        {
            new() { DoctorId = 1, PatientId = 100, Status = ConsultationStatusEnum.Completed },
        }.BuildMock());
        _prescriptionRepositoryMock.Setup(r => r.Query(It.IsAny<bool>())).Returns(new List<PrescriptionRecord>().BuildMock());

        var result = await _handler.Handle(new GetDoctorLeaderboardQuery(), default);

        result.Should().HaveCount(2);
        var idle = result.Single(d => d.DoctorId == 2);
        idle.PrescriptionsCreated.Should().Be(0);
        idle.PatientsConsulted.Should().Be(0);
        idle.AvgRxPerConsultation.Should().Be(0);
        idle.AvgMedsPerRx.Should().Be(0);
    }

    [Fact]
    public async Task Handle_ShouldOrderByPrescriptionsCreatedDescending()
    {
        _doctorRepositoryMock.Setup(r => r.Query(It.IsAny<bool>())).Returns(new List<Doctor>
        {
            new() { DoctorId = 1, FullName = "Dr. Low" },
            new() { DoctorId = 2, FullName = "Dr. High" },
        }.BuildMock());
        _consultationRepositoryMock.Setup(r => r.Query(It.IsAny<bool>())).Returns(new List<Consultation>().BuildMock());
        _prescriptionRepositoryMock.Setup(r => r.Query(It.IsAny<bool>())).Returns(new List<PrescriptionRecord>
        {
            new() { DoctorId = 1, Status = PrescriptionStatusEnum.Finalized },
            new() { DoctorId = 2, Status = PrescriptionStatusEnum.Finalized },
            new() { DoctorId = 2, Status = PrescriptionStatusEnum.Finalized },
        }.BuildMock());

        var result = await _handler.Handle(new GetDoctorLeaderboardQuery(), default);

        result.Select(d => d.DoctorId).Should().ContainInOrder(2L, 1L);
    }
}
