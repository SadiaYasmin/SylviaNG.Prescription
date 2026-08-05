using FluentAssertions;
using Moq;
using SylviaNG.Prescription.Application.Common.Exceptions;
using SylviaNG.Prescription.Application.Features.Staffs.Queries.GetAssignedStaff;
using SylviaNG.Prescription.Application.Interfaces.Repositories;
using SylviaNG.Prescription.Domain.Entities;
using SylviaNG.Prescription.Domain.Enums;
using SylviaNG.Prescription.Infrastructure.Data;
using SylviaNG.Prescription.SharedKernel.Generic;
using SylviaNG.Prescription.Tests.TestHelpers;

namespace SylviaNG.Prescription.Tests.Handlers.Staffs;

public class GetAssignedStaffHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IDoctorRepository> _doctorRepositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly ApplicationDBContext _context = InMemoryDbContextFactory.Create();
    private readonly GetAssignedStaffHandler _handler;

    public GetAssignedStaffHandlerTests()
    {
        _unitOfWorkMock.Setup(u => u.Context).Returns(_context);
        _handler = new GetAssignedStaffHandler(_userRepositoryMock.Object, _doctorRepositoryMock.Object, _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_WithAssignedStaff_ShouldReturnThem()
    {
        // Arrange: doctor 10 (user kc-doc-10) has staff 1 assigned.
        var callerUser = new User { UserId = 100, KeycloakId = "kc-doc-10", Username = "dr.ten", Role = UserRoleEnum.Doctor, IsActive = true };
        var doctor = new Doctor { DoctorId = 10, UserId = 100, FullName = "Dr. Ten" };
        var staffUser = new User { UserId = 1, KeycloakId = "kc-1", Username = "amina", Role = UserRoleEnum.Staff, IsActive = true };
        var staff = new Staff { StaffId = 1, UserId = 1, FullName = "Amina Karim", Phone = "01711111111" };

        _context.Users.AddRange(callerUser, staffUser);
        _context.Staff.Add(staff);
        _context.StaffDoctors.Add(new StaffDoctor { StaffDoctorId = 1, StaffId = 1, DoctorId = 10 });
        await _context.SaveChangesAsync();

        _userRepositoryMock.Setup(r => r.GetByKeycloakIdAsync("kc-doc-10")).ReturnsAsync(callerUser);
        _doctorRepositoryMock.Setup(r => r.GetByUserIdAsync(100)).ReturnsAsync(doctor);

        // Act
        var result = await _handler.Handle(new GetAssignedStaffQuery("kc-doc-10"), default);

        // Assert
        result.Staff.Should().ContainSingle(s => s.FullName == "Amina Karim");
    }

    [Fact]
    public async Task Handle_WithNoAssignedStaff_ShouldReturnEmpty()
    {
        // Arrange
        var callerUser = new User { UserId = 100, KeycloakId = "kc-doc-10", Username = "dr.ten", Role = UserRoleEnum.Doctor, IsActive = true };
        var doctor = new Doctor { DoctorId = 10, UserId = 100, FullName = "Dr. Ten" };
        _context.Users.Add(callerUser);
        await _context.SaveChangesAsync();

        _userRepositoryMock.Setup(r => r.GetByKeycloakIdAsync("kc-doc-10")).ReturnsAsync(callerUser);
        _doctorRepositoryMock.Setup(r => r.GetByUserIdAsync(100)).ReturnsAsync(doctor);

        // Act
        var result = await _handler.Handle(new GetAssignedStaffQuery("kc-doc-10"), default);

        // Assert
        result.Staff.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_WithUnknownKeycloakId_ShouldThrowNotFoundException()
    {
        // Arrange
        _userRepositoryMock.Setup(r => r.GetByKeycloakIdAsync("kc-unknown")).ReturnsAsync((User?)null);

        // Act
        var act = () => _handler.Handle(new GetAssignedStaffQuery("kc-unknown"), default);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WithUserThatIsNotADoctor_ShouldThrowNotFoundException()
    {
        // Arrange
        var callerUser = new User { UserId = 200, KeycloakId = "kc-staff-1", Username = "amina", Role = UserRoleEnum.Staff, IsActive = true };
        _userRepositoryMock.Setup(r => r.GetByKeycloakIdAsync("kc-staff-1")).ReturnsAsync(callerUser);
        _doctorRepositoryMock.Setup(r => r.GetByUserIdAsync(200)).ReturnsAsync((Doctor?)null);

        // Act
        var act = () => _handler.Handle(new GetAssignedStaffQuery("kc-staff-1"), default);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_ShouldOnlyReturnStaffAssignedToThisDoctor_NotOthers()
    {
        // Arrange: doctor 10 has staff 1; doctor 20 has staff 2. Caller is doctor 10.
        var callerUser = new User { UserId = 100, KeycloakId = "kc-doc-10", Username = "dr.ten", Role = UserRoleEnum.Doctor, IsActive = true };
        var otherDoctorUser = new User { UserId = 101, KeycloakId = "kc-doc-20", Username = "dr.twenty", Role = UserRoleEnum.Doctor, IsActive = true };
        var doctor = new Doctor { DoctorId = 10, UserId = 100, FullName = "Dr. Ten" };
        var otherDoctor = new Doctor { DoctorId = 20, UserId = 101, FullName = "Dr. Twenty" };

        var staffUser1 = new User { UserId = 1, KeycloakId = "kc-1", Username = "amina", Role = UserRoleEnum.Staff, IsActive = true };
        var staffUser2 = new User { UserId = 2, KeycloakId = "kc-2", Username = "babul", Role = UserRoleEnum.Staff, IsActive = true };
        var staff1 = new Staff { StaffId = 1, UserId = 1, FullName = "Amina Karim", Phone = "01711111111" };
        var staff2 = new Staff { StaffId = 2, UserId = 2, FullName = "Babul Hossain", Phone = "01722222222" };

        _context.Users.AddRange(callerUser, otherDoctorUser, staffUser1, staffUser2);
        _context.Doctors.AddRange(doctor, otherDoctor);
        _context.Staff.AddRange(staff1, staff2);
        _context.StaffDoctors.Add(new StaffDoctor { StaffDoctorId = 1, StaffId = 1, DoctorId = 10 });
        _context.StaffDoctors.Add(new StaffDoctor { StaffDoctorId = 2, StaffId = 2, DoctorId = 20 });
        await _context.SaveChangesAsync();

        _userRepositoryMock.Setup(r => r.GetByKeycloakIdAsync("kc-doc-10")).ReturnsAsync(callerUser);
        _doctorRepositoryMock.Setup(r => r.GetByUserIdAsync(100)).ReturnsAsync(doctor);

        // Act
        var result = await _handler.Handle(new GetAssignedStaffQuery("kc-doc-10"), default);

        // Assert
        result.Staff.Should().ContainSingle(s => s.FullName == "Amina Karim");
        result.Staff.Should().NotContain(s => s.FullName == "Babul Hossain");
    }
}
