using FluentAssertions;
using MockQueryable;
using Moq;
using SylviaNG.Prescription.Application.Features.Analytics.Queries.GetMyDoctorAnalytics;
using SylviaNG.Prescription.Application.Features.Prescriptions.Models;
using SylviaNG.Prescription.Application.Interfaces.Repositories;
using SylviaNG.Prescription.Application.Mappings;
using SylviaNG.Prescription.Domain.Entities;
using SylviaNG.Prescription.Domain.Enums;
using SylviaNG.Prescription.Infrastructure.Data;
using SylviaNG.Prescription.SharedKernel.Generic;
using SylviaNG.Prescription.Tests.TestHelpers;

namespace SylviaNG.Prescription.Tests.Handlers.Analytics;

public class GetMyDoctorAnalyticsHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IStaffRepository> _staffRepositoryMock = new();
    private readonly Mock<IDoctorRepository> _doctorRepositoryMock = new();
    private readonly Mock<IPatientRepository> _patientRepositoryMock = new();
    private readonly Mock<IConsultationRepository> _consultationRepositoryMock = new();
    private readonly Mock<IPrescriptionRepository> _prescriptionRepositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly ApplicationDBContext _context = InMemoryDbContextFactory.Create();
    private readonly GetMyDoctorAnalyticsHandler _handler;

    public GetMyDoctorAnalyticsHandlerTests()
    {
        _unitOfWorkMock.Setup(u => u.Context).Returns(_context);

        _userRepositoryMock.Setup(r => r.GetByKeycloakIdAsync("kc-doc-10")).ReturnsAsync(
            new User { UserId = 50, KeycloakId = "kc-doc-10", Role = UserRoleEnum.Doctor, IsActive = true, Username = "doc10" });
        _doctorRepositoryMock.Setup(r => r.GetByUserIdAsync(50)).ReturnsAsync(new Doctor { DoctorId = 10, UserId = 50, FullName = "Dr. Ten" });

        // Staff 3 is assigned to doctor 10; staff 4 is not — this is what makes patient 1
        // (registered by staff 3) visible and patient 2 (registered by staff 4) invisible.
        _context.StaffDoctors.Add(new StaffDoctor { StaffId = 3, DoctorId = 10 });
        _context.SaveChanges();

        _patientRepositoryMock.Setup(r => r.Query(It.IsAny<bool>())).Returns(new List<Patient>
        {
            new() { PatientId = 1, RegisteredByStaffId = 3 },
            new() { PatientId = 2, RegisteredByStaffId = 4 },
        }.BuildMock());

        _consultationRepositoryMock.Setup(r => r.Query(It.IsAny<bool>())).Returns(new List<Consultation>
        {
            new() { DoctorId = 10, PatientId = 1 },
            new() { DoctorId = 99, PatientId = 2 }, // a different doctor's consultation — must not count
        }.BuildMock());

        var ownFinalized = new PrescriptionRecord { DoctorId = 10, Status = PrescriptionStatusEnum.Finalized };
        ownFinalized.SetMedicines(new List<MedicineItem> { new() { Medicine = "Napa", Strength = "500mg" } });
        _prescriptionRepositoryMock.Setup(r => r.Query(It.IsAny<bool>())).Returns(new List<PrescriptionRecord>
        {
            new() { DoctorId = 10, Status = PrescriptionStatusEnum.Draft },
            ownFinalized,
            new() { DoctorId = 99, Status = PrescriptionStatusEnum.Finalized }, // a different doctor's Rx — must not count
        }.BuildMock());

        _handler = new GetMyDoctorAnalyticsHandler(
            _userRepositoryMock.Object, _staffRepositoryMock.Object, _doctorRepositoryMock.Object,
            _patientRepositoryMock.Object, _consultationRepositoryMock.Object, _prescriptionRepositoryMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldScopeEverythingToTheCallingDoctorOnly()
    {
        var result = await _handler.Handle(new GetMyDoctorAnalyticsQuery("kc-doc-10"), default);

        result.OwnPatientCount.Should().Be(1); // only patient 1, via PatientVisibilityScope's StaffDoctor join
        result.PatientsConsulted.Should().Be(1);
        result.DraftPrescriptionCount.Should().Be(1);
        result.FinalizedPrescriptionCount.Should().Be(1);
        result.AssignedStaffCount.Should().Be(1);
        result.TopMedicines.Should().ContainSingle(m => m.Name == "Napa");
    }
}
