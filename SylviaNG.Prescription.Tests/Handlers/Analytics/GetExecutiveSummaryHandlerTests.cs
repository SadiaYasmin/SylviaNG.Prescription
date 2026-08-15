using FluentAssertions;
using MockQueryable;
using Moq;
using SylviaNG.Prescription.Application.Features.Analytics.Queries.GetExecutiveSummary;
using SylviaNG.Prescription.Application.Features.Prescriptions.Models;
using SylviaNG.Prescription.Application.Interfaces.Repositories;
using SylviaNG.Prescription.Application.Mappings;
using SylviaNG.Prescription.Domain.Entities;
using SylviaNG.Prescription.Domain.Enums;

namespace SylviaNG.Prescription.Tests.Handlers.Analytics;

public class GetExecutiveSummaryHandlerTests
{
    private readonly Mock<IPatientRepository> _patientRepositoryMock = new();
    private readonly Mock<IPrescriptionRepository> _prescriptionRepositoryMock = new();
    private readonly Mock<IMedicineRepository> _medicineRepositoryMock = new();
    private readonly Mock<IDoctorRepository> _doctorRepositoryMock = new();
    private readonly GetExecutiveSummaryHandler _handler;

    public GetExecutiveSummaryHandlerTests()
    {
        _medicineRepositoryMock.Setup(r => r.Query(It.IsAny<bool>())).Returns(new List<Medicine>().BuildMock());
        _doctorRepositoryMock.Setup(r => r.Query(It.IsAny<bool>())).Returns(new List<Doctor>().BuildMock());

        _handler = new GetExecutiveSummaryHandler(
            _patientRepositoryMock.Object, _prescriptionRepositoryMock.Object,
            _medicineRepositoryMock.Object, _doctorRepositoryMock.Object);
    }

    private static PrescriptionRecord FinalizedRxAt(DateTime finalizedAt)
    {
        var rx = new PrescriptionRecord { Status = PrescriptionStatusEnum.Finalized, FinalizedAt = finalizedAt };
        rx.SetMedicines(new List<MedicineItem> { new() { Medicine = "Napa", Strength = "500mg" } });
        return rx;
    }

    [Fact]
    public async Task Handle_ShouldClassifyByUtcCalendarMonthRelativeToNow()
    {
        var now = DateTime.UtcNow;
        var thisMonth = new DateTime(now.Year, now.Month, 1, 12, 0, 0, DateTimeKind.Utc);
        var lastMonth = thisMonth.AddMonths(-1);
        var twoMonthsAgo = thisMonth.AddMonths(-2);

        _patientRepositoryMock.Setup(r => r.Query(It.IsAny<bool>())).Returns(new List<Patient>().BuildMock());
        _prescriptionRepositoryMock.Setup(r => r.Query(It.IsAny<bool>())).Returns(new List<PrescriptionRecord>
        {
            FinalizedRxAt(thisMonth),
            FinalizedRxAt(thisMonth),
            FinalizedRxAt(lastMonth),
            FinalizedRxAt(twoMonthsAgo), // must not count in either bucket
        }.BuildMock());

        var result = await _handler.Handle(new GetExecutiveSummaryQuery(), default);

        result.PrescriptionTrend.Current.Should().Be(2);
        result.PrescriptionTrend.Previous.Should().Be(1);
        result.PrescriptionTrend.PercentChange.Should().Be(100);
    }

    [Fact]
    public async Task Handle_TopMedicinesAndTopActiveDoctors_ShouldTruncateToFive()
    {
        _patientRepositoryMock.Setup(r => r.Query(It.IsAny<bool>())).Returns(new List<Patient>().BuildMock());
        var prescriptions = new List<PrescriptionRecord>();
        for (var doctorId = 1; doctorId <= 7; doctorId++)
        {
            var rx = new PrescriptionRecord { DoctorId = doctorId, Status = PrescriptionStatusEnum.Finalized, FinalizedAt = DateTime.UtcNow };
            rx.SetMedicines(new List<MedicineItem> { new() { Medicine = $"Med{doctorId}", Strength = "1mg" } });
            prescriptions.Add(rx);
        }
        _prescriptionRepositoryMock.Setup(r => r.Query(It.IsAny<bool>())).Returns(prescriptions.BuildMock());

        var result = await _handler.Handle(new GetExecutiveSummaryQuery(), default);

        result.TopMedicines.Should().HaveCount(5);
        result.TopActiveDoctors.Should().HaveCount(5);
    }
}
