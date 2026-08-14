using FluentAssertions;
using Moq;
using SylviaNG.Prescription.Application.Common.Exceptions;
using SylviaNG.Prescription.Application.Features.Prescriptions.Commands.FinalizePrescription;
using SylviaNG.Prescription.Application.Features.Prescriptions.Models;
using SylviaNG.Prescription.Application.Interfaces.Repositories;
using SylviaNG.Prescription.Application.Mappings;
using SylviaNG.Prescription.Domain.Entities;
using SylviaNG.Prescription.Domain.Enums;
using SylviaNG.Prescription.Infrastructure.Data;
using SylviaNG.Prescription.SharedKernel.Generic;
using SylviaNG.Prescription.Tests.TestHelpers;

namespace SylviaNG.Prescription.Tests.Handlers.Prescriptions;

public class FinalizePrescriptionHandlerTests
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
    private readonly FinalizePrescriptionHandler _handler;

    private const long DoctorId = 10;
    private const long PatientId = 1;
    private const long ConsultationId = 20;
    private const long PrescriptionId = 30;
    private const long TemplateId = 40;

    public FinalizePrescriptionHandlerTests()
    {
        _unitOfWorkMock.Setup(u => u.Context).Returns(_context);

        _userRepositoryMock.Setup(r => r.GetByKeycloakIdAsync("kc-doctor-10")).ReturnsAsync(
            new User { UserId = 15, KeycloakId = "kc-doctor-10", Role = UserRoleEnum.Doctor, IsActive = true, Username = "dr.sabrina" });
        _doctorRepositoryMock.Setup(r => r.GetByUserIdAsync(15)).ReturnsAsync(FullyEquippedDoctor());
        _doctorRepositoryMock.Setup(r => r.GetByIdAsync(DoctorId)).ReturnsAsync(FullyEquippedDoctor());

        _patientRepositoryMock.Setup(r => r.GetByIdAsync(PatientId)).ReturnsAsync(new Patient { PatientId = PatientId, Name = "Rahim", Phone = "01700000000" });
        _consultationRepositoryMock.Setup(r => r.GetByIdAsync(ConsultationId)).ReturnsAsync(
            new Consultation { ConsultationId = ConsultationId, PatientId = PatientId, DoctorId = DoctorId, Status = ConsultationStatusEnum.InConsultation, DisplayCode = "CN-2026-0001", TokenNumber = "T-01" });
        _templateRepositoryMock.Setup(r => r.GetByIdAsync(TemplateId)).ReturnsAsync(
            new PrescriptionTemplate { TemplateId = TemplateId, Name = "Classic", Enabled = true, ConfigJson = "{}" });
        // Domain.Entities.HospitalSettings spelled out — bare "HospitalSettings" resolves to
        // the sibling Tests.Handlers.HospitalSettings namespace from here otherwise.
        _hospitalSettingsRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Domain.Entities.HospitalSettings>());

        _handler = new FinalizePrescriptionHandler(
            _userRepositoryMock.Object, _staffRepositoryMock.Object, _doctorRepositoryMock.Object,
            _patientRepositoryMock.Object, _consultationRepositoryMock.Object, _prescriptionRepositoryMock.Object,
            _templateRepositoryMock.Object, _hospitalSettingsRepositoryMock.Object, _unitOfWorkMock.Object);
    }

    private static Doctor FullyEquippedDoctor() => new()
    {
        DoctorId = DoctorId,
        UserId = 15,
        FullName = "Dr. Sabrina Khatun",
        Phone = "01700000000",
        SignatureBase64 = "data:image/png;base64,abc",
        PreferredTemplateId = TemplateId
    };

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

    private static SaveDraftPrescriptionRequest ValidRequest() => new()
    {
        Language = TemplateLanguageEnum.En,
        Content = new PrescriptionContent
        {
            Diagnoses = new List<DiagnosisItem> { new() { Text = "Common cold" } },
            Medicines = new List<MedicineItem> { new() { Medicine = "Napa", Strength = "500mg" } },
            History = new List<string> { "Fever for 3 days" }
        }
    };

    [Fact]
    public async Task Handle_WithValidPrescriptionAndDoctorReady_ShouldFinalizeAndCompleteConsultationAndUpdateSavedHistory()
    {
        // Arrange
        _prescriptionRepositoryMock.Setup(r => r.GetByIdAsync(PrescriptionId)).ReturnsAsync(DraftPrescription());

        // Act
        var result = await _handler.Handle(new FinalizePrescriptionCommand("kc-doctor-10", PrescriptionId, ValidRequest()), default);

        // Assert
        result.Status.Should().Be(PrescriptionStatusEnum.Finalized);
        result.FinalizedAt.Should().NotBeNull();

        _prescriptionRepositoryMock.Verify(r => r.Update(It.Is<PrescriptionRecord>(p =>
            p.Status == PrescriptionStatusEnum.Finalized && p.FinalizedAt != null)), Times.Once);
        _consultationRepositoryMock.Verify(r => r.Update(It.Is<Consultation>(c => c.Status == ConsultationStatusEnum.Completed)), Times.Once);
        _patientRepositoryMock.Verify(r => r.Update(It.Is<Patient>(p => p.SavedHistory == "Fever for 3 days")), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Theory]
    [InlineData(true, true, false, false)]  // missing signature
    [InlineData(true, true, true, false)]   // missing preferred template
    public async Task Handle_WhenDoctorMissingSignatureOrTemplate_ShouldThrowBadRequestException(
        bool hasDiagnosis, bool hasMedicine, bool hasSignature, bool hasTemplate)
    {
        // Arrange
        _prescriptionRepositoryMock.Setup(r => r.GetByIdAsync(PrescriptionId)).ReturnsAsync(DraftPrescription());
        var doctor = FullyEquippedDoctor();
        if (!hasSignature) doctor.SignatureBase64 = null;
        if (!hasTemplate) doctor.PreferredTemplateId = null;
        _doctorRepositoryMock.Setup(r => r.GetByIdAsync(DoctorId)).ReturnsAsync(doctor);

        var request = ValidRequest();
        if (!hasDiagnosis) request.Content.Diagnoses = new List<DiagnosisItem>();
        if (!hasMedicine) request.Content.Medicines = new List<MedicineItem>();

        // Act
        var act = () => _handler.Handle(new FinalizePrescriptionCommand("kc-doctor-10", PrescriptionId, request), default);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
        _prescriptionRepositoryMock.Verify(r => r.Update(It.IsAny<PrescriptionRecord>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenMissingDiagnosisAndMedicine_ShouldListBothInOneException()
    {
        // Arrange: US-026 — all missing items reported together, not one at a time.
        _prescriptionRepositoryMock.Setup(r => r.GetByIdAsync(PrescriptionId)).ReturnsAsync(DraftPrescription());
        var request = ValidRequest();
        request.Content.Diagnoses = new List<DiagnosisItem>();
        request.Content.Medicines = new List<MedicineItem>();

        // Act
        var act = () => _handler.Handle(new FinalizePrescriptionCommand("kc-doctor-10", PrescriptionId, request), default);

        // Assert
        var ex = await act.Should().ThrowAsync<BadRequestException>();
        ex.Which.Message.Should().Contain("diagnosis").And.Contain("medicine");
    }

    [Fact]
    public async Task Handle_WithDuplicateMedicines_ShouldThrowBadRequestException()
    {
        // Arrange
        _prescriptionRepositoryMock.Setup(r => r.GetByIdAsync(PrescriptionId)).ReturnsAsync(DraftPrescription());
        var request = ValidRequest();
        request.Content.Medicines = new List<MedicineItem>
        {
            new() { Medicine = "Napa", Strength = "500mg" },
            new() { Medicine = "napa", Strength = "500MG" }
        };

        // Act
        var act = () => _handler.Handle(new FinalizePrescriptionCommand("kc-doctor-10", PrescriptionId, request), default);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task Handle_WhenPrescriptionAlreadyFinalized_ShouldThrowBadRequestException()
    {
        // Arrange
        var finalized = DraftPrescription();
        finalized.Status = PrescriptionStatusEnum.Finalized;
        _prescriptionRepositoryMock.Setup(r => r.GetByIdAsync(PrescriptionId)).ReturnsAsync(finalized);

        // Act
        var act = () => _handler.Handle(new FinalizePrescriptionCommand("kc-doctor-10", PrescriptionId, ValidRequest()), default);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task Handle_WhenPrescriptionBelongsToAnotherDoctor_ShouldThrowNotFoundException()
    {
        // Arrange
        var otherDoctorsPrescription = DraftPrescription();
        otherDoctorsPrescription.DoctorId = 999;
        _prescriptionRepositoryMock.Setup(r => r.GetByIdAsync(PrescriptionId)).ReturnsAsync(otherDoctorsPrescription);

        // Act
        var act = () => _handler.Handle(new FinalizePrescriptionCommand("kc-doctor-10", PrescriptionId, ValidRequest()), default);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }
}
