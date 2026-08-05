using FluentAssertions;
using Moq;
using SylviaNG.Prescription.Application.Common.Exceptions;
using SylviaNG.Prescription.Application.Features.Doctors.Queries.GetDoctorDetails;
using SylviaNG.Prescription.Application.Interfaces.Repositories;
using SylviaNG.Prescription.Domain.Entities;
using SylviaNG.Prescription.Domain.Enums;

namespace SylviaNG.Prescription.Tests.Handlers.Doctors;

public class GetDoctorDetailsHandlerTests
{
    private readonly Mock<IDoctorRepository> _doctorRepositoryMock = new();
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly GetDoctorDetailsHandler _handler;

    public GetDoctorDetailsHandlerTests()
    {
        _handler = new GetDoctorDetailsHandler(_doctorRepositoryMock.Object, _userRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WithExistingDoctor_ShouldReturnProfileWithZeroStatePerformance()
    {
        // Arrange
        var doctor = new Doctor { DoctorId = 1, UserId = 5, FullName = "Dr. Jane Doe" };
        var user = new User { UserId = 5, KeycloakId = "kc-5", Username = "jane.doe", Role = UserRoleEnum.Doctor, IsActive = true };
        _doctorRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(doctor);
        _userRepositoryMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(user);

        // Act
        var result = await _handler.Handle(new GetDoctorDetailsQuery(1), default);

        // Assert
        result.Profile.FullName.Should().Be("Dr. Jane Doe");
        result.Performance.TotalPrescriptions.Should().Be(0);
        result.Performance.TopMedicines.Should().BeEmpty();
        result.Performance.RecentPrescriptions.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WithNonExistentDoctor_ShouldThrowNotFoundException()
    {
        // Arrange
        _doctorRepositoryMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Doctor?)null);

        // Act
        var act = () => _handler.Handle(new GetDoctorDetailsQuery(999), default);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }
}
