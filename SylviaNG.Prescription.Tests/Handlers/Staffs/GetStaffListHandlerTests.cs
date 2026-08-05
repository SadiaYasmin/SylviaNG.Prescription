using FluentAssertions;
using MockQueryable;
using Moq;
using SylviaNG.Prescription.Application.Features.Staffs.Models;
using SylviaNG.Prescription.Application.Features.Staffs.Queries.GetStaffList;
using SylviaNG.Prescription.Application.Interfaces.Repositories;
using SylviaNG.Prescription.Domain.Entities;
using SylviaNG.Prescription.Domain.Enums;
using SylviaNG.Prescription.Infrastructure.Data;
using SylviaNG.Prescription.SharedKernel.Generic;
using SylviaNG.Prescription.Tests.TestHelpers;

namespace SylviaNG.Prescription.Tests.Handlers.Staffs;

public class GetStaffListHandlerTests
{
    private readonly Mock<IStaffRepository> _staffRepositoryMock = new();
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly ApplicationDBContext _context = InMemoryDbContextFactory.Create();
    private readonly GetStaffListHandler _handler;

    private readonly List<Staff> _staff = new()
    {
        new Staff { StaffId = 1, UserId = 1, FullName = "Amina Karim", Phone = "01711111111", Department = "Front Desk" },
        new Staff { StaffId = 2, UserId = 2, FullName = "Babul Hossain", Phone = "01722222222", Department = "Pharmacy" },
    };

    private readonly List<User> _users = new()
    {
        new User { UserId = 1, KeycloakId = "kc-1", Username = "amina", Role = UserRoleEnum.Staff, IsActive = true },
        new User { UserId = 2, KeycloakId = "kc-2", Username = "babul", Role = UserRoleEnum.Staff, IsActive = false },
    };

    public GetStaffListHandlerTests()
    {
        _staffRepositoryMock.Setup(r => r.Query(It.IsAny<bool>())).Returns(_staff.BuildMock());
        _userRepositoryMock.Setup(r => r.Query(It.IsAny<bool>())).Returns(_users.BuildMock());
        _unitOfWorkMock.Setup(u => u.Context).Returns(_context);
        _handler = new GetStaffListHandler(_staffRepositoryMock.Object, _userRepositoryMock.Object, _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_WithNoFilters_ShouldReturnAllStaff()
    {
        // Act
        var result = await _handler.Handle(new GetStaffListQuery(new StaffListRequest()), default);

        // Assert
        result.Staff.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task Handle_WithSearchTerm_ShouldFilterByDepartment()
    {
        // Act
        var result = await _handler.Handle(new GetStaffListQuery(new StaffListRequest { SearchTerm = "pharmacy" }), default);

        // Assert
        result.Staff.Should().ContainSingle(s => s.FullName == "Babul Hossain");
    }

    [Fact]
    public async Task Handle_WithActiveFilter_ShouldReturnOnlyActiveStaff()
    {
        // Act
        var result = await _handler.Handle(new GetStaffListQuery(new StaffListRequest { IsActive = true }), default);

        // Assert
        result.Staff.Should().ContainSingle(s => s.FullName == "Amina Karim");
        result.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task Handle_WithDepartmentFilter_ShouldReturnOnlyThatDepartment()
    {
        // Act
        var result = await _handler.Handle(new GetStaffListQuery(new StaffListRequest { Department = "Front Desk" }), default);

        // Assert
        result.Staff.Should().ContainSingle(s => s.FullName == "Amina Karim");
    }

    [Fact]
    public async Task Handle_ShouldIncludeAssignedDoctorsPerStaffMember()
    {
        // Arrange
        _context.Doctors.Add(new Doctor { DoctorId = 10, FullName = "Dr. Ten" });
        _context.StaffDoctors.Add(new StaffDoctor { StaffDoctorId = 1, StaffId = 1, DoctorId = 10 });
        await _context.SaveChangesAsync();

        // Act
        var result = await _handler.Handle(new GetStaffListQuery(new StaffListRequest()), default);

        // Assert
        var amina = result.Staff.Single(s => s.StaffId == 1);
        amina.AssignedDoctors.Should().ContainSingle(d => d.DoctorId == 10 && d.FullName == "Dr. Ten");

        var babul = result.Staff.Single(s => s.StaffId == 2);
        babul.AssignedDoctors.Should().BeEmpty();
    }
}
