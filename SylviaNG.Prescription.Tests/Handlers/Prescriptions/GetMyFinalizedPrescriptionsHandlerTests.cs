using FluentAssertions;
using MockQueryable;
using Moq;
using SylviaNG.Prescription.Application.Features.Prescriptions.Queries.GetMyFinalizedPrescriptions;
using SylviaNG.Prescription.Application.Interfaces.Repositories;
using SylviaNG.Prescription.Domain.Entities;
using SylviaNG.Prescription.Domain.Enums;
using SylviaNG.Prescription.SharedKernel.Utils;
using SylviaNG.Prescription.Tests.TestHelpers;

namespace SylviaNG.Prescription.Tests.Handlers.Prescriptions;

public class GetMyFinalizedPrescriptionsHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IStaffRepository> _staffRepositoryMock = new();
    private readonly Mock<IDoctorRepository> _doctorRepositoryMock = new();
    private readonly Mock<IPatientRepository> _patientRepositoryMock = new();
    private readonly Mock<IPrescriptionRepository> _prescriptionRepositoryMock = new();
    private readonly GetMyFinalizedPrescriptionsHandler _handler;

    private const long DoctorId = 10;
    private static readonly DateOnly Today = DateTimeUtility.TodayLocal();
    private static readonly DateTime FinalizedTodayUtc = DateTimeUtility.StartOfDayUtc(Today).AddHours(2);
    private static readonly DateTime FinalizedLastWeekUtc = DateTimeUtility.StartOfDayUtc(Today.AddDays(-7)).AddHours(2);

    private readonly List<Patient> _patients = new()
    {
        new Patient { PatientId = 1, Name = "Rahim Uddin", Phone = "01711111111", RegisteredByStaffId = 3 },
        new Patient { PatientId = 2, Name = "Karim Sheikh", Phone = "01722222222", RegisteredByStaffId = 3 },
    };

    private readonly List<PrescriptionRecord> _prescriptions = new()
    {
        new PrescriptionRecord { PrescriptionId = 200, DisplayCode = "RX-2026-0200", ConsultationId = 900, PatientId = 1, DoctorId = DoctorId, Status = PrescriptionStatusEnum.Finalized, FinalizedAt = FinalizedTodayUtc },
        new PrescriptionRecord { PrescriptionId = 201, DisplayCode = "RX-2026-0201", ConsultationId = 901, PatientId = 2, DoctorId = DoctorId, Status = PrescriptionStatusEnum.Finalized, FinalizedAt = FinalizedLastWeekUtc },
        new PrescriptionRecord { PrescriptionId = 202, DisplayCode = "RX-2026-0202", ConsultationId = 902, PatientId = 1, DoctorId = DoctorId, Status = PrescriptionStatusEnum.Draft, SavedAt = FinalizedTodayUtc },
        new PrescriptionRecord { PrescriptionId = 203, DisplayCode = "RX-2026-0203", ConsultationId = 903, PatientId = 1, DoctorId = 999, Status = PrescriptionStatusEnum.Finalized, FinalizedAt = FinalizedTodayUtc },
    };

    public GetMyFinalizedPrescriptionsHandlerTests()
    {
        _patientRepositoryMock.Setup(r => r.Query(It.IsAny<bool>())).Returns(_patients.BuildMock());
        _prescriptionRepositoryMock.Setup(r => r.Query(It.IsAny<bool>())).Returns(_prescriptions.BuildMock());

        _userRepositoryMock.Setup(r => r.GetByKeycloakIdAsync("kc-doctor-10")).ReturnsAsync(
            new User { UserId = 15, KeycloakId = "kc-doctor-10", Role = UserRoleEnum.Doctor, IsActive = true, Username = "dr.ten" });
        _doctorRepositoryMock.Setup(r => r.GetByUserIdAsync(15)).ReturnsAsync(new Doctor { DoctorId = DoctorId, UserId = 15, FullName = "Dr. Ten" });
        _doctorRepositoryMock.Setup(r => r.GetByIdAsync(DoctorId)).ReturnsAsync(new Doctor { DoctorId = DoctorId, UserId = 15, FullName = "Dr. Ten" });

        _handler = new GetMyFinalizedPrescriptionsHandler(
            _userRepositoryMock.Object, _staffRepositoryMock.Object, _doctorRepositoryMock.Object,
            _patientRepositoryMock.Object, _prescriptionRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldOnlyReturnThisDoctorsOwnFinalizedPrescriptions()
    {
        // Act
        var result = await _handler.Handle(new GetMyFinalizedPrescriptionsQuery("kc-doctor-10"), default);

        // Assert: draft 202 and another doctor's 203 excluded.
        result.Prescriptions.Select(p => p.PrescriptionId).Should().BeEquivalentTo(new long[] { 200, 201 });
        result.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task Handle_WithSearchTerm_ShouldMatchPatientNameOrRxCode()
    {
        // Act
        var result = await _handler.Handle(new GetMyFinalizedPrescriptionsQuery("kc-doctor-10", searchTerm: "RX-2026-0201"), default);

        // Assert
        result.Prescriptions.Should().ContainSingle(p => p.PrescriptionId == 201);
    }

    [Fact]
    public async Task Handle_WithFromToDateRange_ShouldOnlyReturnFinalizedWithinRange()
    {
        // Act
        var result = await _handler.Handle(new GetMyFinalizedPrescriptionsQuery("kc-doctor-10", fromDate: Today, toDate: Today), default);

        // Assert
        result.Prescriptions.Should().ContainSingle(p => p.PrescriptionId == 200);
    }

    [Fact]
    public async Task Handle_ShouldOrderNewestFinalizedFirst()
    {
        // Act
        var result = await _handler.Handle(new GetMyFinalizedPrescriptionsQuery("kc-doctor-10"), default);

        // Assert
        result.Prescriptions.Select(p => p.PrescriptionId).Should().ContainInOrder(200L, 201L);
    }
}
