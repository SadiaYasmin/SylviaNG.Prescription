using FluentAssertions;
using Moq;
using SylviaNG.Prescription.Application.Common.Exceptions;
using SylviaNG.Prescription.Application.Features.Doctors.Commands.DeactivateDoctor;
using SylviaNG.Prescription.Application.Interfaces.Externals;
using SylviaNG.Prescription.Application.Interfaces.Repositories;
using SylviaNG.Prescription.Domain.Entities;
using SylviaNG.Prescription.Domain.Enums;
using SylviaNG.Prescription.SharedKernel.Generic;

namespace SylviaNG.Prescription.Tests.Handlers.Doctors;

public class DeactivateDoctorHandlerTests
{
    private readonly Mock<IDoctorRepository> _doctorRepositoryMock = new();
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IKeycloakAdminClient> _adminClientMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly DeactivateDoctorHandler _handler;

    public DeactivateDoctorHandlerTests()
    {
        _handler = new DeactivateDoctorHandler(
            _doctorRepositoryMock.Object,
            _userRepositoryMock.Object,
            _adminClientMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_WithActiveDoctor_ShouldDisableInKeycloakAndDeactivateUser()
    {
        // Arrange
        var doctor = new Doctor { DoctorId = 1, UserId = 5 };
        var user = new User { UserId = 5, KeycloakId = "kc-5", Username = "doctor.dev", Role = UserRoleEnum.Doctor, IsActive = true };
        _doctorRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(doctor);
        _userRepositoryMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(user);

        // Act
        await _handler.Handle(new DeactivateDoctorCommand(1), default);

        // Assert
        _adminClientMock.Verify(a => a.SetUserEnabledAsync("kc-5", false), Times.Once);
        user.IsActive.Should().BeFalse();
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_WithAlreadyInactiveDoctor_ShouldBeIdempotent()
    {
        // Arrange
        var doctor = new Doctor { DoctorId = 1, UserId = 5 };
        var user = new User { UserId = 5, KeycloakId = "kc-5", Username = "doctor.dev", Role = UserRoleEnum.Doctor, IsActive = false };
        _doctorRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(doctor);
        _userRepositoryMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(user);

        // Act
        await _handler.Handle(new DeactivateDoctorCommand(1), default);

        // Assert
        _adminClientMock.Verify(a => a.SetUserEnabledAsync(It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task Handle_WithNonExistentDoctor_ShouldThrowNotFoundException()
    {
        // Arrange
        _doctorRepositoryMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Doctor?)null);

        // Act
        var act = () => _handler.Handle(new DeactivateDoctorCommand(999), default);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }
}
