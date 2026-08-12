using FluentAssertions;
using Moq;
using SylviaNG.Prescription.Application.Common.Exceptions;
using SylviaNG.Prescription.Application.Features.Consultations.Commands.OpenConsultation;
using SylviaNG.Prescription.Application.Interfaces.Repositories;
using SylviaNG.Prescription.Domain.Entities;
using SylviaNG.Prescription.Domain.Enums;
using SylviaNG.Prescription.SharedKernel.Generic;

namespace SylviaNG.Prescription.Tests.Handlers.Consultations;

public class OpenConsultationHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IStaffRepository> _staffRepositoryMock = new();
    private readonly Mock<IDoctorRepository> _doctorRepositoryMock = new();
    private readonly Mock<IPatientRepository> _patientRepositoryMock = new();
    private readonly Mock<IConsultationRepository> _consultationRepositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly OpenConsultationHandler _handler;

    public OpenConsultationHandlerTests()
    {
        _userRepositoryMock.Setup(r => r.GetByKeycloakIdAsync("kc-doc-10")).ReturnsAsync(
            new User { UserId = 7, KeycloakId = "kc-doc-10", Role = UserRoleEnum.Doctor, IsActive = true, Username = "dr.ten" });
        _doctorRepositoryMock.Setup(r => r.GetByUserIdAsync(7)).ReturnsAsync(new Doctor { DoctorId = 10, UserId = 7, FullName = "Dr. Ten" });
        _doctorRepositoryMock.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(new Doctor { DoctorId = 10, UserId = 7, FullName = "Dr. Ten" });
        _patientRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Patient { PatientId = 1, Name = "Alice Ahmed" });

        _handler = new OpenConsultationHandler(
            _userRepositoryMock.Object,
            _staffRepositoryMock.Object,
            _doctorRepositoryMock.Object,
            _patientRepositoryMock.Object,
            _consultationRepositoryMock.Object,
            _unitOfWorkMock.Object);
    }

    private static Consultation WaitingConsultation() => new()
    {
        ConsultationId = 1,
        PatientId = 1,
        DoctorId = 10,
        Status = ConsultationStatusEnum.Waiting,
        DisplayCode = "CN-2026-0001",
        TokenNumber = "T-01",
        VisitDate = new DateOnly(2026, 8, 11)
    };

    [Fact]
    public async Task Handle_WhenWaitingAndCorrectDoctor_ShouldTransitionToInConsultationAndReturnJoinedSummary()
    {
        // Arrange
        var consultation = WaitingConsultation();
        _consultationRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(consultation);

        // Act
        var result = await _handler.Handle(new OpenConsultationCommand(1, "kc-doc-10"), default);

        // Assert
        result.Status.Should().Be(ConsultationStatusEnum.InConsultation);
        result.PatientName.Should().Be("Alice Ahmed");
        result.DoctorName.Should().Be("Dr. Ten");
        consultation.Status.Should().Be(ConsultationStatusEnum.InConsultation);
        _consultationRepositoryMock.Verify(r => r.Update(It.Is<Consultation>(c => c.Status == ConsultationStatusEnum.InConsultation)), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenConsultationDoesNotExist_ShouldThrowNotFoundException()
    {
        // Arrange
        _consultationRepositoryMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Consultation?)null);

        // Act
        var act = () => _handler.Handle(new OpenConsultationCommand(999, "kc-doc-10"), default);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenConsultationIsQueuedForADifferentDoctor_ShouldThrowNotFoundException()
    {
        // Arrange: consultation is queued for doctor 20, caller resolves to doctor 10.
        var consultation = WaitingConsultation();
        consultation.DoctorId = 20;
        _consultationRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(consultation);

        // Act
        var act = () => _handler.Handle(new OpenConsultationCommand(1, "kc-doc-10"), default);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
        _consultationRepositoryMock.Verify(r => r.Update(It.IsAny<Consultation>()), Times.Never);
        consultation.Status.Should().Be(ConsultationStatusEnum.Waiting);
    }

    [Fact]
    public async Task Handle_WhenAlreadyInConsultation_ShouldThrowBadRequestException()
    {
        // Arrange
        var consultation = WaitingConsultation();
        consultation.Status = ConsultationStatusEnum.InConsultation;
        _consultationRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(consultation);

        // Act
        var act = () => _handler.Handle(new OpenConsultationCommand(1, "kc-doc-10"), default);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
        _consultationRepositoryMock.Verify(r => r.Update(It.IsAny<Consultation>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenAlreadyCompleted_ShouldThrowBadRequestException()
    {
        // Arrange
        var consultation = WaitingConsultation();
        consultation.Status = ConsultationStatusEnum.Completed;
        _consultationRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(consultation);

        // Act
        var act = () => _handler.Handle(new OpenConsultationCommand(1, "kc-doc-10"), default);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
    }
}
