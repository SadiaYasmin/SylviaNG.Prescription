using FluentAssertions;
using SylviaNG.Prescription.Application.Features.Medicines.Commands.UpdateMedicine;
using SylviaNG.Prescription.Application.Features.Medicines.Models;

namespace SylviaNG.Prescription.Tests.Validators;

public class UpdateMedicineValidatorTests
{
    private readonly UpdateMedicineValidator _validator = new();

    private static UpdateMedicineRequest ValidRequest() => new() { BrandName = "Napa", Strength = "500mg" };

    [Fact]
    public void Validate_WithValidRequest_ShouldHaveNoErrors()
    {
        var result = _validator.Validate(new UpdateMedicineCommand(1, ValidRequest()));

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithEmptyBrandName_ShouldHaveError()
    {
        var request = ValidRequest();
        request.BrandName = "";

        var result = _validator.Validate(new UpdateMedicineCommand(1, request));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "Request.BrandName");
    }
}
