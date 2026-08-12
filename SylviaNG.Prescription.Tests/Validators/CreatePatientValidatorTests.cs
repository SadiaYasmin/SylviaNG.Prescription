using FluentAssertions;
using SylviaNG.Prescription.Application.Features.Patients.Commands.CreatePatient;
using SylviaNG.Prescription.Application.Features.Patients.Models;
using SylviaNG.Prescription.Domain.Enums;

namespace SylviaNG.Prescription.Tests.Validators;

public class CreatePatientValidatorTests
{
    private readonly CreatePatientValidator _validator = new();

    private static CreatePatientRequest ValidRequest() => new()
    {
        Name = "John Doe",
        Phone = "01712345678",
        DateOfBirth = new DateOnly(1990, 1, 1)
    };

    [Fact]
    public void Validate_WithValidRequest_ShouldHaveNoErrors()
    {
        var command = new CreatePatientCommand("kc-1", ValidRequest());

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithEmptyName_ShouldHaveError()
    {
        var request = ValidRequest();
        request.Name = "";
        var command = new CreatePatientCommand("kc-1", request);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "Request.Name");
    }

    [Fact]
    public void Validate_WithEmptyPhone_ShouldHaveError()
    {
        var request = ValidRequest();
        request.Phone = "";
        var command = new CreatePatientCommand("kc-1", request);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Request.Phone");
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
        var command = new CreatePatientCommand("kc-1", request);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Request.Phone");
    }

    [Theory]
    [InlineData("01712345678")]
    [InlineData("01912345678")]
    [InlineData("01312345678")]
    public void Validate_WithValidBangladeshPhone_ShouldHaveNoPhoneError(string phone)
    {
        var request = ValidRequest();
        request.Phone = phone;
        var command = new CreatePatientCommand("kc-1", request);

        var result = _validator.Validate(command);

        result.Errors.Should().NotContain(e => e.PropertyName == "Request.Phone");
    }

    [Fact]
    public void Validate_WithNoDateOfBirthAndNoAge_ShouldHaveError()
    {
        var request = ValidRequest();
        request.DateOfBirth = null;
        request.Age = null;
        var command = new CreatePatientCommand("kc-1", request);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "Request.Age");
    }

    [Fact]
    public void Validate_WithNoDateOfBirthButAgeProvided_ShouldHaveNoErrors()
    {
        var request = ValidRequest();
        request.DateOfBirth = null;
        request.Age = 35;
        var command = new CreatePatientCommand("kc-1", request);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithDateOfBirthAndNoAge_ShouldHaveNoErrors()
    {
        var request = ValidRequest();
        request.DateOfBirth = new DateOnly(1990, 1, 1);
        request.Age = null;
        var command = new CreatePatientCommand("kc-1", request);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithNoAllergyInfoAtAll_ShouldHaveNoErrors()
    {
        var request = ValidRequest();
        request.AllergyPresetId = null;
        request.AllergyOtherText = null;
        var command = new CreatePatientCommand("kc-1", request);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithPresetAllergySelected_ShouldHaveNoErrors()
    {
        var request = ValidRequest();
        request.AllergyPresetId = AllergyPresetEnum.Penicillin;
        request.AllergyOtherText = null;
        var command = new CreatePatientCommand("kc-1", request);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithOtherAllergyTextProvided_ShouldHaveNoErrors()
    {
        var request = ValidRequest();
        request.AllergyPresetId = null;
        request.AllergyOtherText = "Cat dander";
        var command = new CreatePatientCommand("kc-1", request);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithOtherAllergySignalledButBlankText_ShouldHaveError()
    {
        var request = ValidRequest();
        request.AllergyPresetId = null;
        request.AllergyOtherText = "";
        var command = new CreatePatientCommand("kc-1", request);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "Request.AllergyOtherText");
    }
}
