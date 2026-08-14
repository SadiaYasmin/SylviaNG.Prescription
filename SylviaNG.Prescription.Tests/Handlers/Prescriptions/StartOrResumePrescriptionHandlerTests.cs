using FluentAssertions;
using MockQueryable;
using Moq;
using SylviaNG.Prescription.Application.Common.Exceptions;
using SylviaNG.Prescription.Application.Common.Services;
using SylviaNG.Prescription.Application.Features.Prescriptions.Commands.StartOrResumePrescription;
using SylviaNG.Prescription.Application.Features.Prescriptions.Models;
using SylviaNG.Prescription.Application.Interfaces.Repositories;
using SylviaNG.Prescription.Domain.Entities;
using SylviaNG.Prescription.Domain.Enums;
using SylviaNG.Prescription.Infrastructure.Data;
using SylviaNG.Prescription.SharedKernel.Generic;
using SylviaNG.Prescription.SharedKernel.Utils;
using SylviaNG.Prescription.Tests.TestHelpers;

namespace SylviaNG.Prescription.Tests.Handlers.Prescriptions;

public class StartOrResumePrescriptionHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IStaffRepository> _staffRepositoryMock = new();
    private readonly Mock<IDoctorRepository> _doctorRepositoryMock = new();
    private readonly Mock<IPatientRepository> _patientRepositoryMock = new();
    private readonly Mock<IConsultationRepository> _consultationRepositoryMock = new();
    private readonly Mock<IPrescriptionRepository> _prescriptionRepositoryMock = new();
    private readonly Mock<ITemplateRepository> _templateRepositoryMock = new();
    private readonly Mock<IHospitalSettingsRepository> _hospitalSettingsRepositoryMock = new();
    private readonly Mock<ISequenceGenerator> _sequenceGeneratorMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly ApplicationDBContext _context = InMemoryDbContextFactory.Create();
    private readonly StartOrResumePrescriptionHandler _handler;

    private const long DoctorId = 10;
    private const long PatientId = 1;
    private const long TemplateId = 40;

    private static readonly DateOnly Today = DateTimeUtility.TodayLocal();

    public StartOrResumePrescriptionHandlerTests()
    {
        _unitOfWorkMock.Setup(u => u.Context).Returns(_context);

        _userRepositoryMock.Setup(r => r.GetByKeycloakIdAsync("kc-doctor-10")).ReturnsAsync(
            new User { UserId = 15, KeycloakId = "kc-doctor-10", Role = UserRoleEnum.Doctor, IsActive = true, Username = "dr.sabrina" });
        _doctorRepositoryMock.Setup(r => r.GetByUserIdAsync(15)).ReturnsAsync(new Doctor { DoctorId = DoctorId, UserId = 15, FullName = "Dr. Sabrina Khatun", Phone = "01700000000" });
        _doctorRepositoryMock.Setup(r => r.GetByIdAsync(DoctorId)).ReturnsAsync(
            new Doctor { DoctorId = DoctorId, UserId = 15, FullName = "Dr. Sabrina Khatun", Phone = "01700000000", PreferredTemplateId = TemplateId });

        _patientRepositoryMock.Setup(r => r.GetByIdAsync(PatientId)).ReturnsAsync(
            new Patient { PatientId = PatientId, Name = "Rahim", Phone = "01700000001", SavedHistory = "Hypertension" });

        _templateRepositoryMock.Setup(r => r.GetByIdAsync(TemplateId)).ReturnsAsync(
            new PrescriptionTemplate { TemplateId = TemplateId, Name = "Classic", Enabled = true, ConfigJson = "{}" });
        _templateRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<PrescriptionTemplate>
        {
            new() { TemplateId = TemplateId, Name = "Classic", Enabled = true, IsSystemDefault = true, ConfigJson = "{}" }
        });
        _hospitalSettingsRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Domain.Entities.HospitalSettings>());

        _consultationRepositoryMock.Setup(r => r.Query(It.IsAny<bool>())).Returns(new List<Consultation>().BuildMock());
        _prescriptionRepositoryMock.Setup(r => r.Query(It.IsAny<bool>())).Returns(new List<PrescriptionRecord>().BuildMock());

        _sequenceGeneratorMock.Setup(s => s.GetNextAsync(It.IsAny<string>(), It.IsAny<string>(), default)).ReturnsAsync(1);
        _consultationRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Consultation>()))
            .Callback<Consultation>(c => c.ConsultationId = 100)
            .Returns(Task.CompletedTask);
        _prescriptionRepositoryMock.Setup(r => r.AddAsync(It.IsAny<PrescriptionRecord>()))
            .Callback<PrescriptionRecord>(p => p.PrescriptionId = 200)
            .Returns(Task.CompletedTask);

        _handler = new StartOrResumePrescriptionHandler(
            _userRepositoryMock.Object, _staffRepositoryMock.Object, _doctorRepositoryMock.Object,
            _patientRepositoryMock.Object, _consultationRepositoryMock.Object, _prescriptionRepositoryMock.Object,
            _templateRepositoryMock.Object, _hospitalSettingsRepositoryMock.Object,
            _sequenceGeneratorMock.Object, _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_QuickCreateWithNoGuardHits_ShouldCreateConsultationAndBlankPrescriptionPreloadedWithSavedHistory()
    {
        // Act
        var result = await _handler.Handle(
            new StartOrResumePrescriptionCommand("kc-doctor-10", new StartOrResumePrescriptionRequest { PatientId = PatientId }), default);

        // Assert
        result.DuplicateActiveFound.Should().BeFalse();
        result.UnfinishedDraftFound.Should().BeFalse();
        result.Document.Should().NotBeNull();
        result.Document!.Status.Should().Be(PrescriptionStatusEnum.Draft);
        result.Document.Content.History.Should().ContainSingle().Which.Should().Be("Hypertension");
        result.Document.TemplateId.Should().Be(TemplateId);

        _consultationRepositoryMock.Verify(r => r.AddAsync(It.Is<Consultation>(c =>
            c.PatientId == PatientId && c.DoctorId == DoctorId && c.RegisteredByStaffId == null
            && c.Status == ConsultationStatusEnum.InConsultation)), Times.Once);
        _prescriptionRepositoryMock.Verify(r => r.AddAsync(It.Is<PrescriptionRecord>(p =>
            p.PatientId == PatientId && p.DoctorId == DoctorId && p.TemplateId == TemplateId)), Times.Once);
    }

    [Fact]
    public async Task Handle_QuickCreateWhenActiveConsultationExists_ShouldReturnDuplicateActiveFoundWithoutCreating()
    {
        // Arrange
        var existing = new Consultation
        {
            ConsultationId = 55, PatientId = PatientId, DoctorId = 99, VisitDate = Today,
            Status = ConsultationStatusEnum.Waiting, DisplayCode = $"CN-{Today.Year}-0005", TokenNumber = "T-05"
        };
        _consultationRepositoryMock.Setup(r => r.Query(It.IsAny<bool>())).Returns(new[] { existing }.BuildMock());

        // Act
        var result = await _handler.Handle(
            new StartOrResumePrescriptionCommand("kc-doctor-10", new StartOrResumePrescriptionRequest { PatientId = PatientId }), default);

        // Assert
        result.DuplicateActiveFound.Should().BeTrue();
        result.ExistingActiveConsultation!.ConsultationId.Should().Be(55);
        result.Document.Should().BeNull();
        _consultationRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Consultation>()), Times.Never);
    }

    [Fact]
    public async Task Handle_QuickCreateWhenUnfinishedDraftExistsWithThisDoctor_ShouldReturnUnfinishedDraftFound()
    {
        // Arrange
        var draftConsultation = new Consultation
        {
            ConsultationId = 77, PatientId = PatientId, DoctorId = DoctorId, VisitDate = Today.AddDays(-3),
            Status = ConsultationStatusEnum.Draft, DisplayCode = $"CN-{Today.Year}-0007", TokenNumber = "T-07"
        };
        _consultationRepositoryMock.Setup(r => r.Query(It.IsAny<bool>())).Returns(new[] { draftConsultation }.BuildMock());
        _prescriptionRepositoryMock.Setup(r => r.Query(It.IsAny<bool>())).Returns(new List<PrescriptionRecord>
        {
            new() { PrescriptionId = 500, ConsultationId = 77, PatientId = PatientId, DoctorId = DoctorId, DisplayCode = $"RX-{Today.Year}-0001" }
        }.BuildMock());

        // Act
        var result = await _handler.Handle(
            new StartOrResumePrescriptionCommand("kc-doctor-10", new StartOrResumePrescriptionRequest { PatientId = PatientId }), default);

        // Assert
        result.UnfinishedDraftFound.Should().BeTrue();
        result.UnfinishedDrafts.Should().ContainSingle(d => d.PrescriptionId == 500);
        result.Document.Should().BeNull();
        _consultationRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Consultation>()), Times.Never);
    }

    [Fact]
    public async Task Handle_QuickCreateWithForce_ShouldProceedPastBothGuards()
    {
        // Arrange
        var draftConsultation = new Consultation
        {
            ConsultationId = 77, PatientId = PatientId, DoctorId = DoctorId, VisitDate = Today.AddDays(-3),
            Status = ConsultationStatusEnum.Draft, DisplayCode = $"CN-{Today.Year}-0007", TokenNumber = "T-07"
        };
        _consultationRepositoryMock.Setup(r => r.Query(It.IsAny<bool>())).Returns(new[] { draftConsultation }.BuildMock());

        // Act
        var result = await _handler.Handle(
            new StartOrResumePrescriptionCommand("kc-doctor-10", new StartOrResumePrescriptionRequest { PatientId = PatientId, Force = true }), default);

        // Assert
        result.UnfinishedDraftFound.Should().BeFalse();
        result.Document.Should().NotBeNull();
        _consultationRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Consultation>()), Times.Once);
    }

    [Fact]
    public async Task Handle_OpenFromQueue_ShouldTransitionWaitingToInConsultationAndCreateBlankPrescription()
    {
        // Arrange
        var consultation = new Consultation
        {
            ConsultationId = 88, PatientId = PatientId, DoctorId = DoctorId, VisitDate = Today,
            Status = ConsultationStatusEnum.Waiting, DisplayCode = $"CN-{Today.Year}-0008", TokenNumber = "T-08"
        };
        _consultationRepositoryMock.Setup(r => r.GetByIdAsync(88)).ReturnsAsync(consultation);

        // Act
        var result = await _handler.Handle(
            new StartOrResumePrescriptionCommand("kc-doctor-10", new StartOrResumePrescriptionRequest { ConsultationId = 88 }), default);

        // Assert
        result.Document.Should().NotBeNull();
        _consultationRepositoryMock.Verify(r => r.Update(It.Is<Consultation>(c => c.ConsultationId == 88 && c.Status == ConsultationStatusEnum.InConsultation)), Times.Once);
        _prescriptionRepositoryMock.Verify(r => r.AddAsync(It.IsAny<PrescriptionRecord>()), Times.Once);
    }

    [Fact]
    public async Task Handle_OpenFromQueueWhenConsultationBelongsToAnotherDoctor_ShouldThrowNotFoundException()
    {
        // Arrange
        var consultation = new Consultation
        {
            ConsultationId = 88, PatientId = PatientId, DoctorId = 999, VisitDate = Today,
            Status = ConsultationStatusEnum.Waiting, DisplayCode = $"CN-{Today.Year}-0008", TokenNumber = "T-08"
        };
        _consultationRepositoryMock.Setup(r => r.GetByIdAsync(88)).ReturnsAsync(consultation);

        // Act
        var act = () => _handler.Handle(
            new StartOrResumePrescriptionCommand("kc-doctor-10", new StartOrResumePrescriptionRequest { ConsultationId = 88 }), default);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_ResumeDraftByPrescriptionId_ShouldReturnExistingDocumentWithoutCreatingAnother()
    {
        // Arrange
        var consultation = new Consultation
        {
            ConsultationId = 77, PatientId = PatientId, DoctorId = DoctorId, VisitDate = Today.AddDays(-1),
            Status = ConsultationStatusEnum.Draft, DisplayCode = $"CN-{Today.Year}-0007", TokenNumber = "T-07"
        };
        var prescription = new PrescriptionRecord
        {
            PrescriptionId = 500, ConsultationId = 77, PatientId = PatientId, DoctorId = DoctorId,
            TemplateId = TemplateId, DisplayCode = $"RX-{Today.Year}-0001", Status = PrescriptionStatusEnum.Draft
        };
        _prescriptionRepositoryMock.Setup(r => r.GetByIdAsync(500)).ReturnsAsync(prescription);
        _consultationRepositoryMock.Setup(r => r.GetByIdAsync(77)).ReturnsAsync(consultation);

        // Act
        var result = await _handler.Handle(
            new StartOrResumePrescriptionCommand("kc-doctor-10", new StartOrResumePrescriptionRequest { PrescriptionId = 500 }), default);

        // Assert
        result.Document!.PrescriptionId.Should().Be(500);
        _consultationRepositoryMock.Verify(r => r.Update(It.Is<Consultation>(c => c.Status == ConsultationStatusEnum.InConsultation)), Times.Once);
        _prescriptionRepositoryMock.Verify(r => r.AddAsync(It.IsAny<PrescriptionRecord>()), Times.Never);
        _consultationRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Consultation>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ResumeDraftBelongingToAnotherDoctor_ShouldThrowNotFoundException()
    {
        // Arrange
        var prescription = new PrescriptionRecord
        {
            PrescriptionId = 500, ConsultationId = 77, PatientId = PatientId, DoctorId = 999,
            TemplateId = TemplateId, DisplayCode = $"RX-{Today.Year}-0001", Status = PrescriptionStatusEnum.Draft
        };
        _prescriptionRepositoryMock.Setup(r => r.GetByIdAsync(500)).ReturnsAsync(prescription);

        // Act
        var act = () => _handler.Handle(
            new StartOrResumePrescriptionCommand("kc-doctor-10", new StartOrResumePrescriptionRequest { PrescriptionId = 500 }), default);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }
}
