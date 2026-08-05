using FluentAssertions;
using SylviaNG.Prescription.Application.Features.Staffs.Commands.UpdateStaff;
using SylviaNG.Prescription.Application.Features.Staffs.Models;

namespace SylviaNG.Prescription.Tests.Validators;

public class UpdateStaffValidatorTests
{
    private readonly UpdateStaffValidator _validator = new();

    private static UpdateStaffRequest ValidRequest() => new()
    {
        Email = "existing.staff@example.com",
        FullName = "Jane Roy",
        Phone = "01712345678"
    };

    [Fact]
    public void Validate_WithValidRequest_ShouldHaveNoErrors()
    {
        var command = new UpdateStaffCommand(1, ValidRequest());

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithEmptyFullName_ShouldHaveError()
    {
        var request = ValidRequest();
        request.FullName = "";
        var command = new UpdateStaffCommand(1, request);

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
        var command = new UpdateStaffCommand(1, request);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "Request.Phone");
    }

    [Fact]
    public void Validate_WithInvalidEmail_ShouldHaveError()
    {
        var request = ValidRequest();
        request.Email = "not-an-email";
        var command = new UpdateStaffCommand(1, request);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "Request.Email");
    }

    [Fact]
    public void Validate_WithInvalidStaffId_ShouldHaveError()
    {
        var command = new UpdateStaffCommand(0, ValidRequest());

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "StaffId");
    }
}
