using FluentAssertions;
using MockQueryable;
using Moq;
using SylviaNG.Prescription.Application.Features.Doctors.Models;
using SylviaNG.Prescription.Application.Features.Doctors.Queries.GetDoctorList;
using SylviaNG.Prescription.Application.Interfaces.Repositories;
using SylviaNG.Prescription.Domain.Entities;
using SylviaNG.Prescription.Domain.Enums;

namespace SylviaNG.Prescription.Tests.Handlers.Doctors;

public class GetDoctorListHandlerTests
{
    private readonly Mock<IDoctorRepository> _doctorRepositoryMock = new();
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly GetDoctorListHandler _handler;

    private readonly List<Doctor> _doctors = new()
    {
        new Doctor { DoctorId = 1, UserId = 1, FullName = "Dr. Alice Rahman", Department = "Cardiology", Specialization = "Cardiologist", LicenseNumber = "BMDC-1" },
        new Doctor { DoctorId = 2, UserId = 2, FullName = "Dr. Bashir Khan", Department = "Neurology", Specialization = "Neurologist", LicenseNumber = "BMDC-2" },
    };

    private readonly List<User> _users = new()
    {
        new User { UserId = 1, KeycloakId = "kc-1", Username = "alice", Role = UserRoleEnum.Doctor, IsActive = true },
        new User { UserId = 2, KeycloakId = "kc-2", Username = "bashir", Role = UserRoleEnum.Doctor, IsActive = false },
    };

    public GetDoctorListHandlerTests()
    {
        _doctorRepositoryMock.Setup(r => r.Query(It.IsAny<bool>())).Returns(_doctors.BuildMock());
        _userRepositoryMock.Setup(r => r.Query(It.IsAny<bool>())).Returns(_users.BuildMock());
        _handler = new GetDoctorListHandler(_doctorRepositoryMock.Object, _userRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WithNoFilters_ShouldReturnAllDoctorsAndSummary()
    {
        // Act
        var result = await _handler.Handle(new GetDoctorListQuery(new DoctorListRequest()), default);

        // Assert
        result.Doctors.Should().HaveCount(2);
        result.Summary.TotalDoctors.Should().Be(2);
        result.Summary.ActiveDoctors.Should().Be(1);
        result.Summary.TotalPrescriptions.Should().Be(0);
        result.Summary.TotalMedicineEntries.Should().Be(0);
    }

    [Fact]
    public async Task Handle_WithSearchTerm_ShouldFilterBySpecialization()
    {
        // Act
        var result = await _handler.Handle(new GetDoctorListQuery(new DoctorListRequest { SearchTerm = "cardio" }), default);

        // Assert
        result.Doctors.Should().ContainSingle(d => d.FullName == "Dr. Alice Rahman");
    }

    [Fact]
    public async Task Handle_WithActiveFilter_ShouldReturnOnlyActiveDoctors()
    {
        // Act
        var result = await _handler.Handle(new GetDoctorListQuery(new DoctorListRequest { IsActive = true }), default);

        // Assert
        result.Doctors.Should().ContainSingle(d => d.FullName == "Dr. Alice Rahman");
        result.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task Handle_WithDepartmentFilter_ShouldReturnOnlyThatDepartment()
    {
        // Act
        var result = await _handler.Handle(new GetDoctorListQuery(new DoctorListRequest { Department = "Neurology" }), default);

        // Assert
        result.Doctors.Should().ContainSingle(d => d.FullName == "Dr. Bashir Khan");
    }
}
