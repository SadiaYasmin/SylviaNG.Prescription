using FluentAssertions;
using Moq;
using SylviaNG.Prescription.Application.Common.Exceptions;
using SylviaNG.Prescription.Application.Features.Doctors.Commands.UpdateDoctor;
using SylviaNG.Prescription.Application.Features.Doctors.Models;
using SylviaNG.Prescription.Application.Interfaces.Externals;
using SylviaNG.Prescription.Application.Interfaces.Repositories;
using SylviaNG.Prescription.Application.Interfaces.Services;
using SylviaNG.Prescription.Domain.Entities;
using SylviaNG.Prescription.Domain.Enums;
using SylviaNG.Prescription.SharedKernel.Generic;

namespace SylviaNG.Prescription.Tests.Handlers.Doctors;

public class UpdateDoctorHandlerTests
{
    private readonly Mock<IDoctorRepository> _doctorRepositoryMock = new();
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IKeycloakAdminClient> _adminClientMock = new();
    private readonly Mock<IFileStorageService> _fileStorageServiceMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly UpdateDoctorHandler _handler;

    public UpdateDoctorHandlerTests()
    {
        _handler = new UpdateDoctorHandler(
            _doctorRepositoryMock.Object,
            _userRepositoryMock.Object,
            _adminClientMock.Object,
            _fileStorageServiceMock.Object,
            _unitOfWorkMock.Object);
    }

    private static Doctor ExistingDoctor() => new()
    {
        DoctorId = 1,
        UserId = 5,
        FullName = "Dr. Old Name",
        Phone = "01711111111"
    };

    private static User ExistingUser() => new()
    {
        UserId = 5,
        KeycloakId = "kc-5",
        Username = "existing.doctor",
        Role = UserRoleEnum.Doctor,
        IsActive = true
    };

    [Fact]
    public async Task Handle_WithValidRequest_ShouldUpdateDoctorProfile()
    {
        // Arrange
        var doctor = ExistingDoctor();
        var user = ExistingUser();
        _doctorRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(doctor);
        _userRepositoryMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(user);

        var request = new UpdateDoctorRequest { FullName = "Dr. New Name", Phone = "01712345678", IsActive = true };

        // Act
        var result = await _handler.Handle(new UpdateDoctorCommand(1, request), default);

        // Assert
        result.FullName.Should().Be("Dr. New Name");
        result.Phone.Should().Be("01712345678");
        _doctorRepositoryMock.Verify(r => r.Update(It.IsAny<Doctor>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_TogglingIsActiveToFalse_ShouldDisableInKeycloak()
    {
        // Arrange
        var doctor = ExistingDoctor();
        var user = ExistingUser();
        _doctorRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(doctor);
        _userRepositoryMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(user);

        var request = new UpdateDoctorRequest { FullName = "Dr. New Name", Phone = "01712345678", IsActive = false };

        // Act
        var result = await _handler.Handle(new UpdateDoctorCommand(1, request), default);

        // Assert
        result.IsActive.Should().BeFalse();
        _adminClientMock.Verify(a => a.SetUserEnabledAsync("kc-5", false), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentDoctor_ShouldThrowNotFoundException()
    {
        // Arrange
        _doctorRepositoryMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Doctor?)null);
        var request = new UpdateDoctorRequest { FullName = "Dr. New Name", Phone = "01712345678" };

        // Act
        var act = () => _handler.Handle(new UpdateDoctorCommand(999, request), default);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WithLicenseNumberUsedByAnotherDoctor_ShouldThrowDuplicateException()
    {
        // Arrange
        var doctor = ExistingDoctor();
        var user = ExistingUser();
        _doctorRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(doctor);
        _userRepositoryMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(user);
        _doctorRepositoryMock.Setup(r => r.ExistsByLicenseNumberAsync("BMDC-999", 1)).ReturnsAsync(true);

        var request = new UpdateDoctorRequest { FullName = "Dr. New Name", Phone = "01712345678", LicenseNumber = "BMDC-999" };

        // Act
        var act = () => _handler.Handle(new UpdateDoctorCommand(1, request), default);

        // Assert
        await act.Should().ThrowAsync<DuplicateException>();
    }
}
