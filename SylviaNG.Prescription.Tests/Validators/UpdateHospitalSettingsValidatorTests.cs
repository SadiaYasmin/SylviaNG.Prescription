using FluentAssertions;
using SylviaNG.Prescription.Application.Features.HospitalSettings.Commands.UpdateHospitalSettings;
using SylviaNG.Prescription.Application.Features.HospitalSettings.Models;

namespace SylviaNG.Prescription.Tests.Validators;

public class UpdateHospitalSettingsValidatorTests
{
    private readonly UpdateHospitalSettingsValidator _validator = new();

    private static UpdateHospitalSettingsRequest ValidRequest() => new()
    {
        Name = "City Hospital",
        Address = "123 Main St",
        Phone = "01700000000",
        Email = "contact@hospital.com"
    };

    [Fact]
    public void Validate_WithValidRequest_ShouldHaveNoErrors()
    {
        var command = new UpdateHospitalSettingsCommand(ValidRequest());

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithEmptyName_ShouldHaveError()
    {
        var request = ValidRequest();
        request.Name = "";
        var command = new UpdateHospitalSettingsCommand(request);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "Request.Name");
    }

    [Fact]
    public void Validate_WithEmptyPhone_ShouldHaveError()
    {
        var request = ValidRequest();
        request.Phone = "";
        var command = new UpdateHospitalSettingsCommand(request);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "Request.Phone");
    }

    [Fact]
    public void Validate_WithInvalidEmail_ShouldHaveError()
    {
        var request = ValidRequest();
        request.Email = "not-an-email";
        var command = new UpdateHospitalSettingsCommand(request);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "Request.Email");
    }

    [Fact]
    public void Validate_WithNullEmail_ShouldHaveNoErrors()
    {
        var request = ValidRequest();
        request.Email = null;
        var command = new UpdateHospitalSettingsCommand(request);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }
}
