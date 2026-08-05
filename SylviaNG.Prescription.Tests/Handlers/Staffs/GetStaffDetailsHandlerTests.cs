using FluentAssertions;
using Moq;
using SylviaNG.Prescription.Application.Common.Exceptions;
using SylviaNG.Prescription.Application.Features.Staffs.Queries.GetStaffDetails;
using SylviaNG.Prescription.Application.Interfaces.Repositories;
using SylviaNG.Prescription.Domain.Entities;
using SylviaNG.Prescription.Domain.Enums;
using SylviaNG.Prescription.Infrastructure.Data;
using SylviaNG.Prescription.SharedKernel.Generic;
using SylviaNG.Prescription.Tests.TestHelpers;

namespace SylviaNG.Prescription.Tests.Handlers.Staffs;

public class GetStaffDetailsHandlerTests
{
    private readonly Mock<IStaffRepository> _staffRepositoryMock = new();
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly ApplicationDBContext _context = InMemoryDbContextFactory.Create();
    private readonly GetStaffDetailsHandler _handler;

    public GetStaffDetailsHandlerTests()
    {
        _unitOfWorkMock.Setup(u => u.Context).Returns(_context);
        _handler = new GetStaffDetailsHandler(_staffRepositoryMock.Object, _userRepositoryMock.Object, _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_WithExistingStaff_ShouldReturnProfileWithAssignedDoctors()
    {
        // Arrange
        var staff = new Staff { StaffId = 1, UserId = 5, FullName = "Amina Karim", Phone = "01711111111", Department = "Front Desk" };
        var user = new User { UserId = 5, KeycloakId = "kc-5", Username = "amina", Role = UserRoleEnum.Staff, IsActive = true };
        _staffRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(staff);
        _userRepositoryMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(user);

        _context.Doctors.Add(new Doctor { DoctorId = 10, FullName = "Dr. Ten" });
        _context.StaffDoctors.Add(new StaffDoctor { StaffDoctorId = 1, StaffId = 1, DoctorId = 10 });
        await _context.SaveChangesAsync();

        // Act
        var result = await _handler.Handle(new GetStaffDetailsQuery(1), default);

        // Assert
        result.Profile.FullName.Should().Be("Amina Karim");
        result.Profile.AssignedDoctors.Should().ContainSingle(d => d.DoctorId == 10 && d.FullName == "Dr. Ten");
    }

    [Fact]
    public async Task Handle_WithNoAssignedDoctors_ShouldReturnEmptyList()
    {
        // Arrange
        var staff = new Staff { StaffId = 1, UserId = 5, FullName = "Amina Karim", Phone = "01711111111" };
        var user = new User { UserId = 5, KeycloakId = "kc-5", Username = "amina", Role = UserRoleEnum.Staff, IsActive = true };
        _staffRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(staff);
        _userRepositoryMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(user);

        // Act
        var result = await _handler.Handle(new GetStaffDetailsQuery(1), default);

        // Assert
        result.Profile.AssignedDoctors.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WithNonExistentStaff_ShouldThrowNotFoundException()
    {
        // Arrange
        _staffRepositoryMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Staff?)null);

        // Act
        var act = () => _handler.Handle(new GetStaffDetailsQuery(999), default);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WithStaffButNoMatchingUser_ShouldThrowNotFoundException()
    {
        // Arrange
        var staff = new Staff { StaffId = 1, UserId = 5, FullName = "Amina Karim", Phone = "01711111111" };
        _staffRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(staff);
        _userRepositoryMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync((User?)null);

        // Act
        var act = () => _handler.Handle(new GetStaffDetailsQuery(1), default);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }
}
