using FluentAssertions;
using Moq;
using SylviaNG.Prescription.Application.Common.Exceptions;
using SylviaNG.Prescription.Application.Features.Patients.Commands.CreatePatient;
using SylviaNG.Prescription.Application.Features.Patients.Models;
using SylviaNG.Prescription.Application.Interfaces.Repositories;
using SylviaNG.Prescription.Domain.Entities;
using SylviaNG.Prescription.Domain.Enums;
using SylviaNG.Prescription.SharedKernel.Generic;
using SylviaNG.Prescription.Tests.TestHelpers;

namespace SylviaNG.Prescription.Tests.Handlers.Patients;

public class CreatePatientHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IStaffRepository> _staffRepositoryMock = new();
    private readonly Mock<IPatientRepository> _patientRepositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly CreatePatientHandler _handler;

    public CreatePatientHandlerTests()
    {
        _unitOfWorkMock.Setup(u => u.Context).Returns(InMemoryDbContextFactory.Create());
        _handler = new CreatePatientHandler(_userRepositoryMock.Object, _staffRepositoryMock.Object, _patientRepositoryMock.Object, _unitOfWorkMock.Object);
    }

    private static CreatePatientRequest ValidRequest() => new()
    {
        Name = "John Doe",
        Phone = "01712345678",
        DateOfBirth = new DateOnly(1990, 1, 1)
    };

    private static User CallerUser() => new() { UserId = 5, KeycloakId = "kc-staff-1", Username = "amina", Role = UserRoleEnum.Staff, IsActive = true };
    private static Staff CallerStaff() => new() { StaffId = 3, UserId = 5, FullName = "Amina Karim", Phone = "01711111111" };

    [Fact]
    public async Task Handle_WithValidRequest_ShouldCreatePatientRegisteredByCallingStaff()
    {
        // Arrange
        _userRepositoryMock.Setup(r => r.GetByKeycloakIdAsync("kc-staff-1")).ReturnsAsync(CallerUser());
        _staffRepositoryMock.Setup(r => r.GetByUserIdAsync(5)).ReturnsAsync(CallerStaff());
        _patientRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Patient>()))
            .Callback<Patient>(p => p.PatientId = 100)
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(new CreatePatientCommand("kc-staff-1", ValidRequest()), default);

        // Assert
        result.PatientId.Should().Be(100);
        result.RegisteredByStaffId.Should().Be(3);
        result.RegisteredByName.Should().Be("Amina Karim");
        _patientRepositoryMock.Verify(r => r.AddAsync(It.Is<Patient>(p => p.RegisteredByStaffId == 3 && p.Name == "John Doe")), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldSetRegisteredAtServerSide_RegardlessOfRequestContent()
    {
        // Arrange
        _userRepositoryMock.Setup(r => r.GetByKeycloakIdAsync("kc-staff-1")).ReturnsAsync(CallerUser());
        _staffRepositoryMock.Setup(r => r.GetByUserIdAsync(5)).ReturnsAsync(CallerStaff());

        var before = DateTime.UtcNow;

        // Act
        await _handler.Handle(new CreatePatientCommand("kc-staff-1", ValidRequest()), default);
        var after = DateTime.UtcNow;

        // Assert
        _patientRepositoryMock.Verify(r => r.AddAsync(It.Is<Patient>(p => p.RegisteredAt >= before && p.RegisteredAt <= after)), Times.Once);
    }

    [Fact]
    public async Task Handle_WithUnknownKeycloakId_ShouldThrowNotFoundException()
    {
        // Arrange
        _userRepositoryMock.Setup(r => r.GetByKeycloakIdAsync("kc-unknown")).ReturnsAsync((User?)null);

        // Act
        var act = () => _handler.Handle(new CreatePatientCommand("kc-unknown", ValidRequest()), default);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenCallerUserIsNotStaff_ShouldThrowNotFoundException()
    {
        // Arrange
        _userRepositoryMock.Setup(r => r.GetByKeycloakIdAsync("kc-doc-1")).ReturnsAsync(
            new User { UserId = 9, KeycloakId = "kc-doc-1", Username = "dr.one", Role = UserRoleEnum.Doctor, IsActive = true });
        _staffRepositoryMock.Setup(r => r.GetByUserIdAsync(9)).ReturnsAsync((Staff?)null);

        // Act
        var act = () => _handler.Handle(new CreatePatientCommand("kc-doc-1", ValidRequest()), default);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }
}
