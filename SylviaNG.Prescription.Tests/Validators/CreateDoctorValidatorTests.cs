using FluentAssertions;
using SylviaNG.Prescription.Application.Features.Doctors.Commands.CreateDoctor;
using SylviaNG.Prescription.Application.Features.Doctors.Models;

namespace SylviaNG.Prescription.Tests.Validators;

public class CreateDoctorValidatorTests
{
    private readonly CreateDoctorValidator _validator = new();

    private static CreateDoctorRequest ValidRequest() => new()
    {
        Username = "new.doctor",
        Email = "new.doctor@example.com",
        FullName = "Dr. Jane Doe",
        Phone = "01712345678"
    };

    [Fact]
    public void Validate_WithValidRequest_ShouldHaveNoErrors()
    {
        var command = new CreateDoctorCommand(ValidRequest());

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithEmptyFullName_ShouldHaveError()
    {
        var request = ValidRequest();
        request.FullName = "";
        var command = new CreateDoctorCommand(request);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "Request.FullName");
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
        var command = new CreateDoctorCommand(request);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "Request.Phone");
    }

    [Fact]
    public void Validate_WithInvalidEmail_ShouldHaveError()
    {
        var request = ValidRequest();
        request.Email = "not-an-email";
        var command = new CreateDoctorCommand(request);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "Request.Email");
    }

    [Fact]
    public void Validate_WithNegativeExperience_ShouldHaveError()
    {
        var request = ValidRequest();
        request.ExperienceYears = -1;
        var command = new CreateDoctorCommand(request);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "Request.ExperienceYears");
    }
}
