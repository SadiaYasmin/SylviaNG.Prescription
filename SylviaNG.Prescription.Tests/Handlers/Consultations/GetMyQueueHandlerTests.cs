using FluentAssertions;
using MockQueryable;
using Moq;
using SylviaNG.Prescription.Application.Features.Consultations.Queries.GetMyQueue;
using SylviaNG.Prescription.Application.Interfaces.Repositories;
using SylviaNG.Prescription.Domain.Entities;
using SylviaNG.Prescription.Domain.Enums;
using SylviaNG.Prescription.SharedKernel.Utils;

namespace SylviaNG.Prescription.Tests.Handlers.Consultations;

public class GetMyQueueHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IStaffRepository> _staffRepositoryMock = new();
    private readonly Mock<IDoctorRepository> _doctorRepositoryMock = new();
    private readonly Mock<IPatientRepository> _patientRepositoryMock = new();
    private readonly Mock<IConsultationRepository> _consultationRepositoryMock = new();
    private readonly GetMyQueueHandler _handler;

    private static readonly DateOnly Today = DateTimeUtility.TodayLocal();
    private static readonly DateOnly Yesterday = Today.AddDays(-1);

    public GetMyQueueHandlerTests()
    {
        _userRepositoryMock.Setup(r => r.GetByKeycloakIdAsync("kc-staff-3")).ReturnsAsync(
            new User { UserId = 5, KeycloakId = "kc-staff-3", Role = UserRoleEnum.Staff, IsActive = true, Username = "amina" });
        _staffRepositoryMock.Setup(r => r.GetByUserIdAsync(5)).ReturnsAsync(new Staff { StaffId = 3, UserId = 5, FullName = "Amina Karim" });

        _patientRepositoryMock.Setup(r => r.Query(It.IsAny<bool>())).Returns(new List<Patient>
        {
            new() { PatientId = 1, Name = "Alice Ahmed" },
            new() { PatientId = 2, Name = "Bilal Rahman" },
        }.BuildMock());

        _doctorRepositoryMock.Setup(r => r.Query(It.IsAny<bool>())).Returns(new List<Doctor>
        {
            new() { DoctorId = 10, FullName = "Dr. Ten" },
            new() { DoctorId = 20, FullName = "Dr. Twenty" },
        }.BuildMock());

        _handler = new GetMyQueueHandler(
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
    public async Task Handle_ShouldReturnOnlyTodaysQueuedConsultations_RegisteredByCallingStaff_WithPerRowDoctorName()
    {
        // Arrange: staff 3 registered consultations with two different doctors today.
        SetUpConsultations(
            new Consultation { ConsultationId = 1, RegisteredByStaffId = 3, DoctorId = 10, PatientId = 1, VisitDate = Today, Status = ConsultationStatusEnum.Waiting, CheckInAt = new DateTime(2026, 8, 11, 9, 0, 0, DateTimeKind.Utc), DisplayCode = "CN-1", TokenNumber = "T-01" },
            new Consultation { ConsultationId = 2, RegisteredByStaffId = 3, DoctorId = 20, PatientId = 2, VisitDate = Today, Status = ConsultationStatusEnum.Waiting, CheckInAt = new DateTime(2026, 8, 11, 8, 0, 0, DateTimeKind.Utc), DisplayCode = "CN-2", TokenNumber = "T-01" },
            new Consultation { ConsultationId = 3, RegisteredByStaffId = 4, DoctorId = 10, PatientId = 1, VisitDate = Today, Status = ConsultationStatusEnum.Waiting, CheckInAt = new DateTime(2026, 8, 11, 7, 0, 0, DateTimeKind.Utc), DisplayCode = "CN-3", TokenNumber = "T-02" }, // excluded: different staff
            new Consultation { ConsultationId = 4, RegisteredByStaffId = 3, DoctorId = 10, PatientId = 1, VisitDate = Yesterday, Status = ConsultationStatusEnum.Waiting, CheckInAt = new DateTime(2026, 8, 10, 9, 0, 0, DateTimeKind.Utc), DisplayCode = "CN-4", TokenNumber = "T-03" }, // excluded: not today
            new Consultation { ConsultationId = 5, RegisteredByStaffId = 3, DoctorId = 10, PatientId = 1, VisitDate = Today, Status = ConsultationStatusEnum.Completed, CheckInAt = new DateTime(2026, 8, 11, 6, 0, 0, DateTimeKind.Utc), DisplayCode = "CN-5", TokenNumber = "T-04" }); // excluded: Completed

        // Act
        var result = await _handler.Handle(new GetMyQueueQuery("kc-staff-3"), default);

        // Assert
        result.Should().HaveCount(2);
        result.Select(x => x.ConsultationId).Should().ContainInOrder(2L, 1L); // ordered by CheckInAt ascending
        result.Single(x => x.ConsultationId == 1).DoctorName.Should().Be("Dr. Ten");
        result.Single(x => x.ConsultationId == 2).DoctorName.Should().Be("Dr. Twenty");
    }

    [Fact]
    public async Task Handle_WhenNoQueuedConsultations_ShouldReturnEmpty()
    {
        // Arrange
        SetUpConsultations();

        // Act
        var result = await _handler.Handle(new GetMyQueueQuery("kc-staff-3"), default);

        // Assert
        result.Should().BeEmpty();
    }
}
