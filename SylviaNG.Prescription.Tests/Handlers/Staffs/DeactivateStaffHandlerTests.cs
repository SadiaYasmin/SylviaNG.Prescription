using FluentAssertions;
using Moq;
using SylviaNG.Prescription.Application.Common.Exceptions;
using SylviaNG.Prescription.Application.Features.Staffs.Commands.DeactivateStaff;
using SylviaNG.Prescription.Application.Interfaces.Externals;
using SylviaNG.Prescription.Application.Interfaces.Repositories;
using SylviaNG.Prescription.Domain.Entities;
using SylviaNG.Prescription.Domain.Enums;
using SylviaNG.Prescription.SharedKernel.Generic;

namespace SylviaNG.Prescription.Tests.Handlers.Staffs;

public class DeactivateStaffHandlerTests
{
    private readonly Mock<IStaffRepository> _staffRepositoryMock = new();
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IKeycloakAdminClient> _adminClientMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly DeactivateStaffHandler _handler;

    public DeactivateStaffHandlerTests()
    {
        _handler = new DeactivateStaffHandler(
            _staffRepositoryMock.Object,
            _userRepositoryMock.Object,
            _adminClientMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_WithActiveStaff_ShouldDisableInKeycloakAndDeactivateUser()
    {
        // Arrange
        var staff = new Staff { StaffId = 1, UserId = 5 };
        var user = new User { UserId = 5, KeycloakId = "kc-5", Username = "staff.dev", Role = UserRoleEnum.Staff, IsActive = true };
        _staffRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(staff);
        _userRepositoryMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(user);

        // Act
        await _handler.Handle(new DeactivateStaffCommand(1), default);

        // Assert
        _adminClientMock.Verify(a => a.SetUserEnabledAsync("kc-5", false), Times.Once);
        user.IsActive.Should().BeFalse();
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_WithAlreadyInactiveStaff_ShouldBeIdempotent()
    {
        // Arrange
        var staff = new Staff { StaffId = 1, UserId = 5 };
        var user = new User { UserId = 5, KeycloakId = "kc-5", Username = "staff.dev", Role = UserRoleEnum.Staff, IsActive = false };
        _staffRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(staff);
        _userRepositoryMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(user);

        // Act
        await _handler.Handle(new DeactivateStaffCommand(1), default);

        // Assert
        _adminClientMock.Verify(a => a.SetUserEnabledAsync(It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task Handle_WithNonExistentStaff_ShouldThrowNotFoundException()
    {
        // Arrange
        _staffRepositoryMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Staff?)null);

        // Act
        var act = () => _handler.Handle(new DeactivateStaffCommand(999), default);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }
}
