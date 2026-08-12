using FluentAssertions;
using Moq;
using SylviaNG.Prescription.Application.Common.Exceptions;
using SylviaNG.Prescription.Application.Features.Patients.Commands.UpdatePatient;
using SylviaNG.Prescription.Application.Features.Patients.Models;
using SylviaNG.Prescription.Application.Interfaces.Repositories;
using SylviaNG.Prescription.Domain.Entities;
using SylviaNG.Prescription.Domain.Enums;
using SylviaNG.Prescription.Infrastructure.Data;
using SylviaNG.Prescription.SharedKernel.Generic;
using SylviaNG.Prescription.Tests.TestHelpers;

namespace SylviaNG.Prescription.Tests.Handlers.Patients;

public class UpdatePatientHandlerTests
{
    private readonly Mock<IPatientRepository> _patientRepositoryMock = new();
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IStaffRepository> _staffRepositoryMock = new();
    private readonly Mock<IDoctorRepository> _doctorRepositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly ApplicationDBContext _context = InMemoryDbContextFactory.Create();
    private readonly UpdatePatientHandler _handler;

    public UpdatePatientHandlerTests()
    {
        _unitOfWorkMock.Setup(u => u.Context).Returns(_context);
        _handler = new UpdatePatientHandler(
            _patientRepositoryMock.Object,
            _userRepositoryMock.Object,
            _staffRepositoryMock.Object,
            _doctorRepositoryMock.Object,
            _unitOfWorkMock.Object);
    }

    private static Patient ExistingPatient() => new()
    {
        PatientId = 1,
        Name = "Old Name",
        Phone = "01711111111",
        RegisteredByStaffId = 3,
        RegisteredAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
    };

    private static UpdatePatientRequest ValidRequest() => new()
    {
        Name = "New Name",
        Phone = "01712345678",
        DateOfBirth = new DateOnly(1990, 1, 1)
    };

    [Fact]
    public async Task Handle_WhenCallerIsTheRegisteringStaff_ShouldUpdatePatient()
    {
        // Arrange: staff 3 registered patient 1, and is the caller.
        var callerUser = new User { UserId = 5, KeycloakId = "kc-staff-3", Role = UserRoleEnum.Staff, IsActive = true, Username = "amina" };
        var callerStaff = new Staff { StaffId = 3, UserId = 5, FullName = "Amina Karim", Phone = "01711111111" };

        _patientRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(ExistingPatient());
        _userRepositoryMock.Setup(r => r.GetByKeycloakIdAsync("kc-staff-3")).ReturnsAsync(callerUser);
        _staffRepositoryMock.Setup(r => r.GetByUserIdAsync(5)).ReturnsAsync(callerStaff);
        _staffRepositoryMock.Setup(r => r.GetByIdAsync(3)).ReturnsAsync(callerStaff);

        // Act
        var result = await _handler.Handle(new UpdatePatientCommand(1, "kc-staff-3", ValidRequest()), default);

        // Assert
        result.Name.Should().Be("New Name");
        result.Phone.Should().Be("01712345678");
        result.RegisteredByName.Should().Be("Amina Karim");
        _patientRepositoryMock.Verify(r => r.Update(It.IsAny<Patient>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenCallerIsStaffWhoDidNotRegisterPatient_ShouldThrowNotFoundException()
    {
        // Arrange: patient 1 was registered by staff 3; caller is staff 99.
        var callerUser = new User { UserId = 6, KeycloakId = "kc-staff-99", Role = UserRoleEnum.Staff, IsActive = true, Username = "babul" };
        var callerStaff = new Staff { StaffId = 99, UserId = 6, FullName = "Babul Hossain", Phone = "01722222222" };

        _patientRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(ExistingPatient());
        _userRepositoryMock.Setup(r => r.GetByKeycloakIdAsync("kc-staff-99")).ReturnsAsync(callerUser);
        _staffRepositoryMock.Setup(r => r.GetByUserIdAsync(6)).ReturnsAsync(callerStaff);

        // Act
        var act = () => _handler.Handle(new UpdatePatientCommand(1, "kc-staff-99", ValidRequest()), default);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
        _patientRepositoryMock.Verify(r => r.Update(It.IsAny<Patient>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenCallerIsDoctorAssignedToRegisteringStaff_ShouldUpdatePatient()
    {
        // Arrange: patient 1 registered by staff 3; doctor 10 is assigned to staff 3.
        var callerUser = new User { UserId = 7, KeycloakId = "kc-doc-10", Role = UserRoleEnum.Doctor, IsActive = true, Username = "dr.ten" };
        var callerDoctor = new Doctor { DoctorId = 10, UserId = 7, FullName = "Dr. Ten" };
        _context.StaffDoctors.Add(new StaffDoctor { StaffDoctorId = 1, StaffId = 3, DoctorId = 10 });
        await _context.SaveChangesAsync();

        _patientRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(ExistingPatient());
        _userRepositoryMock.Setup(r => r.GetByKeycloakIdAsync("kc-doc-10")).ReturnsAsync(callerUser);
        _doctorRepositoryMock.Setup(r => r.GetByUserIdAsync(7)).ReturnsAsync(callerDoctor);
        _staffRepositoryMock.Setup(r => r.GetByIdAsync(3)).ReturnsAsync(new Staff { StaffId = 3, FullName = "Amina Karim" });

        // Act
        var result = await _handler.Handle(new UpdatePatientCommand(1, "kc-doc-10", ValidRequest()), default);

        // Assert
        result.Name.Should().Be("New Name");
    }

    [Fact]
    public async Task Handle_WhenCallerIsDoctorNotAssignedToRegisteringStaff_ShouldThrowNotFoundException()
    {
        // Arrange: patient 1 registered by staff 3; doctor 20 is assigned to staff 99 only.
        var callerUser = new User { UserId = 8, KeycloakId = "kc-doc-20", Role = UserRoleEnum.Doctor, IsActive = true, Username = "dr.twenty" };
        var callerDoctor = new Doctor { DoctorId = 20, UserId = 8, FullName = "Dr. Twenty" };
        _context.StaffDoctors.Add(new StaffDoctor { StaffDoctorId = 1, StaffId = 99, DoctorId = 20 });
        await _context.SaveChangesAsync();

        _patientRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(ExistingPatient());
        _userRepositoryMock.Setup(r => r.GetByKeycloakIdAsync("kc-doc-20")).ReturnsAsync(callerUser);
        _doctorRepositoryMock.Setup(r => r.GetByUserIdAsync(8)).ReturnsAsync(callerDoctor);

        // Act
        var act = () => _handler.Handle(new UpdatePatientCommand(1, "kc-doc-20", ValidRequest()), default);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenCallerIsAdmin_ShouldUpdatePatientRegardlessOfWhoRegisteredIt()
    {
        // Arrange
        var callerUser = new User { UserId = 9, KeycloakId = "kc-admin-1", Role = UserRoleEnum.Admin, IsActive = true, Username = "admin" };

        _patientRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(ExistingPatient());
        _userRepositoryMock.Setup(r => r.GetByKeycloakIdAsync("kc-admin-1")).ReturnsAsync(callerUser);
        _staffRepositoryMock.Setup(r => r.GetByIdAsync(3)).ReturnsAsync(new Staff { StaffId = 3, FullName = "Amina Karim" });

        // Act
        var result = await _handler.Handle(new UpdatePatientCommand(1, "kc-admin-1", ValidRequest()), default);

        // Assert
        result.Name.Should().Be("New Name");
    }

    [Fact]
    public async Task Handle_WithNonExistentPatient_ShouldThrowNotFoundException()
    {
        // Arrange
        _patientRepositoryMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Patient?)null);

        // Act
        var act = () => _handler.Handle(new UpdatePatientCommand(999, "kc-staff-3", ValidRequest()), default);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }
}
