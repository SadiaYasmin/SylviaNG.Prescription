using FluentAssertions;
using Moq;
using SylviaNG.Prescription.Application.Common.Exceptions;
using SylviaNG.Prescription.Application.Common.Models;
using SylviaNG.Prescription.Application.Features.Auth.Models;
using SylviaNG.Prescription.Application.Features.Doctors.Commands.CreateDoctor;
using SylviaNG.Prescription.Application.Features.Doctors.Models;
using SylviaNG.Prescription.Application.Interfaces.Repositories;
using SylviaNG.Prescription.Application.Interfaces.Services;
using SylviaNG.Prescription.Domain.Entities;
using SylviaNG.Prescription.Domain.Enums;
using SylviaNG.Prescription.SharedKernel.Generic;

namespace SylviaNG.Prescription.Tests.Handlers.Doctors;

public class CreateDoctorHandlerTests
{
    private readonly Mock<IAuthService> _authServiceMock = new();
    private readonly Mock<IDoctorRepository> _doctorRepositoryMock = new();
    private readonly Mock<IFileStorageService> _fileStorageServiceMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly CreateDoctorHandler _handler;

    public CreateDoctorHandlerTests()
    {
        _handler = new CreateDoctorHandler(
            _authServiceMock.Object, _doctorRepositoryMock.Object, _fileStorageServiceMock.Object, _unitOfWorkMock.Object);
    }

    private static CreateDoctorRequest ValidRequest() => new()
    {
        Username = "new.doctor",
        Email = "new.doctor@example.com",
        FullName = "Dr. Jane Doe",
        Phone = "01712345678",
        LicenseNumber = "BMDC-123"
    };

    [Fact]
    public async Task Handle_WithValidRequest_ShouldProvisionAccountAndCreateDoctorProfile()
    {
        // Arrange
        var request = ValidRequest();
        _doctorRepositoryMock.Setup(r => r.ExistsByLicenseNumberAsync(request.LicenseNumber!, null)).ReturnsAsync(false);
        _authServiceMock.Setup(a => a.CreateUserAccountAsync(It.Is<CreateUserAccountRequest>(r =>
                r.Username == request.Username && r.Role == UserRoleEnum.Doctor)))
            .ReturnsAsync(new CreateUserAccountResponse { UserId = 42, Username = request.Username, TemporaryPassword = "Temp123!" });
        _doctorRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Doctor>()))
            .Callback<Doctor>(d => d.DoctorId = 7)
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(new CreateDoctorCommand(request), default);

        // Assert
        result.DoctorId.Should().Be(7);
        result.UserId.Should().Be(42);
        result.TemporaryPassword.Should().Be("Temp123!");
        _doctorRepositoryMock.Verify(r => r.AddAsync(It.Is<Doctor>(d => d.UserId == 42 && d.FullName == request.FullName)), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_WithDuplicateLicenseNumber_ShouldThrowDuplicateException_WithoutCallingAuthService()
    {
        // Arrange
        var request = ValidRequest();
        _doctorRepositoryMock.Setup(r => r.ExistsByLicenseNumberAsync(request.LicenseNumber!, null)).ReturnsAsync(true);

        // Act
        var act = () => _handler.Handle(new CreateDoctorCommand(request), default);

        // Assert
        await act.Should().ThrowAsync<DuplicateException>();
        _authServiceMock.Verify(a => a.CreateUserAccountAsync(It.IsAny<CreateUserAccountRequest>()), Times.Never);
    }
}
