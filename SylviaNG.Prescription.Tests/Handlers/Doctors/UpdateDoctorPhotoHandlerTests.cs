using FluentAssertions;
using Moq;
using SylviaNG.Prescription.Application.Features.Doctors.Commands.UpdateDoctorPhoto;
using SylviaNG.Prescription.Application.Features.Doctors.Models;
using SylviaNG.Prescription.Application.Interfaces.Repositories;
using SylviaNG.Prescription.Domain.Entities;
using SylviaNG.Prescription.Domain.Enums;
using SylviaNG.Prescription.SharedKernel.Generic;

namespace SylviaNG.Prescription.Tests.Handlers.Doctors;

public class UpdateDoctorPhotoHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IStaffRepository> _staffRepositoryMock = new();
    private readonly Mock<IDoctorRepository> _doctorRepositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly UpdateDoctorPhotoHandler _handler;

    public UpdateDoctorPhotoHandlerTests()
    {
        _handler = new UpdateDoctorPhotoHandler(
            _userRepositoryMock.Object, _staffRepositoryMock.Object, _doctorRepositoryMock.Object, _unitOfWorkMock.Object);

        _userRepositoryMock.Setup(r => r.GetByKeycloakIdAsync("kc-doc")).ReturnsAsync(
            new User { UserId = 5, KeycloakId = "kc-doc", Role = UserRoleEnum.Doctor, IsActive = true, Username = "doc" });
        _doctorRepositoryMock.Setup(r => r.GetByUserIdAsync(5)).ReturnsAsync(new Doctor { DoctorId = 10, UserId = 5, FullName = "Dr. Doc" });
        _doctorRepositoryMock.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(new Doctor { DoctorId = 10, UserId = 5, FullName = "Dr. Doc" });
        _userRepositoryMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(new User { UserId = 5, KeycloakId = "kc-doc", Role = UserRoleEnum.Doctor, IsActive = true, Username = "doc" });
    }

    [Fact]
    public async Task Handle_WithPhotoDataUrl_ShouldSetPhoto()
    {
        var result = await _handler.Handle(new UpdateDoctorPhotoCommand("kc-doc", new UpdateDoctorPhotoRequest { PhotoBase64 = "data:image/png;base64,xyz" }), default);

        result.PhotoBase64.Should().Be("data:image/png;base64,xyz");
        _doctorRepositoryMock.Verify(r => r.Update(It.IsAny<Doctor>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNullPhoto_ShouldRemovePhoto()
    {
        var result = await _handler.Handle(new UpdateDoctorPhotoCommand("kc-doc", new UpdateDoctorPhotoRequest { PhotoBase64 = null }), default);

        result.PhotoBase64.Should().BeNull();
    }
}
