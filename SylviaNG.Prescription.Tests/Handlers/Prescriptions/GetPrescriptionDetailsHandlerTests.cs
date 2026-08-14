using FluentAssertions;
using Moq;
using SylviaNG.Prescription.Application.Common.Exceptions;
using SylviaNG.Prescription.Application.Features.Prescriptions.Queries.GetPrescriptionDetails;
using SylviaNG.Prescription.Application.Interfaces.Repositories;
using SylviaNG.Prescription.Domain.Entities;
using SylviaNG.Prescription.Domain.Enums;
using SylviaNG.Prescription.Infrastructure.Data;
using SylviaNG.Prescription.SharedKernel.Generic;
using SylviaNG.Prescription.Tests.TestHelpers;

namespace SylviaNG.Prescription.Tests.Handlers.Prescriptions;

/// <summary>
/// Covers PrescriptionVisibilityScope's non-owning-Doctor/Staff/Admin branches through the
/// real query handler (rather than testing the static scope class in isolation), matching
/// how PatientVisibilityScope is exercised elsewhere in this test project.
/// </summary>
public class GetPrescriptionDetailsHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IStaffRepository> _staffRepositoryMock = new();
    private readonly Mock<IDoctorRepository> _doctorRepositoryMock = new();
    private readonly Mock<IPatientRepository> _patientRepositoryMock = new();
    private readonly Mock<IPrescriptionRepository> _prescriptionRepositoryMock = new();
    private readonly Mock<ITemplateRepository> _templateRepositoryMock = new();
    private readonly Mock<IHospitalSettingsRepository> _hospitalSettingsRepositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly ApplicationDBContext _context = InMemoryDbContextFactory.Create();
    private readonly GetPrescriptionDetailsHandler _handler;

    private const long AuthoringDoctorId = 10;
    private const long OtherDoctorId = 11;
    private const long CareTeamStaffId = 3;
    private const long OtherStaffId = 4;
    private const long PatientId = 1;
    private const long PrescriptionId = 30;
    private const long TemplateId = 40;

    public GetPrescriptionDetailsHandlerTests()
    {
        _unitOfWorkMock.Setup(u => u.Context).Returns(_context);

        // Patient 1 was registered by staff 3, who is assigned to doctor 10 — the
        // authoring doctor's own care team.
        _context.Patients.Add(new Patient { PatientId = PatientId, Name = "Rahim", Phone = "01700000001", RegisteredByStaffId = CareTeamStaffId });
        _context.StaffDoctors.Add(new StaffDoctor { StaffDoctorId = 1, StaffId = CareTeamStaffId, DoctorId = AuthoringDoctorId });
        _context.Prescriptions.Add(new PrescriptionRecord
        {
            PrescriptionId = PrescriptionId, DisplayCode = "RX-2026-0001", ConsultationId = 20,
            PatientId = PatientId, DoctorId = AuthoringDoctorId, TemplateId = TemplateId, Status = PrescriptionStatusEnum.Finalized
        });
        _context.SaveChanges();

        _patientRepositoryMock.Setup(r => r.GetByIdAsync(PatientId)).ReturnsAsync(new Patient { PatientId = PatientId, Name = "Rahim", Phone = "01700000001" });
        _doctorRepositoryMock.Setup(r => r.GetByIdAsync(AuthoringDoctorId)).ReturnsAsync(new Doctor { DoctorId = AuthoringDoctorId, UserId = 15, FullName = "Dr. Sabrina", Phone = "01700000000" });
        _templateRepositoryMock.Setup(r => r.GetByIdAsync(TemplateId)).ReturnsAsync(new PrescriptionTemplate { TemplateId = TemplateId, Name = "Classic", Enabled = true, ConfigJson = "{}" });
        _hospitalSettingsRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Domain.Entities.HospitalSettings>());
        _prescriptionRepositoryMock.Setup(r => r.GetByIdAsync(PrescriptionId)).ReturnsAsync(_context.Prescriptions.First());

        _handler = new GetPrescriptionDetailsHandler(
            _userRepositoryMock.Object, _staffRepositoryMock.Object, _doctorRepositoryMock.Object,
            _patientRepositoryMock.Object, _prescriptionRepositoryMock.Object,
            _templateRepositoryMock.Object, _hospitalSettingsRepositoryMock.Object, _unitOfWorkMock.Object);
    }

    private void SetUpCaller(UserRoleEnum role, long? staffId, long? doctorId)
    {
        _userRepositoryMock.Setup(r => r.GetByKeycloakIdAsync("kc-caller")).ReturnsAsync(
            new User { UserId = 99, KeycloakId = "kc-caller", Role = role, IsActive = true, Username = "caller" });
        if (role == UserRoleEnum.Staff)
            _staffRepositoryMock.Setup(r => r.GetByUserIdAsync(99)).ReturnsAsync(new Staff { StaffId = staffId!.Value, UserId = 99, FullName = "Staff" });
        if (role == UserRoleEnum.Doctor)
            _doctorRepositoryMock.Setup(r => r.GetByUserIdAsync(99)).ReturnsAsync(new Doctor { DoctorId = doctorId!.Value, UserId = 99, FullName = "Doctor", Phone = "01700000000" });
    }

    [Fact]
    public async Task Handle_AsAdmin_ShouldReturnPrescription()
    {
        SetUpCaller(UserRoleEnum.Admin, null, null);

        var result = await _handler.Handle(new GetPrescriptionDetailsQuery("kc-caller", PrescriptionId), default);

        result.PrescriptionId.Should().Be(PrescriptionId);
    }

    [Fact]
    public async Task Handle_AsAuthoringDoctor_ShouldReturnPrescription()
    {
        SetUpCaller(UserRoleEnum.Doctor, null, AuthoringDoctorId);

        var result = await _handler.Handle(new GetPrescriptionDetailsQuery("kc-caller", PrescriptionId), default);

        result.PrescriptionId.Should().Be(PrescriptionId);
    }

    [Fact]
    public async Task Handle_AsUnrelatedDoctor_ShouldThrowNotFoundException()
    {
        SetUpCaller(UserRoleEnum.Doctor, null, OtherDoctorId);

        var act = () => _handler.Handle(new GetPrescriptionDetailsQuery("kc-caller", PrescriptionId), default);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_AsCareTeamStaffWhoRegisteredThePatient_ShouldReturnPrescription()
    {
        SetUpCaller(UserRoleEnum.Staff, CareTeamStaffId, null);

        var result = await _handler.Handle(new GetPrescriptionDetailsQuery("kc-caller", PrescriptionId), default);

        result.PrescriptionId.Should().Be(PrescriptionId);
    }

    [Fact]
    public async Task Handle_AsUnrelatedStaff_ShouldThrowNotFoundException()
    {
        SetUpCaller(UserRoleEnum.Staff, OtherStaffId, null);

        var act = () => _handler.Handle(new GetPrescriptionDetailsQuery("kc-caller", PrescriptionId), default);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
