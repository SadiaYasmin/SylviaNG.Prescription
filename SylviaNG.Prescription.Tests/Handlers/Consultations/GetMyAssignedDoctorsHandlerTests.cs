using FluentAssertions;
using Moq;
using SylviaNG.Prescription.Application.Features.Consultations.Queries.GetMyAssignedDoctors;
using SylviaNG.Prescription.Application.Interfaces.Repositories;
using SylviaNG.Prescription.Domain.Entities;
using SylviaNG.Prescription.Domain.Enums;
using SylviaNG.Prescription.Infrastructure.Data;
using SylviaNG.Prescription.SharedKernel.Generic;
using SylviaNG.Prescription.Tests.TestHelpers;

namespace SylviaNG.Prescription.Tests.Handlers.Consultations;

public class GetMyAssignedDoctorsHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IStaffRepository> _staffRepositoryMock = new();
    private readonly Mock<IDoctorRepository> _doctorRepositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly ApplicationDBContext _context = InMemoryDbContextFactory.Create();
    private readonly GetMyAssignedDoctorsHandler _handler;

    public GetMyAssignedDoctorsHandlerTests()
    {
        _userRepositoryMock.Setup(r => r.GetByKeycloakIdAsync("kc-staff-3")).ReturnsAsync(
            new User { UserId = 5, KeycloakId = "kc-staff-3", Role = UserRoleEnum.Staff, IsActive = true, Username = "amina" });
        _staffRepositoryMock.Setup(r => r.GetByUserIdAsync(5)).ReturnsAsync(new Staff { StaffId = 3, UserId = 5, FullName = "Amina Karim" });

        _context.Doctors.AddRange(
            new Doctor { DoctorId = 10, FullName = "Dr. Ten" },
            new Doctor { DoctorId = 20, FullName = "Dr. Twenty" },
            new Doctor { DoctorId = 30, FullName = "Dr. Thirty" });
        _context.SaveChanges();

        _unitOfWorkMock.Setup(u => u.Context).Returns(_context);

        _handler = new GetMyAssignedDoctorsHandler(_userRepositoryMock.Object, _staffRepositoryMock.Object, _doctorRepositoryMock.Object, _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnOnlyDoctorsAssignedToCallingStaff()
    {
        // Arrange: staff 3 is assigned doctors 10 and 30, not 20.
        _context.StaffDoctors.AddRange(
            new StaffDoctor { StaffDoctorId = 1, StaffId = 3, DoctorId = 10 },
            new StaffDoctor { StaffDoctorId = 2, StaffId = 3, DoctorId = 30 },
            new StaffDoctor { StaffDoctorId = 3, StaffId = 4, DoctorId = 20 });
        await _context.SaveChangesAsync();

        // Act
        var result = await _handler.Handle(new GetMyAssignedDoctorsQuery("kc-staff-3"), default);

        // Assert
        result.Should().HaveCount(2);
        result.Select(d => d.DoctorId).Should().BeEquivalentTo(new[] { 10L, 30L });
        result.Should().ContainSingle(d => d.FullName == "Dr. Ten");
    }

    [Fact]
    public async Task Handle_WhenNoDoctorsAssigned_ShouldReturnEmpty()
    {
        // Act
        var result = await _handler.Handle(new GetMyAssignedDoctorsQuery("kc-staff-3"), default);

        // Assert
        result.Should().BeEmpty();
    }
}
