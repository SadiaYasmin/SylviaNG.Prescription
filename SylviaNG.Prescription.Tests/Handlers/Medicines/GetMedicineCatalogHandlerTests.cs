using FluentAssertions;
using MockQueryable;
using Moq;
using SylviaNG.Prescription.Application.Features.Medicines.Queries.GetMedicineCatalog;
using SylviaNG.Prescription.Application.Features.Prescriptions.Models;
using SylviaNG.Prescription.Application.Interfaces.Repositories;
using SylviaNG.Prescription.Application.Mappings;
using SylviaNG.Prescription.Domain.Entities;
using SylviaNG.Prescription.Domain.Enums;

namespace SylviaNG.Prescription.Tests.Handlers.Medicines;

public class GetMedicineCatalogHandlerTests
{
    private readonly Mock<IMedicineRepository> _medicineRepositoryMock = new();
    private readonly Mock<IPrescriptionRepository> _prescriptionRepositoryMock = new();
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IStaffRepository> _staffRepositoryMock = new();
    private readonly Mock<IDoctorRepository> _doctorRepositoryMock = new();
    private readonly GetMedicineCatalogHandler _handler;

    public GetMedicineCatalogHandlerTests()
    {
        _medicineRepositoryMock.Setup(r => r.Query(It.IsAny<bool>())).Returns(new List<Medicine>
        {
            new() { MedicineId = 1, BrandName = "Napa", Strength = "500mg" },
            new() { MedicineId = 2, BrandName = "Seclo", Strength = "20mg" },
        }.BuildMock());

        _handler = new GetMedicineCatalogHandler(
            _medicineRepositoryMock.Object,
            _prescriptionRepositoryMock.Object,
            _userRepositoryMock.Object,
            _staffRepositoryMock.Object,
            _doctorRepositoryMock.Object);
    }

    private static PrescriptionRecord FinalizedRxFor(long doctorId, params (string Medicine, string Strength)[] lines)
    {
        var rx = new PrescriptionRecord { DoctorId = doctorId, Status = PrescriptionStatusEnum.Finalized };
        rx.SetMedicines(lines.Select(l => new MedicineItem { Medicine = l.Medicine, Strength = l.Strength }).ToList());
        return rx;
    }

    [Fact]
    public async Task Handle_AsAdmin_ShouldCountAcrossAllDoctors()
    {
        _userRepositoryMock.Setup(r => r.GetByKeycloakIdAsync("kc-admin")).ReturnsAsync(
            new User { UserId = 1, KeycloakId = "kc-admin", Role = UserRoleEnum.Admin, IsActive = true, Username = "admin" });
        _prescriptionRepositoryMock.Setup(r => r.Query(It.IsAny<bool>())).Returns(new List<PrescriptionRecord>
        {
            FinalizedRxFor(10, ("Napa", "500mg")),
            FinalizedRxFor(20, ("Napa", "500mg")),
            FinalizedRxFor(20, ("Seclo", "20mg")),
        }.BuildMock());

        var result = await _handler.Handle(new GetMedicineCatalogQuery(null, "kc-admin"), default);

        result.Single(m => m.BrandName == "Napa").TotalPrescribed.Should().Be(2);
        result.Single(m => m.BrandName == "Seclo").TotalPrescribed.Should().Be(1);
    }

    [Fact]
    public async Task Handle_AsDoctor_ShouldCountOnlyOwnFinalizedPrescriptions()
    {
        _userRepositoryMock.Setup(r => r.GetByKeycloakIdAsync("kc-doc")).ReturnsAsync(
            new User { UserId = 2, KeycloakId = "kc-doc", Role = UserRoleEnum.Doctor, IsActive = true, Username = "doc" });
        _doctorRepositoryMock.Setup(r => r.GetByUserIdAsync(2)).ReturnsAsync(new Doctor { DoctorId = 10, FullName = "Dr. Ten" });
        _prescriptionRepositoryMock.Setup(r => r.Query(It.IsAny<bool>())).Returns(new List<PrescriptionRecord>
        {
            FinalizedRxFor(10, ("Napa", "500mg")),
            FinalizedRxFor(20, ("Napa", "500mg")), // a different doctor — must not count
        }.BuildMock());

        var result = await _handler.Handle(new GetMedicineCatalogQuery(null, "kc-doc"), default);

        result.Single(m => m.BrandName == "Napa").TotalPrescribed.Should().Be(1);
    }

    [Fact]
    public async Task Handle_ShouldOrderByTotalPrescribedDescending()
    {
        _userRepositoryMock.Setup(r => r.GetByKeycloakIdAsync("kc-admin")).ReturnsAsync(
            new User { UserId = 1, KeycloakId = "kc-admin", Role = UserRoleEnum.Admin, IsActive = true, Username = "admin" });
        _prescriptionRepositoryMock.Setup(r => r.Query(It.IsAny<bool>())).Returns(new List<PrescriptionRecord>
        {
            FinalizedRxFor(10, ("Seclo", "20mg")),
            FinalizedRxFor(10, ("Seclo", "20mg")),
            FinalizedRxFor(10, ("Napa", "500mg")),
        }.BuildMock());

        var result = await _handler.Handle(new GetMedicineCatalogQuery(null, "kc-admin"), default);

        result.Select(m => m.BrandName).Should().ContainInOrder("Seclo", "Napa");
    }

    [Fact]
    public async Task Handle_IgnoresDraftPrescriptions()
    {
        _userRepositoryMock.Setup(r => r.GetByKeycloakIdAsync("kc-admin")).ReturnsAsync(
            new User { UserId = 1, KeycloakId = "kc-admin", Role = UserRoleEnum.Admin, IsActive = true, Username = "admin" });
        var draft = new PrescriptionRecord { DoctorId = 10, Status = PrescriptionStatusEnum.Draft };
        draft.SetMedicines(new List<MedicineItem> { new() { Medicine = "Napa", Strength = "500mg" } });
        _prescriptionRepositoryMock.Setup(r => r.Query(It.IsAny<bool>()))
            .Returns(new List<PrescriptionRecord> { draft }.BuildMock());

        var result = await _handler.Handle(new GetMedicineCatalogQuery(null, "kc-admin"), default);

        result.Single(m => m.BrandName == "Napa").TotalPrescribed.Should().Be(0);
    }
}
