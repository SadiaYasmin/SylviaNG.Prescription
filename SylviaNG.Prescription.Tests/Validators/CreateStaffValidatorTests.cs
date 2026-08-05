using FluentAssertions;
using SylviaNG.Prescription.Application.Features.Staffs.Commands.CreateStaff;
using SylviaNG.Prescription.Application.Features.Staffs.Models;

namespace SylviaNG.Prescription.Tests.Validators;

public class CreateStaffValidatorTests
{
    private readonly CreateStaffValidator _validator = new();

    private static CreateStaffRequest ValidRequest() => new()
    {
        Username = "new.staff",
        Email = "new.staff@example.com",
        FullName = "Jane Roy",
        Phone = "01712345678"
    };

    [Fact]
    public void Validate_WithValidRequest_ShouldHaveNoErrors()
    {
        var command = new CreateStaffCommand(ValidRequest());

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithEmptyFullName_ShouldHaveError()
    {
        var request = ValidRequest();
        request.FullName = "";
        var command = new CreateStaffCommand(request);

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
        var command = new CreateStaffCommand(request);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "Request.Phone");
    }

    [Fact]
    public void Validate_WithInvalidEmail_ShouldHaveError()
    {
        var request = ValidRequest();
        request.Email = "not-an-email";
        var command = new CreateStaffCommand(request);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "Request.Email");
    }

    [Fact]
    public void Validate_WithEmptyUsername_ShouldHaveError()
    {
        var request = ValidRequest();
        request.Username = "";
        var command = new CreateStaffCommand(request);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "Request.Username");
    }
}
