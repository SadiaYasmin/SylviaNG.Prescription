using FluentAssertions;
using MockQueryable;
using Moq;
using SylviaNG.Prescription.Application.Common.Exceptions;
using SylviaNG.Prescription.Application.Common.Services;
using SylviaNG.Prescription.Application.Features.Consultations.Commands.CreateConsultation;
using SylviaNG.Prescription.Application.Features.Consultations.Models;
using SylviaNG.Prescription.Application.Interfaces.Repositories;
using SylviaNG.Prescription.Domain.Entities;
using SylviaNG.Prescription.Domain.Enums;
using SylviaNG.Prescription.Infrastructure.Data;
using SylviaNG.Prescription.SharedKernel.Generic;
using SylviaNG.Prescription.SharedKernel.Utils;
using SylviaNG.Prescription.Tests.TestHelpers;

namespace SylviaNG.Prescription.Tests.Handlers.Consultations;

public class CreateConsultationHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IStaffRepository> _staffRepositoryMock = new();
    private readonly Mock<IDoctorRepository> _doctorRepositoryMock = new();
    private readonly Mock<IPatientRepository> _patientRepositoryMock = new();
    private readonly Mock<IConsultationRepository> _consultationRepositoryMock = new();
    private readonly Mock<IPrescriptionRepository> _prescriptionRepositoryMock = new();
    private readonly Mock<ISequenceGenerator> _sequenceGeneratorMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly ApplicationDBContext _context = InMemoryDbContextFactory.Create();
    private readonly CreateConsultationHandler _handler;

    private static readonly DateOnly Today = DateTimeUtility.TodayLocal();

    public CreateConsultationHandlerTests()
    {
        _unitOfWorkMock.Setup(u => u.Context).Returns(_context);
        _consultationRepositoryMock.Setup(r => r.Query(It.IsAny<bool>())).Returns(new List<Consultation>().BuildMock());

        _userRepositoryMock.Setup(r => r.GetByKeycloakIdAsync("kc-staff-3")).ReturnsAsync(
            new User { UserId = 5, KeycloakId = "kc-staff-3", Role = UserRoleEnum.Staff, IsActive = true, Username = "amina" });
        _staffRepositoryMock.Setup(r => r.GetByUserIdAsync(5)).ReturnsAsync(new Staff { StaffId = 3, UserId = 5, FullName = "Amina Karim" });

        _context.StaffDoctors.Add(new StaffDoctor { StaffDoctorId = 1, StaffId = 3, DoctorId = 10 });
        _context.SaveChanges();

        _prescriptionRepositoryMock.Setup(r => r.Query(It.IsAny<bool>())).Returns(new List<PrescriptionRecord>().BuildMock());

        _handler = new CreateConsultationHandler(
            _userRepositoryMock.Object,
            _staffRepositoryMock.Object,
            _doctorRepositoryMock.Object,
            _patientRepositoryMock.Object,
            _consultationRepositoryMock.Object,
            _prescriptionRepositoryMock.Object,
            _sequenceGeneratorMock.Object,
            _unitOfWorkMock.Object);
    }

    private static CreateConsultationRequest ValidRequest(bool force = false) => new()
    {
        PatientId = 1,
        DoctorId = 10,
        Force = force
    };

    private void SetUpExistingConsultations(params Consultation[] consultations)
    {
        _consultationRepositoryMock.Setup(r => r.Query(It.IsAny<bool>())).Returns(consultations.BuildMock());
    }

    [Fact]
    public async Task Handle_WithValidRequestAndNoExisting_ShouldCreateConsultationAndReturnNotDuplicate()
    {
        // Arrange
        _sequenceGeneratorMock.Setup(s => s.GetNextAsync("ConsultationId", Today.Year.ToString(), default)).ReturnsAsync(1);
        _sequenceGeneratorMock.Setup(s => s.GetNextAsync("ConsultationToken:10", DateTimeUtility.FormatDate(Today), default)).ReturnsAsync(1);
        _consultationRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Consultation>()))
            .Callback<Consultation>(c => c.ConsultationId = 100)
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(new CreateConsultationCommand("kc-staff-3", ValidRequest()), default);

        // Assert
        result.DuplicateFound.Should().BeFalse();
        result.Consultation.Should().NotBeNull();
        result.Consultation!.ConsultationId.Should().Be(100);
        result.Consultation.DisplayCode.Should().Be($"CN-{Today.Year}-0001");
        result.Consultation.TokenNumber.Should().Be("T-01");
        result.Consultation.Status.Should().Be(ConsultationStatusEnum.Waiting);
        result.ExistingConsultation.Should().BeNull();

        _consultationRepositoryMock.Verify(r => r.AddAsync(It.Is<Consultation>(c =>
            c.PatientId == 1 && c.DoctorId == 10 && c.RegisteredByStaffId == 3 &&
            c.Status == ConsultationStatusEnum.Waiting && c.VisitDate == Today)), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldSetCheckInAtServerSide_AroundUtcNow()
    {
        // Arrange
        _sequenceGeneratorMock.Setup(s => s.GetNextAsync(It.IsAny<string>(), It.IsAny<string>(), default)).ReturnsAsync(1);
        var before = DateTime.UtcNow;

        // Act
        await _handler.Handle(new CreateConsultationCommand("kc-staff-3", ValidRequest()), default);
        var after = DateTime.UtcNow;

        // Assert
        _consultationRepositoryMock.Verify(r => r.AddAsync(It.Is<Consultation>(c => c.CheckInAt >= before && c.CheckInAt <= after)), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenDoctorNotAssignedToCaller_ShouldThrowBadRequestException()
    {
        // Arrange: doctor 99 is not in StaffDoctors for staff 3.
        var request = ValidRequest();
        request.DoctorId = 99;

        // Act
        var act = () => _handler.Handle(new CreateConsultationCommand("kc-staff-3", request), default);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
        _consultationRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Consultation>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenDuplicateExistsAndNotForced_ShouldReturnDuplicateFoundWithoutCreating()
    {
        // Arrange
        var existing = new Consultation
        {
            ConsultationId = 55,
            PatientId = 1,
            DoctorId = 10,
            VisitDate = Today,
            Status = ConsultationStatusEnum.Waiting,
            DisplayCode = $"CN-{Today.Year}-0005",
            TokenNumber = "T-05"
        };
        SetUpExistingConsultations(existing);

        // Act
        var result = await _handler.Handle(new CreateConsultationCommand("kc-staff-3", ValidRequest(force: false)), default);

        // Assert
        result.DuplicateFound.Should().BeTrue();
        result.ExistingConsultation.Should().NotBeNull();
        result.ExistingConsultation!.ConsultationId.Should().Be(55);
        result.ExistingConsultation.TokenNumber.Should().Be("T-05");
        result.Consultation.Should().BeNull();

        _consultationRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Consultation>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
        _sequenceGeneratorMock.Verify(s => s.GetNextAsync(It.IsAny<string>(), It.IsAny<string>(), default), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenPatientHasActiveConsultationWithAnotherDoctor_ShouldRejectDuplicate()
    {
        var existing = new Consultation
        {
            ConsultationId = 56,
            PatientId = 1,
            DoctorId = 20,
            VisitDate = Today,
            Status = ConsultationStatusEnum.InConsultation,
            DisplayCode = $"CN-{Today.Year}-0006",
            TokenNumber = "T-06"
        };
        SetUpExistingConsultations(existing);

        var result = await _handler.Handle(new CreateConsultationCommand("kc-staff-3", ValidRequest()), default);

        result.DuplicateFound.Should().BeTrue();
        result.ExistingConsultation!.ConsultationId.Should().Be(56);
        _consultationRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Consultation>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenDuplicateExistsAndForced_ShouldStillRejectDuplicate()
    {
        // Arrange
        var existing = new Consultation
        {
            ConsultationId = 55,
            PatientId = 1,
            DoctorId = 10,
            VisitDate = Today,
            Status = ConsultationStatusEnum.Waiting,
            DisplayCode = $"CN-{Today.Year}-0005",
            TokenNumber = "T-05"
        };
        SetUpExistingConsultations(existing);
        // Act
        var result = await _handler.Handle(new CreateConsultationCommand("kc-staff-3", ValidRequest(force: true)), default);

        // Assert
        result.DuplicateFound.Should().BeTrue();
        result.Consultation.Should().BeNull();
        _consultationRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Consultation>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenExistingConsultationIsCompleted_ShouldNotCountAsDuplicate()
    {
        // Arrange: a Completed consultation for the same patient/date doesn't
        // block a brand new check-in.
        var existing = new Consultation
        {
            ConsultationId = 55,
            PatientId = 1,
            DoctorId = 10,
            VisitDate = Today,
            Status = ConsultationStatusEnum.Completed,
            DisplayCode = $"CN-{Today.Year}-0005",
            TokenNumber = "T-05"
        };
        SetUpExistingConsultations(existing);
        _sequenceGeneratorMock.Setup(s => s.GetNextAsync(It.IsAny<string>(), It.IsAny<string>(), default)).ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(new CreateConsultationCommand("kc-staff-3", ValidRequest()), default);

        // Assert
        result.DuplicateFound.Should().BeFalse();
        _consultationRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Consultation>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUnfinishedDraftExistsAndNotForced_ShouldReturnUnfinishedDraftFoundWithoutCreating()
    {
        // Arrange: a Draft-status consultation for the same patient+doctor (from any day)
        // is a different guard (US-012) from the active-duplicate one (US-011) above.
        var draftConsultation = new Consultation
        {
            ConsultationId = 77,
            PatientId = 1,
            DoctorId = 10,
            VisitDate = Today.AddDays(-2),
            Status = ConsultationStatusEnum.Draft,
            DisplayCode = $"CN-{Today.Year}-0007",
            TokenNumber = "T-07"
        };
        SetUpExistingConsultations(draftConsultation);
        _prescriptionRepositoryMock.Setup(r => r.Query(It.IsAny<bool>())).Returns(new List<PrescriptionRecord>
        {
            new() { PrescriptionId = 500, ConsultationId = 77, PatientId = 1, DoctorId = 10, DisplayCode = $"RX-{Today.Year}-0001" }
        }.BuildMock());

        // Act
        var result = await _handler.Handle(new CreateConsultationCommand("kc-staff-3", ValidRequest(force: false)), default);

        // Assert
        result.UnfinishedDraftFound.Should().BeTrue();
        result.UnfinishedDrafts.Should().ContainSingle(d => d.PrescriptionId == 500);
        result.Consultation.Should().BeNull();
        result.DuplicateFound.Should().BeFalse();
        _consultationRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Consultation>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenUnfinishedDraftExistsAndForced_ShouldProceedToCreate()
    {
        // Arrange
        var draftConsultation = new Consultation
        {
            ConsultationId = 77,
            PatientId = 1,
            DoctorId = 10,
            VisitDate = Today.AddDays(-2),
            Status = ConsultationStatusEnum.Draft,
            DisplayCode = $"CN-{Today.Year}-0007",
            TokenNumber = "T-07"
        };
        SetUpExistingConsultations(draftConsultation);
        _sequenceGeneratorMock.Setup(s => s.GetNextAsync(It.IsAny<string>(), It.IsAny<string>(), default)).ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(new CreateConsultationCommand("kc-staff-3", ValidRequest(force: true)), default);

        // Assert
        result.UnfinishedDraftFound.Should().BeFalse();
        result.DuplicateFound.Should().BeFalse();
        _consultationRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Consultation>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }
}
