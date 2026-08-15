using FluentAssertions;
using MockQueryable;
using Moq;
using SylviaNG.Prescription.Application.Features.Analytics.Queries.GetMedicineAnalytics;
using SylviaNG.Prescription.Application.Features.Prescriptions.Models;
using SylviaNG.Prescription.Application.Interfaces.Repositories;
using SylviaNG.Prescription.Application.Mappings;
using SylviaNG.Prescription.Domain.Entities;
using SylviaNG.Prescription.Domain.Enums;

namespace SylviaNG.Prescription.Tests.Handlers.Analytics;

public class GetMedicineAnalyticsHandlerTests
{
    private readonly Mock<IMedicineRepository> _medicineRepositoryMock = new();
    private readonly Mock<IPrescriptionRepository> _prescriptionRepositoryMock = new();
    private readonly GetMedicineAnalyticsHandler _handler;

    public GetMedicineAnalyticsHandlerTests()
    {
        _handler = new GetMedicineAnalyticsHandler(_medicineRepositoryMock.Object, _prescriptionRepositoryMock.Object);
    }

    private static PrescriptionRecord FinalizedRxWith(params (string Medicine, string? Generic, string Strength)[] lines)
    {
        var rx = new PrescriptionRecord { Status = PrescriptionStatusEnum.Finalized };
        rx.SetMedicines(lines.Select(l => new MedicineItem { Medicine = l.Medicine, Generic = l.Generic, Strength = l.Strength }).ToList());
        return rx;
    }

    [Fact]
    public async Task Handle_RarelyUsedMedicines_ShouldIncludeNeverPrescribedCatalogRowsAtZero()
    {
        _medicineRepositoryMock.Setup(r => r.Query(It.IsAny<bool>())).Returns(new List<Medicine>
        {
            new() { MedicineId = 1, BrandName = "Napa", GenericName = "Paracetamol" },
            new() { MedicineId = 2, BrandName = "NeverUsed", GenericName = "NeverGeneric" },
        }.BuildMock());
        _prescriptionRepositoryMock.Setup(r => r.Query(It.IsAny<bool>())).Returns(new List<PrescriptionRecord>
        {
            FinalizedRxWith(("Napa", "Paracetamol", "500mg")),
        }.BuildMock());

        var result = await _handler.Handle(new GetMedicineAnalyticsQuery(rareThreshold: 1), default);

        result.RarelyUsedMedicines.Should().Contain(m => m.Name == "NeverGeneric" && m.Count == 0);
        result.RarelyUsedMedicines.Should().Contain(m => m.Name == "Paracetamol" && m.Count == 1);
    }

    [Fact]
    public async Task Handle_RarelyUsedMedicines_ShouldExcludeMedicinesAboveTheThreshold()
    {
        _medicineRepositoryMock.Setup(r => r.Query(It.IsAny<bool>())).Returns(new List<Medicine>
        {
            new() { MedicineId = 1, BrandName = "Napa", GenericName = "Paracetamol" },
        }.BuildMock());
        _prescriptionRepositoryMock.Setup(r => r.Query(It.IsAny<bool>())).Returns(new List<PrescriptionRecord>
        {
            FinalizedRxWith(("Napa", "Paracetamol", "500mg")),
            FinalizedRxWith(("Napa", "Paracetamol", "500mg")),
        }.BuildMock());

        var result = await _handler.Handle(new GetMedicineAnalyticsQuery(rareThreshold: 1), default);

        result.RarelyUsedMedicines.Should().NotContain(m => m.Name == "Paracetamol");
    }

    [Fact]
    public async Task Handle_ShouldOnlyCountFinalizedPrescriptions()
    {
        _medicineRepositoryMock.Setup(r => r.Query(It.IsAny<bool>())).Returns(new List<Medicine>().BuildMock());
        var draft = new PrescriptionRecord { Status = PrescriptionStatusEnum.Draft };
        draft.SetMedicines(new List<MedicineItem> { new() { Medicine = "Napa", Strength = "500mg" } });
        _prescriptionRepositoryMock.Setup(r => r.Query(It.IsAny<bool>())).Returns(new List<PrescriptionRecord> { draft }.BuildMock());

        var result = await _handler.Handle(new GetMedicineAnalyticsQuery(), default);

        result.TopPrescribedMedicines.Should().BeEmpty();
    }
}
