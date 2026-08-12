using FluentAssertions;
using MockQueryable;
using Moq;
using SylviaNG.Prescription.Application.Features.Patients.Models;
using SylviaNG.Prescription.Application.Features.Patients.Queries.GetPatientList;
using SylviaNG.Prescription.Application.Interfaces.Repositories;
using SylviaNG.Prescription.Domain.Entities;
using SylviaNG.Prescription.Domain.Enums;
using SylviaNG.Prescription.Infrastructure.Data;
using SylviaNG.Prescription.SharedKernel.Generic;
using SylviaNG.Prescription.Tests.TestHelpers;

namespace SylviaNG.Prescription.Tests.Handlers.Patients;

public class GetPatientListHandlerTests
{
    private readonly Mock<IPatientRepository> _patientRepositoryMock = new();
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IStaffRepository> _staffRepositoryMock = new();
    private readonly Mock<IDoctorRepository> _doctorRepositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly ApplicationDBContext _context = InMemoryDbContextFactory.Create();
    private readonly GetPatientListHandler _handler;

    // Alice and Chandra were registered by staff 3; Bilal was registered by staff 4.
    private readonly List<Patient> _patients = new()
    {
        new Patient { PatientId = 1, Name = "Alice Ahmed", Phone = "01711111111", RegisteredByStaffId = 3 },
        new Patient { PatientId = 2, Name = "Bilal Rahman", Phone = "01722222222", RegisteredByStaffId = 4 },
        new Patient { PatientId = 3, Name = "Chandra Roy", Phone = "01733333333", RegisteredByStaffId = 3 },
    };

    public GetPatientListHandlerTests()
    {
        _patientRepositoryMock.Setup(r => r.Query(It.IsAny<bool>())).Returns(_patients.BuildMock());
        _unitOfWorkMock.Setup(u => u.Context).Returns(_context);
        _handler = new GetPatientListHandler(
            _patientRepositoryMock.Object,
            _userRepositoryMock.Object,
            _staffRepositoryMock.Object,
            _doctorRepositoryMock.Object,
            _unitOfWorkMock.Object);

        _context.Staff.AddRange(
            new Staff { StaffId = 3, UserId = 5, FullName = "Amina Karim", Phone = "01711111111" },
            new Staff { StaffId = 4, UserId = 6, FullName = "Babul Hossain", Phone = "01722222222" });
        _context.SaveChanges();
    }

    [Fact]
    public async Task Handle_WhenCallerIsStaff_ShouldReturnOnlyPatientsTheyRegistered()
    {
        // Arrange
        _userRepositoryMock.Setup(r => r.GetByKeycloakIdAsync("kc-staff-3")).ReturnsAsync(
            new User { UserId = 5, KeycloakId = "kc-staff-3", Role = UserRoleEnum.Staff, IsActive = true, Username = "amina" });
        _staffRepositoryMock.Setup(r => r.GetByUserIdAsync(5)).ReturnsAsync(new Staff { StaffId = 3, UserId = 5, FullName = "Amina Karim" });

        // Act
        var result = await _handler.Handle(new GetPatientListQuery("kc-staff-3", new PatientListRequest()), default);

        // Assert
        result.Patients.Should().HaveCount(2);
        result.Patients.Should().OnlyContain(p => p.RegisteredByStaffId == 3);
        result.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task Handle_WhenCallerIsDoctor_ShouldReturnOnlyPatientsRegisteredByAssignedStaff()
    {
        // Arrange: doctor 10 is assigned to staff 3 only.
        _userRepositoryMock.Setup(r => r.GetByKeycloakIdAsync("kc-doc-10")).ReturnsAsync(
            new User { UserId = 7, KeycloakId = "kc-doc-10", Role = UserRoleEnum.Doctor, IsActive = true, Username = "dr.ten" });
        _doctorRepositoryMock.Setup(r => r.GetByUserIdAsync(7)).ReturnsAsync(new Doctor { DoctorId = 10, UserId = 7, FullName = "Dr. Ten" });
        _context.StaffDoctors.Add(new StaffDoctor { StaffDoctorId = 1, StaffId = 3, DoctorId = 10 });
        await _context.SaveChangesAsync();

        // Act
        var result = await _handler.Handle(new GetPatientListQuery("kc-doc-10", new PatientListRequest()), default);

        // Assert
        result.Patients.Should().HaveCount(2);
        result.Patients.Should().OnlyContain(p => p.RegisteredByStaffId == 3);
    }

    [Fact]
    public async Task Handle_WhenCallerIsDoctorWithNoAssignedStaff_ShouldReturnEmpty()
    {
        // Arrange: doctor 20 has no StaffDoctor rows at all.
        _userRepositoryMock.Setup(r => r.GetByKeycloakIdAsync("kc-doc-20")).ReturnsAsync(
            new User { UserId = 8, KeycloakId = "kc-doc-20", Role = UserRoleEnum.Doctor, IsActive = true, Username = "dr.twenty" });
        _doctorRepositoryMock.Setup(r => r.GetByUserIdAsync(8)).ReturnsAsync(new Doctor { DoctorId = 20, UserId = 8, FullName = "Dr. Twenty" });

        // Act
        var result = await _handler.Handle(new GetPatientListQuery("kc-doc-20", new PatientListRequest()), default);

        // Assert
        result.Patients.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_WhenCallerIsAdmin_ShouldReturnAllPatients()
    {
        // Arrange
        _userRepositoryMock.Setup(r => r.GetByKeycloakIdAsync("kc-admin-1")).ReturnsAsync(
            new User { UserId = 9, KeycloakId = "kc-admin-1", Role = UserRoleEnum.Admin, IsActive = true, Username = "admin" });

        // Act
        var result = await _handler.Handle(new GetPatientListQuery("kc-admin-1", new PatientListRequest()), default);

        // Assert
        result.Patients.Should().HaveCount(3);
        result.TotalCount.Should().Be(3);
    }

    [Fact]
    public async Task Handle_WithSearchTerm_ShouldFilterByNameOrPhoneCaseInsensitively()
    {
        // Arrange
        _userRepositoryMock.Setup(r => r.GetByKeycloakIdAsync("kc-admin-1")).ReturnsAsync(
            new User { UserId = 9, KeycloakId = "kc-admin-1", Role = UserRoleEnum.Admin, IsActive = true, Username = "admin" });

        // Act
        var result = await _handler.Handle(new GetPatientListQuery("kc-admin-1", new PatientListRequest { SearchTerm = "BILAL" }), default);

        // Assert
        result.Patients.Should().ContainSingle(p => p.Name == "Bilal Rahman");
    }

    [Fact]
    public async Task Handle_ShouldIncludeRegisteredByNameForEveryRow()
    {
        // Arrange
        _userRepositoryMock.Setup(r => r.GetByKeycloakIdAsync("kc-admin-1")).ReturnsAsync(
            new User { UserId = 9, KeycloakId = "kc-admin-1", Role = UserRoleEnum.Admin, IsActive = true, Username = "admin" });

        // Act
        var result = await _handler.Handle(new GetPatientListQuery("kc-admin-1", new PatientListRequest()), default);

        // Assert
        result.Patients.Single(p => p.PatientId == 1).RegisteredByName.Should().Be("Amina Karim");
        result.Patients.Single(p => p.PatientId == 2).RegisteredByName.Should().Be("Babul Hossain");
    }

    [Fact]
    public async Task Handle_ShouldReturnNewestFirst()
    {
        // Arrange
        _userRepositoryMock.Setup(r => r.GetByKeycloakIdAsync("kc-admin-1")).ReturnsAsync(
            new User { UserId = 9, KeycloakId = "kc-admin-1", Role = UserRoleEnum.Admin, IsActive = true, Username = "admin" });

        // Act
        var result = await _handler.Handle(new GetPatientListQuery("kc-admin-1", new PatientListRequest()), default);

        // Assert
        result.Patients.Select(p => p.PatientId).Should().ContainInOrder(3L, 2L, 1L);
    }
}
