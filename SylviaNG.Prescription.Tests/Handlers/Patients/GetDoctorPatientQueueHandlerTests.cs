using FluentAssertions;
using MockQueryable;
using Moq;
using SylviaNG.Prescription.Application.Features.Patients.Models;
using SylviaNG.Prescription.Application.Features.Patients.Queries.GetDoctorPatientQueue;
using SylviaNG.Prescription.Application.Interfaces.Repositories;
using SylviaNG.Prescription.Domain.Entities;
using SylviaNG.Prescription.Domain.Enums;
using SylviaNG.Prescription.Infrastructure.Data;
using SylviaNG.Prescription.SharedKernel.Generic;
using SylviaNG.Prescription.SharedKernel.Utils;
using SylviaNG.Prescription.Tests.TestHelpers;

namespace SylviaNG.Prescription.Tests.Handlers.Patients;

public class GetDoctorPatientQueueHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IStaffRepository> _staffRepositoryMock = new();
    private readonly Mock<IDoctorRepository> _doctorRepositoryMock = new();
    private readonly Mock<IPatientRepository> _patientRepositoryMock = new();
    private readonly Mock<IConsultationRepository> _consultationRepositoryMock = new();
    private readonly Mock<IPrescriptionRepository> _prescriptionRepositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly ApplicationDBContext _context = InMemoryDbContextFactory.Create();
    private readonly GetDoctorPatientQueueHandler _handler;

    private const long DoctorId = 10;
    private static readonly DateOnly Today = DateTimeUtility.TodayLocal();

    // Patients 1-3 are registered by Staff 3, who is assigned to Doctor 10. Patient 4 is
    // registered by Staff 4, who is NOT assigned to Doctor 10, so it must never appear.
    // Patient 1 is Waiting today; Patient 2 is Completed today (with a finalized
    // prescription); Patient 3 has no consultation record today at all.
    private readonly List<Patient> _patients = new()
    {
        new Patient { PatientId = 1, Name = "Waiting Wendy", Phone = "01711111111", RegisteredByStaffId = 3 },
        new Patient { PatientId = 2, Name = "Completed Cara", Phone = "01722222222", RegisteredByStaffId = 3 },
        new Patient { PatientId = 3, Name = "Fresh Farid", Phone = "01733333333", RegisteredByStaffId = 3 },
        new Patient { PatientId = 4, Name = "Unassigned Uma", Phone = "01744444444", RegisteredByStaffId = 4 },
    };

    private readonly List<Consultation> _consultations = new()
    {
        new Consultation { ConsultationId = 100, PatientId = 1, DoctorId = DoctorId, VisitDate = Today, Status = ConsultationStatusEnum.Waiting, DisplayCode = "CN-1", TokenNumber = "T-01", CheckInAt = DateTime.UtcNow },
        new Consultation { ConsultationId = 101, PatientId = 2, DoctorId = DoctorId, VisitDate = Today, Status = ConsultationStatusEnum.Completed, DisplayCode = "CN-2", TokenNumber = "T-02", CheckInAt = DateTime.UtcNow },
    };

    private readonly List<PrescriptionRecord> _prescriptions = new()
    {
        new PrescriptionRecord { PrescriptionId = 200, ConsultationId = 101, PatientId = 2, DoctorId = DoctorId, DisplayCode = "RX-1", Status = PrescriptionStatusEnum.Finalized },
    };

    public GetDoctorPatientQueueHandlerTests()
    {
        _patientRepositoryMock.Setup(r => r.Query(It.IsAny<bool>())).Returns(_patients.BuildMock());
        _consultationRepositoryMock.Setup(r => r.Query(It.IsAny<bool>())).Returns(_consultations.BuildMock());
        _prescriptionRepositoryMock.Setup(r => r.Query(It.IsAny<bool>())).Returns(_prescriptions.BuildMock());
        _unitOfWorkMock.Setup(u => u.Context).Returns(_context);

        _userRepositoryMock.Setup(r => r.GetByKeycloakIdAsync("kc-doctor-10")).ReturnsAsync(
            new User { UserId = 15, KeycloakId = "kc-doctor-10", Role = UserRoleEnum.Doctor, IsActive = true, Username = "dr.ten" });
        _doctorRepositoryMock.Setup(r => r.GetByUserIdAsync(15)).ReturnsAsync(new Doctor { DoctorId = DoctorId, UserId = 15, FullName = "Dr. Ten" });

        _context.StaffDoctors.Add(new StaffDoctor { StaffDoctorId = 1, StaffId = 3, DoctorId = DoctorId });
        _context.Staff.Add(new Staff { StaffId = 3, UserId = 5, FullName = "Amina Karim", Phone = "01700000000" });
        _context.SaveChanges();

        _handler = new GetDoctorPatientQueueHandler(
            _userRepositoryMock.Object, _staffRepositoryMock.Object, _doctorRepositoryMock.Object,
            _patientRepositoryMock.Object, _consultationRepositoryMock.Object, _prescriptionRepositoryMock.Object,
            _unitOfWorkMock.Object);
    }

    private Task<DoctorPatientQueueResponse> Handle(PatientQueueFilterEnum filter, string? searchTerm = null) =>
        _handler.Handle(new GetDoctorPatientQueueQuery("kc-doctor-10", new DoctorPatientQueueRequest { QueueFilter = filter, SearchTerm = searchTerm }), default);

    [Fact]
    public async Task Handle_TodayQueueFilter_ShouldReturnOnlyWaitingOrInConsultationPatients()
    {
        // Act
        var result = await Handle(PatientQueueFilterEnum.TodayQueue);

        // Assert
        result.Patients.Should().ContainSingle(p => p.PatientId == 1);
        result.Patients.Single().TodayConsultationStatus.Should().Be(ConsultationStatusEnum.Waiting);
    }

    [Fact]
    public async Task Handle_AllRegisteredFilter_ShouldReturnEveryStaffScopedPatientRegardlessOfStatus()
    {
        // Act
        var result = await Handle(PatientQueueFilterEnum.AllRegistered);

        // Assert: patients 1, 2, 3 (staff 3's roster) — patient 4 (staff 4) excluded.
        result.Patients.Select(p => p.PatientId).Should().BeEquivalentTo(new long[] { 1, 2, 3 });
    }

    [Fact]
    public async Task Handle_NotConsultedTodayFilter_ShouldExcludeOnlyCompletedPatients()
    {
        // Act
        var result = await Handle(PatientQueueFilterEnum.NotConsultedToday);

        // Assert: patient 1 (Waiting) and patient 3 (no record today) both count as "not
        // consulted"; patient 2 (Completed today) is excluded.
        result.Patients.Select(p => p.PatientId).Should().BeEquivalentTo(new long[] { 1, 3 });
    }

    [Fact]
    public async Task Handle_CompletedTodayFilter_ShouldReturnOnlyCompletedPatientsWithPrescriptionId()
    {
        // Act
        var result = await Handle(PatientQueueFilterEnum.CompletedToday);

        // Assert
        var row = result.Patients.Should().ContainSingle().Which;
        row.PatientId.Should().Be(2);
        row.TodayConsultationStatus.Should().Be(ConsultationStatusEnum.Completed);
        row.TodayPrescriptionId.Should().Be(200);
    }

    [Fact]
    public async Task Handle_ShouldNeverReturnPatientsRegisteredByUnassignedStaff()
    {
        // Act
        var result = await Handle(PatientQueueFilterEnum.AllRegistered);

        // Assert
        result.Patients.Should().NotContain(p => p.PatientId == 4);
    }

    [Fact]
    public async Task Handle_WithSearchTerm_ShouldFilterByNameOrPhoneCaseInsensitively()
    {
        // Act
        var result = await Handle(PatientQueueFilterEnum.AllRegistered, "FARID");

        // Assert
        result.Patients.Should().ContainSingle(p => p.PatientId == 3);
    }
}
