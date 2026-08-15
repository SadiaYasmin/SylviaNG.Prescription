using FluentAssertions;
using SylviaNG.Prescription.Application.Features.Analytics;
using SylviaNG.Prescription.Application.Features.Prescriptions.Models;
using SylviaNG.Prescription.Application.Mappings;
using SylviaNG.Prescription.Domain.Entities;
using SylviaNG.Prescription.Domain.Enums;

namespace SylviaNG.Prescription.Tests.Handlers.Analytics;

public class MedicinePrescribingAggregatorTests
{
    private static PrescriptionRecord RxWith(params (string Medicine, string? Generic, string Strength)[] lines)
    {
        var rx = new PrescriptionRecord { Status = PrescriptionStatusEnum.Finalized };
        rx.SetMedicines(lines.Select(l => new MedicineItem { Medicine = l.Medicine, Generic = l.Generic, Strength = l.Strength }).ToList());
        return rx;
    }

    [Fact]
    public void Aggregate_ShouldGroupDifferentBrandsSharingAGenericTogether()
    {
        var prescriptions = new List<PrescriptionRecord>
        {
            RxWith(("Napa", "Paracetamol", "500mg")),
            RxWith(("Panadol", "Paracetamol", "650mg")), // different brand+strength, same generic
        };

        var result = MedicinePrescribingAggregator.Aggregate(prescriptions);

        result.CountsByKey.Should().HaveCount(1);
        result.CountsByKey.Values.Single().Should().Be(2);
    }

    [Fact]
    public void Aggregate_WithNoGeneric_ShouldFallBackToBrandName()
    {
        var prescriptions = new List<PrescriptionRecord> { RxWith(("Seclo", null, "20mg")) };

        var result = MedicinePrescribingAggregator.Aggregate(prescriptions);

        result.LabelByKey.Values.Single().Should().Be("Seclo");
    }

    [Fact]
    public void Aggregate_DuplicateMedicineWithinOnePrescription_ShouldNotPairWithItself()
    {
        // Same medicine appears twice on one Rx (e.g. two different dosage lines) — must not
        // create a self-pair, and the co-prescribed pair count must still reflect one
        // prescription containing both distinct medicines, not be inflated by the duplicate.
        var prescriptions = new List<PrescriptionRecord>
        {
            RxWith(("Napa", "Paracetamol", "500mg"), ("Napa", "Paracetamol", "500mg"), ("Seclo", "Omeprazole", "20mg")),
        };

        var result = MedicinePrescribingAggregator.Aggregate(prescriptions);

        result.CoPrescribedPairCounts.Should().HaveCount(1);
        result.CoPrescribedPairCounts.Values.Single().Should().Be(1);
    }

    [Fact]
    public void Aggregate_CoPrescribedPair_ShouldOrderAlphabeticallyRegardlessOfInputOrder()
    {
        var forward = MedicinePrescribingAggregator.Aggregate(new List<PrescriptionRecord>
        {
            RxWith(("Napa", "Paracetamol", "500mg"), ("Amoxicillin", "Amoxicillin", "250mg")),
        });
        var reversed = MedicinePrescribingAggregator.Aggregate(new List<PrescriptionRecord>
        {
            RxWith(("Amoxicillin", "Amoxicillin", "250mg"), ("Napa", "Paracetamol", "500mg")),
        });

        forward.CoPrescribedPairCounts.Keys.Single().Should().Be(reversed.CoPrescribedPairCounts.Keys.Single());
        forward.CoPrescribedPairCounts.Keys.Single().A.Should().Be("Amoxicillin");
        forward.CoPrescribedPairCounts.Keys.Single().B.Should().Be("Paracetamol");
    }

    [Fact]
    public void BreakdownByCategory_ShouldResolveViaGenericKeyAndFallBackToUncategorized()
    {
        var catalog = new List<Medicine>
        {
            new() { BrandName = "Napa", GenericName = "Paracetamol", Category = "Analgesic" },
        };
        var prescriptions = new List<PrescriptionRecord>
        {
            RxWith(("Napa", "Paracetamol", "500mg")),
            RxWith(("Unknown Med", "Unknown Generic", "10mg")),
        };

        var result = MedicinePrescribingAggregator.BreakdownByCategory(prescriptions, catalog);

        result.Single(c => c.Category == "Analgesic").Count.Should().Be(1);
        result.Single(c => c.Category == "Uncategorized").Count.Should().Be(1);
    }
}
