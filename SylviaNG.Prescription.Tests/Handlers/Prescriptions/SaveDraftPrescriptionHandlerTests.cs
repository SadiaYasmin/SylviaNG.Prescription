using FluentAssertions;
using Moq;
using SylviaNG.Prescription.Application.Common.Exceptions;
using SylviaNG.Prescription.Application.Features.Prescriptions.Commands.SaveDraftPrescription;
using SylviaNG.Prescription.Application.Features.Prescriptions.Models;
using SylviaNG.Prescription.Application.Interfaces.Repositories;
using SylviaNG.Prescription.Domain.Entities;
using SylviaNG.Prescription.Domain.Enums;
using SylviaNG.Prescription.Infrastructure.Data;
using SylviaNG.Prescription.SharedKernel.Generic;
using SylviaNG.Prescription.Tests.TestHelpers;

namespace SylviaNG.Prescription.Tests.Handlers.Prescriptions;

public class SaveDraftPrescriptionHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IStaffRepository> _staffRepositoryMock = new();
    private readonly Mock<IDoctorRepository> _doctorRepositoryMock = new();
    private readonly Mock<IPatientRepository> _patientRepositoryMock = new();
    private readonly Mock<IConsultationRepository> _consultationRepositoryMock = new();
    private readonly Mock<IPrescriptionRepository> _prescriptionRepositoryMock = new();
    private readonly Mock<ITemplateRepository> _templateRepositoryMock = new();
    private readonly Mock<IHospitalSettingsRepository> _hospitalSettingsRepositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly ApplicationDBContext _context = InMemoryDbContextFactory.Create();
    private readonly SaveDraftPrescriptionHandler _handler;

    private const long DoctorId = 10;
    private const long PatientId = 1;
    private const long ConsultationId = 20;
    private const long PrescriptionId = 30;
    private const long TemplateId = 40;

    public SaveDraftPrescriptionHandlerTests()
    {
        _unitOfWorkMock.Setup(u => u.Context).Returns(_context);

        _userRepositoryMock.Setup(r => r.GetByKeycloakIdAsync("kc-doctor-10")).ReturnsAsync(
            new User { UserId = 15, KeycloakId = "kc-doctor-10", Role = UserRoleEnum.Doctor, IsActive = true, Username = "dr.sabrina" });
        _doctorRepositoryMock.Setup(r => r.GetByUserIdAsync(15)).ReturnsAsync(new Doctor { DoctorId = DoctorId, UserId = 15, FullName = "Dr. Sabrina Khatun", Phone = "01700000000" });

        _patientRepositoryMock.Setup(r => r.GetByIdAsync(PatientId)).ReturnsAsync(new Patient { PatientId = PatientId, Name = "Rahim", Phone = "01700000001" });
        _doctorRepositoryMock.Setup(r => r.GetByIdAsync(DoctorId)).ReturnsAsync(new Doctor { DoctorId = DoctorId, UserId = 15, FullName = "Dr. Sabrina Khatun", Phone = "01700000000" });
        _consultationRepositoryMock.Setup(r => r.GetByIdAsync(ConsultationId)).ReturnsAsync(
            new Consultation { ConsultationId = ConsultationId, PatientId = PatientId, DoctorId = DoctorId, Status = ConsultationStatusEnum.InConsultation, DisplayCode = "CN-2026-0001", TokenNumber = "T-01" });
        _templateRepositoryMock.Setup(r => r.GetByIdAsync(TemplateId)).ReturnsAsync(
            new PrescriptionTemplate { TemplateId = TemplateId, Name = "Classic", Enabled = true, ConfigJson = "{}" });
        _hospitalSettingsRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Domain.Entities.HospitalSettings>());

        _handler = new SaveDraftPrescriptionHandler(
            _userRepositoryMock.Object, _staffRepositoryMock.Object, _doctorRepositoryMock.Object,
            _patientRepositoryMock.Object, _consultationRepositoryMock.Object, _prescriptionRepositoryMock.Object,
            _templateRepositoryMock.Object, _hospitalSettingsRepositoryMock.Object, _unitOfWorkMock.Object);
    }

    private static PrescriptionRecord DraftPrescription() => new()
    {
        PrescriptionId = PrescriptionId,
        DisplayCode = "RX-2026-0001",
        ConsultationId = ConsultationId,
        PatientId = PatientId,
        DoctorId = DoctorId,
        TemplateId = TemplateId,
        Status = PrescriptionStatusEnum.Draft,
        Language = TemplateLanguageEnum.En
    };

    [Fact]
    public async Task Handle_WithValidPayload_ShouldPersistContentStampSavedAtAndSetConsultationDraft()
    {
        // Arrange
        _prescriptionRepositoryMock.Setup(r => r.GetByIdAsync(PrescriptionId)).ReturnsAsync(DraftPrescription());
        var request = new SaveDraftPrescriptionRequest
        {
            Language = TemplateLanguageEnum.Bn,
            Content = new PrescriptionContent { ChiefComplaints = new List<string> { "Fever" } }
        };

        // Act
        var result = await _handler.Handle(new SaveDraftPrescriptionCommand("kc-doctor-10", PrescriptionId, request), default);

        // Assert
        result.Language.Should().Be(TemplateLanguageEnum.Bn);
        result.Content.ChiefComplaints.Should().ContainSingle().Which.Should().Be("Fever");
        result.SavedAt.Should().NotBeNull();

        _prescriptionRepositoryMock.Verify(r => r.Update(It.Is<PrescriptionRecord>(p => p.SavedAt != null && p.Status == PrescriptionStatusEnum.Draft)), Times.Once);
        _consultationRepositoryMock.Verify(r => r.Update(It.Is<Consultation>(c => c.Status == ConsultationStatusEnum.Draft)), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldNeverMarkTheConsultationCompleted()
    {
        // US-017 invariant, other direction from FinalizePrescriptionHandlerTests'
        // "finalize completes the consultation" coverage: saving a draft must never
        // transition the consultation to Completed — only finalizing a prescription may.
        _prescriptionRepositoryMock.Setup(r => r.GetByIdAsync(PrescriptionId)).ReturnsAsync(DraftPrescription());
        var request = new SaveDraftPrescriptionRequest { Language = TemplateLanguageEnum.En, Content = new PrescriptionContent() };

        await _handler.Handle(new SaveDraftPrescriptionCommand("kc-doctor-10", PrescriptionId, request), default);

        _consultationRepositoryMock.Verify(r => r.Update(It.Is<Consultation>(c => c.Status != ConsultationStatusEnum.Completed)), Times.Once);
    }

    [Fact]
    public async Task Handle_WithDuplicateMedicines_ShouldThrowBadRequestExceptionAndNotSave()
    {
        // Arrange
        _prescriptionRepositoryMock.Setup(r => r.GetByIdAsync(PrescriptionId)).ReturnsAsync(DraftPrescription());
        var request = new SaveDraftPrescriptionRequest
        {
            Language = TemplateLanguageEnum.En,
            Content = new PrescriptionContent
            {
                Medicines = new List<MedicineItem>
                {
                    new() { Medicine = "Napa", Strength = "500mg" },
                    new() { Medicine = "Napa", Strength = "500mg" }
                }
            }
        };

        // Act
        var act = () => _handler.Handle(new SaveDraftPrescriptionCommand("kc-doctor-10", PrescriptionId, request), default);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenPrescriptionBelongsToAnotherDoctor_ShouldThrowNotFoundException()
    {
        // Arrange
        var otherDoctorsPrescription = DraftPrescription();
        otherDoctorsPrescription.DoctorId = 999;
        _prescriptionRepositoryMock.Setup(r => r.GetByIdAsync(PrescriptionId)).ReturnsAsync(otherDoctorsPrescription);

        // Act
        var act = () => _handler.Handle(new SaveDraftPrescriptionCommand("kc-doctor-10", PrescriptionId, new SaveDraftPrescriptionRequest()), default);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenPrescriptionAlreadyFinalized_ShouldThrowBadRequestException()
    {
        // Arrange: a finalized prescription is permanently read-only through authoring.
        var finalized = DraftPrescription();
        finalized.Status = PrescriptionStatusEnum.Finalized;
        _prescriptionRepositoryMock.Setup(r => r.GetByIdAsync(PrescriptionId)).ReturnsAsync(finalized);

        // Act
        var act = () => _handler.Handle(new SaveDraftPrescriptionCommand("kc-doctor-10", PrescriptionId, new SaveDraftPrescriptionRequest()), default);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
    }
}
