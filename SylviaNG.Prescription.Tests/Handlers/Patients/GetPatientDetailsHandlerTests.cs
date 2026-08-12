using FluentAssertions;
using Moq;
using SylviaNG.Prescription.Application.Common.Exceptions;
using SylviaNG.Prescription.Application.Features.Patients.Queries.GetPatientDetails;
using SylviaNG.Prescription.Application.Interfaces.Repositories;
using SylviaNG.Prescription.Domain.Entities;
using SylviaNG.Prescription.Domain.Enums;
using SylviaNG.Prescription.Infrastructure.Data;
using SylviaNG.Prescription.SharedKernel.Generic;
using SylviaNG.Prescription.Tests.TestHelpers;

namespace SylviaNG.Prescription.Tests.Handlers.Patients;

public class GetPatientDetailsHandlerTests
{
    private readonly Mock<IPatientRepository> _patientRepositoryMock = new();
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IStaffRepository> _staffRepositoryMock = new();
    private readonly Mock<IDoctorRepository> _doctorRepositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly ApplicationDBContext _context = InMemoryDbContextFactory.Create();
    private readonly GetPatientDetailsHandler _handler;

    private static Patient ExistingPatient() => new()
    {
        PatientId = 1,
        Name = "Alice Ahmed",
        Phone = "01711111111",
        RegisteredByStaffId = 3
    };

    public GetPatientDetailsHandlerTests()
    {
        _unitOfWorkMock.Setup(u => u.Context).Returns(_context);
        _handler = new GetPatientDetailsHandler(
            _patientRepositoryMock.Object,
            _userRepositoryMock.Object,
            _staffRepositoryMock.Object,
            _doctorRepositoryMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_WhenCallerIsTheRegisteringStaff_ShouldReturnDetails()
    {
        // Arrange
        _patientRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(ExistingPatient());
        _userRepositoryMock.Setup(r => r.GetByKeycloakIdAsync("kc-staff-3")).ReturnsAsync(
            new User { UserId = 5, KeycloakId = "kc-staff-3", Role = UserRoleEnum.Staff, IsActive = true, Username = "amina" });
        _staffRepositoryMock.Setup(r => r.GetByUserIdAsync(5)).ReturnsAsync(new Staff { StaffId = 3, UserId = 5, FullName = "Amina Karim" });
        _staffRepositoryMock.Setup(r => r.GetByIdAsync(3)).ReturnsAsync(new Staff { StaffId = 3, FullName = "Amina Karim" });

        // Act
        var result = await _handler.Handle(new GetPatientDetailsQuery(1, "kc-staff-3"), default);

        // Assert
        result.Profile.Name.Should().Be("Alice Ahmed");
        result.Profile.RegisteredByName.Should().Be("Amina Karim");
    }

    [Fact]
    public async Task Handle_WhenCallerIsUnrelatedStaff_ShouldThrowNotFoundException()
    {
        // Arrange
        _patientRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(ExistingPatient());
        _userRepositoryMock.Setup(r => r.GetByKeycloakIdAsync("kc-staff-99")).ReturnsAsync(
            new User { UserId = 6, KeycloakId = "kc-staff-99", Role = UserRoleEnum.Staff, IsActive = true, Username = "babul" });
        _staffRepositoryMock.Setup(r => r.GetByUserIdAsync(6)).ReturnsAsync(new Staff { StaffId = 99, UserId = 6, FullName = "Babul Hossain" });

        // Act
        var act = () => _handler.Handle(new GetPatientDetailsQuery(1, "kc-staff-99"), default);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenCallerIsAssignedDoctor_ShouldReturnDetails()
    {
        // Arrange
        _context.StaffDoctors.Add(new StaffDoctor { StaffDoctorId = 1, StaffId = 3, DoctorId = 10 });
        await _context.SaveChangesAsync();

        _patientRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(ExistingPatient());
        _userRepositoryMock.Setup(r => r.GetByKeycloakIdAsync("kc-doc-10")).ReturnsAsync(
            new User { UserId = 7, KeycloakId = "kc-doc-10", Role = UserRoleEnum.Doctor, IsActive = true, Username = "dr.ten" });
        _doctorRepositoryMock.Setup(r => r.GetByUserIdAsync(7)).ReturnsAsync(new Doctor { DoctorId = 10, UserId = 7, FullName = "Dr. Ten" });
        _staffRepositoryMock.Setup(r => r.GetByIdAsync(3)).ReturnsAsync(new Staff { StaffId = 3, FullName = "Amina Karim" });

        // Act
        var result = await _handler.Handle(new GetPatientDetailsQuery(1, "kc-doc-10"), default);

        // Assert
        result.Profile.Name.Should().Be("Alice Ahmed");
    }

    [Fact]
    public async Task Handle_WhenCallerIsUnassignedDoctor_ShouldThrowNotFoundException()
    {
        // Arrange
        _patientRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(ExistingPatient());
        _userRepositoryMock.Setup(r => r.GetByKeycloakIdAsync("kc-doc-20")).ReturnsAsync(
            new User { UserId = 8, KeycloakId = "kc-doc-20", Role = UserRoleEnum.Doctor, IsActive = true, Username = "dr.twenty" });
        _doctorRepositoryMock.Setup(r => r.GetByUserIdAsync(8)).ReturnsAsync(new Doctor { DoctorId = 20, UserId = 8, FullName = "Dr. Twenty" });

        // Act
        var act = () => _handler.Handle(new GetPatientDetailsQuery(1, "kc-doc-20"), default);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenCallerIsAdmin_ShouldReturnDetailsRegardlessOfWhoRegisteredIt()
    {
        // Arrange
        _patientRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(ExistingPatient());
        _userRepositoryMock.Setup(r => r.GetByKeycloakIdAsync("kc-admin-1")).ReturnsAsync(
            new User { UserId = 9, KeycloakId = "kc-admin-1", Role = UserRoleEnum.Admin, IsActive = true, Username = "admin" });
        _staffRepositoryMock.Setup(r => r.GetByIdAsync(3)).ReturnsAsync(new Staff { StaffId = 3, FullName = "Amina Karim" });

        // Act
        var result = await _handler.Handle(new GetPatientDetailsQuery(1, "kc-admin-1"), default);

        // Assert
        result.Profile.Name.Should().Be("Alice Ahmed");
    }

    [Fact]
    public async Task Handle_WithNonExistentPatient_ShouldThrowNotFoundException()
    {
        // Arrange
        _patientRepositoryMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Patient?)null);

        // Act
        var act = () => _handler.Handle(new GetPatientDetailsQuery(999, "kc-admin-1"), default);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }
}
