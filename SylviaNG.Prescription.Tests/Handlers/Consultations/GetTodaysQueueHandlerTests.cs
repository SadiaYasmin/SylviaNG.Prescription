using FluentAssertions;
using MockQueryable;
using Moq;
using SylviaNG.Prescription.Application.Features.Consultations.Queries.GetTodaysQueue;
using SylviaNG.Prescription.Application.Interfaces.Repositories;
using SylviaNG.Prescription.Domain.Entities;
using SylviaNG.Prescription.Domain.Enums;
using SylviaNG.Prescription.SharedKernel.Utils;

namespace SylviaNG.Prescription.Tests.Handlers.Consultations;

public class GetTodaysQueueHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IStaffRepository> _staffRepositoryMock = new();
    private readonly Mock<IDoctorRepository> _doctorRepositoryMock = new();
    private readonly Mock<IPatientRepository> _patientRepositoryMock = new();
    private readonly Mock<IConsultationRepository> _consultationRepositoryMock = new();
    private readonly GetTodaysQueueHandler _handler;

    private static readonly DateOnly Today = DateTimeUtility.TodayLocal();
    private static readonly DateOnly Yesterday = Today.AddDays(-1);

    public GetTodaysQueueHandlerTests()
    {
        _userRepositoryMock.Setup(r => r.GetByKeycloakIdAsync("kc-doc-10")).ReturnsAsync(
            new User { UserId = 7, KeycloakId = "kc-doc-10", Role = UserRoleEnum.Doctor, IsActive = true, Username = "dr.ten" });
        _doctorRepositoryMock.Setup(r => r.GetByUserIdAsync(7)).ReturnsAsync(new Doctor { DoctorId = 10, UserId = 7, FullName = "Dr. Ten" });
        _doctorRepositoryMock.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(new Doctor { DoctorId = 10, UserId = 7, FullName = "Dr. Ten" });

        _patientRepositoryMock.Setup(r => r.Query(It.IsAny<bool>())).Returns(new List<Patient>
        {
            new() { PatientId = 1, Name = "Alice Ahmed" },
            new() { PatientId = 2, Name = "Bilal Rahman" },
            new() { PatientId = 3, Name = "Chandra Roy" },
        }.BuildMock());

        _handler = new GetTodaysQueueHandler(
            _userRepositoryMock.Object,
            _staffRepositoryMock.Object,
            _doctorRepositoryMock.Object,
            _patientRepositoryMock.Object,
            _consultationRepositoryMock.Object);
    }

    private void SetUpConsultations(params Consultation[] consultations)
    {
        _consultationRepositoryMock.Setup(r => r.Query(It.IsAny<bool>())).Returns(consultations.BuildMock());
    }

    [Fact]
    public async Task Handle_ShouldReturnOnlyTodaysWaitingOrInConsultation_ForCallingDoctor()
    {
        // Arrange
        SetUpConsultations(
            new Consultation { ConsultationId = 1, DoctorId = 10, PatientId = 1, VisitDate = Today, Status = ConsultationStatusEnum.Waiting, CheckInAt = new DateTime(2026, 8, 11, 9, 0, 0, DateTimeKind.Utc), DisplayCode = "CN-1", TokenNumber = "T-01" },
            new Consultation { ConsultationId = 2, DoctorId = 10, PatientId = 2, VisitDate = Today, Status = ConsultationStatusEnum.InConsultation, CheckInAt = new DateTime(2026, 8, 11, 8, 0, 0, DateTimeKind.Utc), DisplayCode = "CN-2", TokenNumber = "T-02" },
            new Consultation { ConsultationId = 3, DoctorId = 10, PatientId = 3, VisitDate = Today, Status = ConsultationStatusEnum.Completed, CheckInAt = new DateTime(2026, 8, 11, 7, 0, 0, DateTimeKind.Utc), DisplayCode = "CN-3", TokenNumber = "T-03" }, // excluded: Completed
            new Consultation { ConsultationId = 4, DoctorId = 10, PatientId = 1, VisitDate = Yesterday, Status = ConsultationStatusEnum.Waiting, CheckInAt = new DateTime(2026, 8, 10, 9, 0, 0, DateTimeKind.Utc), DisplayCode = "CN-4", TokenNumber = "T-01" }, // excluded: not today
            new Consultation { ConsultationId = 5, DoctorId = 20, PatientId = 1, VisitDate = Today, Status = ConsultationStatusEnum.Waiting, CheckInAt = new DateTime(2026, 8, 11, 6, 0, 0, DateTimeKind.Utc), DisplayCode = "CN-5", TokenNumber = "T-01" }); // excluded: different doctor

        // Act
        var result = await _handler.Handle(new GetTodaysQueueQuery("kc-doc-10"), default);

        // Assert
        result.Should().HaveCount(2);
        result.Select(x => x.ConsultationId).Should().ContainInOrder(2L, 1L); // ordered by CheckInAt ascending (2 checked in earlier)
        result.Should().OnlyContain(x => x.DoctorName == "Dr. Ten");
        result.Single(x => x.ConsultationId == 1).PatientName.Should().Be("Alice Ahmed");
    }

    [Fact]
    public async Task Handle_WhenNoQueuedConsultations_ShouldReturnEmpty()
    {
        // Arrange
        SetUpConsultations();

        // Act
        var result = await _handler.Handle(new GetTodaysQueueQuery("kc-doc-10"), default);

        // Assert
        result.Should().BeEmpty();
    }
}
