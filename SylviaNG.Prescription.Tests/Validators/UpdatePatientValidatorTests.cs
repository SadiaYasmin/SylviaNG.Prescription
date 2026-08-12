using FluentAssertions;
using SylviaNG.Prescription.Application.Features.Patients.Commands.UpdatePatient;
using SylviaNG.Prescription.Application.Features.Patients.Models;
using SylviaNG.Prescription.Domain.Enums;

namespace SylviaNG.Prescription.Tests.Validators;

public class UpdatePatientValidatorTests
{
    private readonly UpdatePatientValidator _validator = new();

    private static UpdatePatientRequest ValidRequest() => new()
    {
        Name = "John Doe",
        Phone = "01712345678",
        DateOfBirth = new DateOnly(1990, 1, 1)
    };

    [Fact]
    public void Validate_WithValidRequest_ShouldHaveNoErrors()
    {
        var command = new UpdatePatientCommand(1, "kc-1", ValidRequest());

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithInvalidPatientId_ShouldHaveError()
    {
        var command = new UpdatePatientCommand(0, "kc-1", ValidRequest());

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "PatientId");
    }

    [Fact]
    public void Validate_WithEmptyName_ShouldHaveError()
    {
        var request = ValidRequest();
        request.Name = "";
        var command = new UpdatePatientCommand(1, "kc-1", request);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "Request.Name");
    }

    [Theory]
    [InlineData("123")]
    [InlineData("02712345678")]
    [InlineData("017123456")]
    [InlineData("0171234567890")]
    public void Validate_WithInvalidBangladeshPhone_ShouldHaveError(string phone)
    {
        var request = ValidRequest();
        request.Phone = phone;
        var command = new UpdatePatientCommand(1, "kc-1", request);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Request.Phone");
    }

    [Fact]
    public void Validate_WithNoDateOfBirthAndNoAge_ShouldHaveError()
    {
        var request = ValidRequest();
        request.DateOfBirth = null;
        request.Age = null;
        var command = new UpdatePatientCommand(1, "kc-1", request);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "Request.Age");
    }

    [Fact]
    public void Validate_WithNoDateOfBirthButAgeProvided_ShouldHaveNoErrors()
    {
        var request = ValidRequest();
        request.DateOfBirth = null;
        request.Age = 40;
        var command = new UpdatePatientCommand(1, "kc-1", request);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithOtherAllergySignalledButBlankText_ShouldHaveError()
    {
        var request = ValidRequest();
        request.AllergyPresetId = null;
        request.AllergyOtherText = "";
        var command = new UpdatePatientCommand(1, "kc-1", request);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "Request.AllergyOtherText");
    }

    [Fact]
    public void Validate_WithOtherAllergyTextProvided_ShouldHaveNoErrors()
    {
        var request = ValidRequest();
        request.AllergyPresetId = null;
        request.AllergyOtherText = "Cat dander";
        var command = new UpdatePatientCommand(1, "kc-1", request);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithNoAllergyInfoAtAll_ShouldHaveNoErrors()
    {
        var request = ValidRequest();
        request.AllergyPresetId = null;
        request.AllergyOtherText = null;
        var command = new UpdatePatientCommand(1, "kc-1", request);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithPresetAllergySelected_ShouldHaveNoErrors()
    {
        var request = ValidRequest();
        request.AllergyPresetId = AllergyPresetEnum.Dust;
        request.AllergyOtherText = null;
        var command = new UpdatePatientCommand(1, "kc-1", request);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }
}
