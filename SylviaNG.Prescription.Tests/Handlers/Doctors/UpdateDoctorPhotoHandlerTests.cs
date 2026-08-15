using FluentAssertions;
using Moq;
using SylviaNG.Prescription.Application.Features.Doctors.Commands.UpdateDoctorPhoto;
using SylviaNG.Prescription.Application.Features.Doctors.Models;
using SylviaNG.Prescription.Application.Interfaces.Repositories;
using SylviaNG.Prescription.Application.Interfaces.Services;
using SylviaNG.Prescription.Domain.Entities;
using SylviaNG.Prescription.Domain.Enums;
using SylviaNG.Prescription.SharedKernel.Generic;

namespace SylviaNG.Prescription.Tests.Handlers.Doctors;

public class UpdateDoctorPhotoHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IStaffRepository> _staffRepositoryMock = new();
    private readonly Mock<IDoctorRepository> _doctorRepositoryMock = new();
    private readonly Mock<IFileStorageService> _fileStorageServiceMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly UpdateDoctorPhotoHandler _handler;

    public UpdateDoctorPhotoHandlerTests()
    {
        _handler = new UpdateDoctorPhotoHandler(
            _userRepositoryMock.Object, _staffRepositoryMock.Object, _doctorRepositoryMock.Object,
            _fileStorageServiceMock.Object, _unitOfWorkMock.Object);

        _userRepositoryMock.Setup(r => r.GetByKeycloakIdAsync("kc-doc")).ReturnsAsync(
            new User { UserId = 5, KeycloakId = "kc-doc", Role = UserRoleEnum.Doctor, IsActive = true, Username = "doc" });
        _doctorRepositoryMock.Setup(r => r.GetByUserIdAsync(5)).ReturnsAsync(new Doctor { DoctorId = 10, UserId = 5, FullName = "Dr. Doc" });
        _doctorRepositoryMock.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(new Doctor { DoctorId = 10, UserId = 5, FullName = "Dr. Doc" });
        _userRepositoryMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(new User { UserId = 5, KeycloakId = "kc-doc", Role = UserRoleEnum.Doctor, IsActive = true, Username = "doc" });

        _fileStorageServiceMock.Setup(s => s.SaveImageAsync("data:image/png;base64,xyz", "doctor-photos", It.IsAny<CancellationToken>()))
            .ReturnsAsync("/uploads/doctor-photos/new-photo.png");
        _fileStorageServiceMock.Setup(s => s.SaveImageAsync(null, "doctor-photos", It.IsAny<CancellationToken>()))
            .ReturnsAsync((string?)null);
    }

    [Fact]
    public async Task Handle_WithPhotoDataUrl_ShouldSetPhoto()
    {
        var result = await _handler.Handle(new UpdateDoctorPhotoCommand("kc-doc", new UpdateDoctorPhotoRequest { PhotoBase64 = "data:image/png;base64,xyz" }), default);

        result.PhotoUrl.Should().Be("/uploads/doctor-photos/new-photo.png");
        _doctorRepositoryMock.Verify(r => r.Update(It.IsAny<Doctor>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNullPhoto_ShouldRemovePhoto()
    {
        var result = await _handler.Handle(new UpdateDoctorPhotoCommand("kc-doc", new UpdateDoctorPhotoRequest { PhotoBase64 = null }), default);

        result.PhotoUrl.Should().BeNull();
        _fileStorageServiceMock.Verify(s => s.DeleteAsync(null), Times.Once);
    }
}
