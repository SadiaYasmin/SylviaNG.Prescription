using FluentAssertions;
using Moq;
using SylviaNG.Prescription.Application.Common.Exceptions;
using SylviaNG.Prescription.Application.Features.Doctors.Commands.UpdateDoctorProfile;
using SylviaNG.Prescription.Application.Features.Doctors.Models;
using SylviaNG.Prescription.Application.Interfaces.Repositories;
using SylviaNG.Prescription.Domain.Entities;
using SylviaNG.Prescription.Domain.Enums;
using SylviaNG.Prescription.SharedKernel.Generic;

namespace SylviaNG.Prescription.Tests.Handlers.Doctors;

public class UpdateDoctorProfileHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IStaffRepository> _staffRepositoryMock = new();
    private readonly Mock<IDoctorRepository> _doctorRepositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly UpdateDoctorProfileHandler _handler;

    public UpdateDoctorProfileHandlerTests()
    {
        _handler = new UpdateDoctorProfileHandler(
            _userRepositoryMock.Object, _staffRepositoryMock.Object, _doctorRepositoryMock.Object, _unitOfWorkMock.Object);
    }

    private void SetUpCaller(long doctorId = 10, long userId = 5)
    {
        _userRepositoryMock.Setup(r => r.GetByKeycloakIdAsync("kc-doc")).ReturnsAsync(
            new User { UserId = userId, KeycloakId = "kc-doc", Role = UserRoleEnum.Doctor, IsActive = true, Username = "doc" });
        _doctorRepositoryMock.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(new Doctor { DoctorId = doctorId, UserId = userId, FullName = "Dr. Old", Phone = "01711111111" });
        _doctorRepositoryMock.Setup(r => r.GetByIdAsync(doctorId)).ReturnsAsync(new Doctor { DoctorId = doctorId, UserId = userId, FullName = "Dr. Old", Phone = "01711111111" });
        _userRepositoryMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(new User { UserId = userId, KeycloakId = "kc-doc", Role = UserRoleEnum.Doctor, IsActive = true, Username = "doc" });
    }

    private static UpdateDoctorProfileRequest ValidRequest() => new() { FullName = "Dr. New Name", Phone = "01712345678", Qualification = "MBBS", Email = "new@example.com" };

    [Fact]
    public async Task Handle_WithValidRequest_ShouldUpdateOwnProfile()
    {
        SetUpCaller();

        var result = await _handler.Handle(new UpdateDoctorProfileCommand("kc-doc", ValidRequest()), default);

        result.FullName.Should().Be("Dr. New Name");
        result.Phone.Should().Be("01712345678");
        result.Email.Should().Be("new@example.com");
        _doctorRepositoryMock.Verify(r => r.Update(It.IsAny<Doctor>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldOnlyEverAffectTheCallersOwnRecord_NeverATargetIdFromTheRequest()
    {
        // The request DTO has no doctor id field at all — the caller is resolved solely from
        // the JWT via CallerContextResolver, so there's nothing in the request that could be
        // used to target another doctor's record even if a malicious client tried.
        SetUpCaller(doctorId: 42, userId: 7);

        await _handler.Handle(new UpdateDoctorProfileCommand("kc-doc", ValidRequest()), default);

        _doctorRepositoryMock.Verify(r => r.GetByIdAsync(42), Times.Once);
        _doctorRepositoryMock.Verify(r => r.GetByIdAsync(It.Is<long>(id => id != 42)), Times.Never);
    }

    [Fact]
    public async Task Handle_WithDuplicateLicenseNumber_ShouldThrowDuplicateException()
    {
        SetUpCaller();
        _doctorRepositoryMock.Setup(r => r.ExistsByLicenseNumberAsync("A-999", 10)).ReturnsAsync(true);
        var request = ValidRequest();
        request.LicenseNumber = "A-999";

        var act = () => _handler.Handle(new UpdateDoctorProfileCommand("kc-doc", request), default);

        await act.Should().ThrowAsync<DuplicateException>();
        _doctorRepositoryMock.Verify(r => r.Update(It.IsAny<Doctor>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithBlankEmail_ShouldNotOverwriteExistingEmail()
    {
        SetUpCaller();
        var request = ValidRequest();
        request.Email = "";

        var result = await _handler.Handle(new UpdateDoctorProfileCommand("kc-doc", request), default);

        result.Email.Should().BeNull(); // existing user had no email set in SetUpCaller
    }
}
