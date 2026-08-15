using FluentAssertions;
using MockQueryable;
using Moq;
using SylviaNG.Prescription.Application.Common.Exceptions;
using SylviaNG.Prescription.Application.Features.Doctors.Queries.GetDoctorDetails;
using SylviaNG.Prescription.Application.Features.Prescriptions.Models;
using SylviaNG.Prescription.Application.Interfaces.Repositories;
using SylviaNG.Prescription.Application.Mappings;
using SylviaNG.Prescription.Domain.Entities;
using SylviaNG.Prescription.Domain.Enums;

namespace SylviaNG.Prescription.Tests.Handlers.Doctors;

public class GetDoctorDetailsHandlerTests
{
    private readonly Mock<IDoctorRepository> _doctorRepositoryMock = new();
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IConsultationRepository> _consultationRepositoryMock = new();
    private readonly Mock<IPrescriptionRepository> _prescriptionRepositoryMock = new();
    private readonly Mock<IPatientRepository> _patientRepositoryMock = new();
    private readonly GetDoctorDetailsHandler _handler;

    public GetDoctorDetailsHandlerTests()
    {
        _consultationRepositoryMock.Setup(r => r.Query(It.IsAny<bool>())).Returns(new List<Consultation>().BuildMock());
        _prescriptionRepositoryMock.Setup(r => r.Query(It.IsAny<bool>())).Returns(new List<PrescriptionRecord>().BuildMock());
        _patientRepositoryMock.Setup(r => r.Query(It.IsAny<bool>())).Returns(new List<Patient>().BuildMock());

        _handler = new GetDoctorDetailsHandler(
            _doctorRepositoryMock.Object,
            _userRepositoryMock.Object,
            _consultationRepositoryMock.Object,
            _prescriptionRepositoryMock.Object,
            _patientRepositoryMock.Object);
    }

    private static PrescriptionRecord FinalizedRxFor(
        long doctorId, long patientId, DateTime finalizedAt, string diagnosisText, params (string Medicine, string Strength)[] medicines)
    {
        var rx = new PrescriptionRecord
        {
            DoctorId = doctorId,
            PatientId = patientId,
            Status = PrescriptionStatusEnum.Finalized,
            FinalizedAt = finalizedAt,
            DisplayCode = $"RX-{patientId}-{finalizedAt:yyyyMMdd}"
        };
        rx.SetMedicines(medicines.Select(m => new MedicineItem { Medicine = m.Medicine, Strength = m.Strength }).ToList());
        rx.SetDiagnoses(new List<DiagnosisItem> { new() { Text = diagnosisText } });
        return rx;
    }

    [Fact]
    public async Task Handle_WithNoActivity_ShouldReturnZeroStatePerformanceWithoutDividingByZero()
    {
        var doctor = new Doctor { DoctorId = 1, UserId = 5, FullName = "Dr. Jane Doe" };
        var user = new User { UserId = 5, KeycloakId = "kc-5", Username = "jane.doe", Role = UserRoleEnum.Doctor, IsActive = true };
        _doctorRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(doctor);
        _userRepositoryMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(user);

        var result = await _handler.Handle(new GetDoctorDetailsQuery(1), default);

        result.Profile.FullName.Should().Be("Dr. Jane Doe");
        result.Performance.TotalPrescriptions.Should().Be(0);
        result.Performance.AvgPrescriptionsPerConsultation.Should().Be(0);
        result.Performance.AvgMedicinesPerPrescription.Should().Be(0);
        result.Performance.TopMedicines.Should().BeEmpty();
        result.Performance.RecentPrescriptions.Should().BeEmpty();
        result.Performance.ActivityTrend.Should().BeEmpty();
        result.Performance.BusiestHours.Should().HaveCount(24);
        result.Performance.BusiestHours.Should().OnlyContain(h => h.Count == 0);
        result.Performance.BusiestHours.Select(h => h.Hour).Should().ContainInOrder(Enumerable.Range(0, 24));
    }

    [Fact]
    public async Task Handle_WithRealActivity_ShouldComputeAggregatesFromConsultationsAndFinalizedPrescriptions()
    {
        var doctor = new Doctor { DoctorId = 1, UserId = 5, FullName = "Dr. Jane Doe" };
        var user = new User { UserId = 5, KeycloakId = "kc-5", Username = "jane.doe", Role = UserRoleEnum.Doctor, IsActive = true };
        _doctorRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(doctor);
        _userRepositoryMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(user);

        _consultationRepositoryMock.Setup(r => r.Query(It.IsAny<bool>())).Returns(new List<Consultation>
        {
            new() { DoctorId = 1, PatientId = 100, Status = ConsultationStatusEnum.Completed, CheckInAt = new DateTime(2026, 1, 1, 9, 15, 0, DateTimeKind.Utc) },
            new() { DoctorId = 1, PatientId = 100, Status = ConsultationStatusEnum.Completed, CheckInAt = new DateTime(2026, 1, 2, 9, 45, 0, DateTimeKind.Utc) },
            new() { DoctorId = 1, PatientId = 200, Status = ConsultationStatusEnum.Waiting, CheckInAt = new DateTime(2026, 1, 3, 14, 5, 0, DateTimeKind.Utc) },
            new() { DoctorId = 99, PatientId = 300, Status = ConsultationStatusEnum.Completed, CheckInAt = new DateTime(2026, 1, 1, 9, 0, 0, DateTimeKind.Utc) }, // different doctor
        }.BuildMock());

        _prescriptionRepositoryMock.Setup(r => r.Query(It.IsAny<bool>())).Returns(new List<PrescriptionRecord>
        {
            FinalizedRxFor(1, 100, new DateTime(2026, 1, 5), "Flu", ("Napa", "500mg"), ("Seclo", "20mg")),
            FinalizedRxFor(1, 200, new DateTime(2026, 1, 6), "Cold", ("Napa", "500mg")),
            FinalizedRxFor(99, 300, new DateTime(2026, 1, 6), "Other doctor", ("Napa", "500mg")), // different doctor
        }.BuildMock());

        _patientRepositoryMock.Setup(r => r.Query(It.IsAny<bool>())).Returns(new List<Patient>
        {
            new() { PatientId = 100, Name = "Alice" },
            new() { PatientId = 200, Name = "Bob" },
        }.BuildMock());

        var result = await _handler.Handle(new GetDoctorDetailsQuery(1), default);

        var performance = result.Performance;
        performance.TotalPatientsConsulted.Should().Be(2); // distinct 100/200, own-doctor consultations only
        performance.TotalPrescriptions.Should().Be(2);
        performance.TotalMedicinesPrescribed.Should().Be(3);
        performance.AvgPrescriptionsPerConsultation.Should().Be(1.0); // 2 finalized / 2 completed consultations
        performance.AvgMedicinesPerPrescription.Should().Be(1.5); // 3 medicine lines / 2 prescriptions

        performance.TopMedicines.Should().HaveCount(2);
        performance.TopMedicines[0].Name.Should().Be("Napa");
        performance.TopMedicines[0].Count.Should().Be(2);

        performance.RecentPrescriptions.Should().HaveCount(2);
        performance.RecentPrescriptions[0].PatientName.Should().Be("Bob"); // most recently finalized first
        performance.RecentPrescriptions[1].PatientName.Should().Be("Alice");

        performance.ActivityTrend.Should().HaveCount(2);

        performance.BusiestHours.Should().HaveCount(24);
        performance.BusiestHours.Single(h => h.Hour == 9).Count.Should().Be(2);
        performance.BusiestHours.Single(h => h.Hour == 14).Count.Should().Be(1);
        performance.BusiestHours.Where(h => h.Hour != 9 && h.Hour != 14).Should().OnlyContain(h => h.Count == 0);
    }

    [Fact]
    public async Task Handle_WithNonExistentDoctor_ShouldThrowNotFoundException()
    {
        _doctorRepositoryMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Doctor?)null);

        var act = () => _handler.Handle(new GetDoctorDetailsQuery(999), default);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
