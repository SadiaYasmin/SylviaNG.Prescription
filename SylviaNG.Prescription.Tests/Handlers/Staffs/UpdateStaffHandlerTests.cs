using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using SylviaNG.Prescription.Application.Common.Exceptions;
using SylviaNG.Prescription.Application.Features.Staffs.Commands.UpdateStaff;
using SylviaNG.Prescription.Application.Features.Staffs.Models;
using SylviaNG.Prescription.Application.Interfaces.Externals;
using SylviaNG.Prescription.Application.Interfaces.Repositories;
using SylviaNG.Prescription.Domain.Entities;
using SylviaNG.Prescription.Domain.Enums;
using SylviaNG.Prescription.Infrastructure.Data;
using SylviaNG.Prescription.SharedKernel.Generic;
using SylviaNG.Prescription.Tests.TestHelpers;

namespace SylviaNG.Prescription.Tests.Handlers.Staffs;

public class UpdateStaffHandlerTests
{
    private readonly Mock<IStaffRepository> _staffRepositoryMock = new();
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IDoctorRepository> _doctorRepositoryMock = new();
    private readonly Mock<IKeycloakAdminClient> _adminClientMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly ApplicationDBContext _context = InMemoryDbContextFactory.Create();
    private readonly UpdateStaffHandler _handler;

    public UpdateStaffHandlerTests()
    {
        _unitOfWorkMock.Setup(u => u.Context).Returns(_context);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).Returns(() => _context.SaveChangesAsync());
        _handler = new UpdateStaffHandler(
            _staffRepositoryMock.Object,
            _userRepositoryMock.Object,
            _doctorRepositoryMock.Object,
            _adminClientMock.Object,
            _unitOfWorkMock.Object);
    }

    private static Staff ExistingStaff() => new()
    {
        StaffId = 1,
        UserId = 5,
        FullName = "Old Name",
        Phone = "01711111111"
    };

    private static User ExistingUser() => new()
    {
        UserId = 5,
        KeycloakId = "kc-5",
        Username = "existing.staff",
        Role = UserRoleEnum.Staff,
        IsActive = true
    };

    [Fact]
    public async Task Handle_WithValidRequest_ShouldUpdateStaffProfile()
    {
        // Arrange
        _staffRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(ExistingStaff());
        _userRepositoryMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(ExistingUser());

        var request = new UpdateStaffRequest { FullName = "New Name", Phone = "01712345678", IsActive = true };

        // Act
        var result = await _handler.Handle(new UpdateStaffCommand(1, request), default);

        // Assert
        result.FullName.Should().Be("New Name");
        result.Phone.Should().Be("01712345678");
        _staffRepositoryMock.Verify(r => r.Update(It.IsAny<Staff>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_TogglingIsActiveToFalse_ShouldDisableInKeycloak()
    {
        // Arrange
        _staffRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(ExistingStaff());
        _userRepositoryMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(ExistingUser());

        var request = new UpdateStaffRequest { FullName = "New Name", Phone = "01712345678", IsActive = false };

        // Act
        var result = await _handler.Handle(new UpdateStaffCommand(1, request), default);

        // Assert
        result.IsActive.Should().BeFalse();
        _adminClientMock.Verify(a => a.SetUserEnabledAsync("kc-5", false), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentStaff_ShouldThrowNotFoundException()
    {
        // Arrange
        _staffRepositoryMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Staff?)null);
        var request = new UpdateStaffRequest { FullName = "New Name", Phone = "01712345678" };

        // Act
        var act = () => _handler.Handle(new UpdateStaffCommand(999, request), default);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WithNonExistentAssignedDoctor_ShouldThrowNotFoundException()
    {
        // Arrange
        _staffRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(ExistingStaff());
        _userRepositoryMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(ExistingUser());
        _doctorRepositoryMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Doctor?)null);

        var request = new UpdateStaffRequest { FullName = "New Name", Phone = "01712345678", AssignedDoctorIds = new List<long> { 99 } };

        // Act
        var act = () => _handler.Handle(new UpdateStaffCommand(1, request), default);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WithChangedAssignments_ShouldDiffAddAndRemoveLinkRows()
    {
        // Arrange: staff 1 is currently assigned to doctor 10 only.
        _context.Doctors.Add(new Doctor { DoctorId = 10, FullName = "Dr. Ten" });
        _context.Doctors.Add(new Doctor { DoctorId = 20, FullName = "Dr. Twenty" });
        _context.StaffDoctors.Add(new StaffDoctor { StaffDoctorId = 1, StaffId = 1, DoctorId = 10 });
        await _context.SaveChangesAsync();

        _staffRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(ExistingStaff());
        _userRepositoryMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(ExistingUser());
        _doctorRepositoryMock.Setup(r => r.GetByIdAsync(20)).ReturnsAsync(new Doctor { DoctorId = 20, FullName = "Dr. Twenty" });

        // Act: replace assignment with doctor 20 only (drop 10, add 20).
        var request = new UpdateStaffRequest { FullName = "New Name", Phone = "01712345678", AssignedDoctorIds = new List<long> { 20 } };
        var result = await _handler.Handle(new UpdateStaffCommand(1, request), default);

        // Assert
        result.AssignedDoctors.Should().ContainSingle(d => d.DoctorId == 20);

        var persistedLinks = await _context.StaffDoctors.AsNoTracking().Where(sd => sd.StaffId == 1).ToListAsync();
        persistedLinks.Should().ContainSingle(l => l.DoctorId == 20);
        persistedLinks.Should().NotContain(l => l.DoctorId == 10);
    }
}
