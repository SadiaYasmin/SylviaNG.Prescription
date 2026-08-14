using FluentAssertions;
using SylviaNG.Prescription.Application.Common.Exceptions;
using SylviaNG.Prescription.Application.Features.Prescriptions;
using SylviaNG.Prescription.Application.Features.Prescriptions.Models;

namespace SylviaNG.Prescription.Tests.Handlers.Prescriptions;

public class MedicineDuplicateGuardTests
{
    [Fact]
    public void EnsureNoDuplicates_WithDistinctMedicines_ShouldNotThrow()
    {
        var medicines = new List<MedicineItem>
        {
            new() { Medicine = "Napa", Strength = "500mg" },
            new() { Medicine = "Napa", Strength = "250mg" },
            new() { Medicine = "Seclo", Strength = "20mg" }
        };

        var act = () => MedicineDuplicateGuard.EnsureNoDuplicates(medicines);

        act.Should().NotThrow();
    }

    [Fact]
    public void EnsureNoDuplicates_WithSameMedicineAndStrength_ShouldThrowBadRequestException()
    {
        var medicines = new List<MedicineItem>
        {
            new() { Medicine = "Napa", Strength = "500mg" },
            new() { Medicine = "Napa", Strength = "500mg" }
        };

        var act = () => MedicineDuplicateGuard.EnsureNoDuplicates(medicines);

        act.Should().Throw<BadRequestException>();
    }

    [Theory]
    [InlineData("Napa", "500mg", "napa", "500MG")]
    [InlineData("Napa ", "500mg", "Napa", " 500mg ")]
    public void EnsureNoDuplicates_IsCaseAndWhitespaceInsensitive(string medicine1, string strength1, string medicine2, string strength2)
    {
        var medicines = new List<MedicineItem>
        {
            new() { Medicine = medicine1, Strength = strength1 },
            new() { Medicine = medicine2, Strength = strength2 }
        };

        var act = () => MedicineDuplicateGuard.EnsureNoDuplicates(medicines);

        act.Should().Throw<BadRequestException>();
    }

    [Fact]
    public void EnsureNoDuplicates_IgnoresDosageFrequencyDurationInstructions()
    {
        // Same medicine+strength but different dosage/frequency still counts as a duplicate —
        // those fields are deliberately excluded from the matching key (US-022).
        var medicines = new List<MedicineItem>
        {
            new() { Medicine = "Napa", Strength = "500mg", Dosage = "1 tablet", Frequency = "BID" },
            new() { Medicine = "Napa", Strength = "500mg", Dosage = "2 tablets", Frequency = "TID" }
        };

        var act = () => MedicineDuplicateGuard.EnsureNoDuplicates(medicines);

        act.Should().Throw<BadRequestException>();
    }
}
