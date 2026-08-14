using FluentAssertions;
using MockQueryable;
using Moq;
using SylviaNG.Prescription.Application.Features.Prescriptions.Queries.GetMyDraftPrescriptions;
using SylviaNG.Prescription.Application.Interfaces.Repositories;
using SylviaNG.Prescription.Domain.Entities;
using SylviaNG.Prescription.Domain.Enums;
using SylviaNG.Prescription.SharedKernel.Utils;
using SylviaNG.Prescription.Tests.TestHelpers;

namespace SylviaNG.Prescription.Tests.Handlers.Prescriptions;

public class GetMyDraftPrescriptionsHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IStaffRepository> _staffRepositoryMock = new();
    private readonly Mock<IDoctorRepository> _doctorRepositoryMock = new();
    private readonly Mock<IPatientRepository> _patientRepositoryMock = new();
    private readonly Mock<IPrescriptionRepository> _prescriptionRepositoryMock = new();
    private readonly GetMyDraftPrescriptionsHandler _handler;

    private const long DoctorId = 10;
    private static readonly DateOnly Today = DateTimeUtility.TodayLocal();
    private static readonly DateTime SavedTodayUtc = DateTimeUtility.StartOfDayUtc(Today).AddHours(2);
    private static readonly DateTime SavedYesterdayUtc = DateTimeUtility.StartOfDayUtc(Today.AddDays(-1)).AddHours(2);

    private readonly List<Patient> _patients = new()
    {
        new Patient { PatientId = 1, Name = "Rahim Uddin", Phone = "01711111111", RegisteredByStaffId = 3 },
        new Patient { PatientId = 2, Name = "Karim Sheikh", Phone = "01722222222", RegisteredByStaffId = 3 },
    };

    private readonly List<PrescriptionRecord> _prescriptions = new()
    {
        new PrescriptionRecord { PrescriptionId = 100, DisplayCode = "RX-2026-0100", ConsultationId = 900, PatientId = 1, DoctorId = DoctorId, Status = PrescriptionStatusEnum.Draft, SavedAt = SavedTodayUtc },
        new PrescriptionRecord { PrescriptionId = 101, DisplayCode = "RX-2026-0101", ConsultationId = 901, PatientId = 2, DoctorId = DoctorId, Status = PrescriptionStatusEnum.Draft, SavedAt = SavedYesterdayUtc },
        new PrescriptionRecord { PrescriptionId = 102, DisplayCode = "RX-2026-0102", ConsultationId = 902, PatientId = 1, DoctorId = DoctorId, Status = PrescriptionStatusEnum.Finalized, SavedAt = SavedTodayUtc, FinalizedAt = SavedTodayUtc },
        new PrescriptionRecord { PrescriptionId = 103, DisplayCode = "RX-2026-0103", ConsultationId = 903, PatientId = 1, DoctorId = 999, Status = PrescriptionStatusEnum.Draft, SavedAt = SavedTodayUtc },
    };

    public GetMyDraftPrescriptionsHandlerTests()
    {
        _patientRepositoryMock.Setup(r => r.Query(It.IsAny<bool>())).Returns(_patients.BuildMock());
        _prescriptionRepositoryMock.Setup(r => r.Query(It.IsAny<bool>())).Returns(_prescriptions.BuildMock());

        _userRepositoryMock.Setup(r => r.GetByKeycloakIdAsync("kc-doctor-10")).ReturnsAsync(
            new User { UserId = 15, KeycloakId = "kc-doctor-10", Role = UserRoleEnum.Doctor, IsActive = true, Username = "dr.ten" });
        _doctorRepositoryMock.Setup(r => r.GetByUserIdAsync(15)).ReturnsAsync(new Doctor { DoctorId = DoctorId, UserId = 15, FullName = "Dr. Ten" });
        _doctorRepositoryMock.Setup(r => r.GetByIdAsync(DoctorId)).ReturnsAsync(new Doctor { DoctorId = DoctorId, UserId = 15, FullName = "Dr. Ten" });

        _handler = new GetMyDraftPrescriptionsHandler(
            _userRepositoryMock.Object, _staffRepositoryMock.Object, _doctorRepositoryMock.Object,
            _patientRepositoryMock.Object, _prescriptionRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldOnlyReturnThisDoctorsOwnDrafts()
    {
        // Act
        var result = await _handler.Handle(new GetMyDraftPrescriptionsQuery("kc-doctor-10", null), default);

        // Assert: prescription 102 (Finalized) and 103 (another doctor) excluded.
        result.Prescriptions.Select(p => p.PrescriptionId).Should().BeEquivalentTo(new long[] { 100, 101 });
        result.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task Handle_WithSearchTerm_ShouldMatchPatientNameRxCodeOrPhone()
    {
        // Act — matches patient 2's phone, not name.
        var result = await _handler.Handle(new GetMyDraftPrescriptionsQuery("kc-doctor-10", null, searchTerm: "01722222222"), default);

        // Assert
        result.Prescriptions.Should().ContainSingle(p => p.PrescriptionId == 101);
    }

    [Fact]
    public async Task Handle_WithDate_ShouldOnlyReturnDraftsSavedOnThatDate()
    {
        // Act
        var result = await _handler.Handle(new GetMyDraftPrescriptionsQuery("kc-doctor-10", null, date: Today), default);

        // Assert
        result.Prescriptions.Should().ContainSingle(p => p.PrescriptionId == 100);
    }

    [Fact]
    public async Task Handle_ShouldSupportPaging()
    {
        // Act
        var result = await _handler.Handle(new GetMyDraftPrescriptionsQuery("kc-doctor-10", null, page: 1, pageSize: 1), default);

        // Assert
        result.Prescriptions.Should().HaveCount(1);
        result.TotalCount.Should().Be(2);
        result.PageSize.Should().Be(1);
    }
}
