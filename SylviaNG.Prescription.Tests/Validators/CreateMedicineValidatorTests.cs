using FluentAssertions;
using SylviaNG.Prescription.Application.Features.Medicines.Commands.CreateMedicine;
using SylviaNG.Prescription.Application.Features.Medicines.Models;

namespace SylviaNG.Prescription.Tests.Validators;

public class CreateMedicineValidatorTests
{
    private readonly CreateMedicineValidator _validator = new();

    private static CreateMedicineRequest ValidRequest() => new() { BrandName = "Napa", Strength = "500mg" };

    [Fact]
    public void Validate_WithValidRequest_ShouldHaveNoErrors()
    {
        var result = _validator.Validate(new CreateMedicineCommand(ValidRequest()));

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithEmptyBrandName_ShouldHaveError()
    {
        var request = ValidRequest();
        request.BrandName = "";

        var result = _validator.Validate(new CreateMedicineCommand(request));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "Request.BrandName");
    }

    [Fact]
    public void Validate_WithBrandNameTooLong_ShouldHaveError()
    {
        var request = ValidRequest();
        request.BrandName = new string('a', 201);

        var result = _validator.Validate(new CreateMedicineCommand(request));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "Request.BrandName");
    }
}
