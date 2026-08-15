using FluentAssertions;
using MockQueryable;
using Moq;
using SylviaNG.Prescription.Application.Features.Analytics.Queries.GetMyStaffAnalytics;
using SylviaNG.Prescription.Application.Interfaces.Repositories;
using SylviaNG.Prescription.Domain.Entities;
using SylviaNG.Prescription.Domain.Enums;
using SylviaNG.Prescription.Infrastructure.Data;
using SylviaNG.Prescription.SharedKernel.Generic;
using SylviaNG.Prescription.Tests.TestHelpers;

namespace SylviaNG.Prescription.Tests.Handlers.Analytics;

public class GetMyStaffAnalyticsHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IStaffRepository> _staffRepositoryMock = new();
    private readonly Mock<IDoctorRepository> _doctorRepositoryMock = new();
    private readonly Mock<IPatientRepository> _patientRepositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly ApplicationDBContext _context = InMemoryDbContextFactory.Create();
    private readonly GetMyStaffAnalyticsHandler _handler;

    public GetMyStaffAnalyticsHandlerTests()
    {
        _unitOfWorkMock.Setup(u => u.Context).Returns(_context);

        _userRepositoryMock.Setup(r => r.GetByKeycloakIdAsync("kc-staff-3")).ReturnsAsync(
            new User { UserId = 5, KeycloakId = "kc-staff-3", Role = UserRoleEnum.Staff, IsActive = true, Username = "amina" });
        _staffRepositoryMock.Setup(r => r.GetByUserIdAsync(5)).ReturnsAsync(new Staff { StaffId = 3, UserId = 5, FullName = "Amina Karim" });

        _context.Doctors.AddRange(
            new Doctor { DoctorId = 10, FullName = "Dr. Ten" },
            new Doctor { DoctorId = 20, FullName = "Dr. Twenty" });
        _context.StaffDoctors.AddRange(
            new StaffDoctor { StaffId = 3, DoctorId = 10 },
            new StaffDoctor { StaffId = 3, DoctorId = 20 });
        _context.SaveChanges();

        _patientRepositoryMock.Setup(r => r.Query(It.IsAny<bool>())).Returns(new List<Patient>
        {
            new() { PatientId = 1, RegisteredByStaffId = 3 },
            new() { PatientId = 2, RegisteredByStaffId = 4 }, // a different staff member — must not count
        }.BuildMock());

        _handler = new GetMyStaffAnalyticsHandler(
            _userRepositoryMock.Object, _staffRepositoryMock.Object, _doctorRepositoryMock.Object,
            _patientRepositoryMock.Object, _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldScopeToOwnRegisteredPatientsAndOwnAssignedDoctors()
    {
        var result = await _handler.Handle(new GetMyStaffAnalyticsQuery("kc-staff-3"), default);

        result.PatientsRegisteredByMe.Should().Be(1);
        result.AssignedDoctors.Should().HaveCount(2);
        result.AssignedDoctors.Should().Contain(d => d.DoctorId == 10 && d.FullName == "Dr. Ten");
        result.AssignedDoctors.Should().Contain(d => d.DoctorId == 20 && d.FullName == "Dr. Twenty");
    }
}
