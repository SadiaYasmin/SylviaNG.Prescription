using FluentAssertions;
using SylviaNG.Prescription.Application.Features.Doctors.Commands.UpdateDoctorProfile;
using SylviaNG.Prescription.Application.Features.Doctors.Models;

namespace SylviaNG.Prescription.Tests.Validators;

public class UpdateDoctorProfileValidatorTests
{
    private readonly UpdateDoctorProfileValidator _validator = new();

    private static UpdateDoctorProfileRequest ValidRequest() => new() { FullName = "Dr. Jane Doe", Phone = "01712345678" };

    [Fact]
    public void Validate_WithValidRequest_ShouldHaveNoErrors()
    {
        var result = _validator.Validate(new UpdateDoctorProfileCommand("kc-1", ValidRequest()));

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithEmptyFullName_ShouldHaveError()
    {
        var request = ValidRequest();
        request.FullName = "";

        var result = _validator.Validate(new UpdateDoctorProfileCommand("kc-1", request));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "Request.FullName");
    }

    [Fact]
    public void Validate_WithInvalidEmail_ShouldHaveError()
    {
        var request = ValidRequest();
        request.Email = "not-an-email";

        var result = _validator.Validate(new UpdateDoctorProfileCommand("kc-1", request));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "Request.Email");
    }

    [Theory]
    [InlineData("0171234567")] // 10 digits
    [InlineData("02712345678")] // wrong prefix
    public void Validate_WithInvalidPhone_ShouldHaveError(string phone)
    {
        var request = ValidRequest();
        request.Phone = phone;

        var result = _validator.Validate(new UpdateDoctorProfileCommand("kc-1", request));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "Request.Phone");
    }

    [Fact]
    public void Validate_WithEmptyPhone_ShouldHaveErrors()
    {
        var request = ValidRequest();
        request.Phone = "";

        var result = _validator.Validate(new UpdateDoctorProfileCommand("kc-1", request));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Request.Phone");
    }
}
