using FluentAssertions;
using SylviaNG.Prescription.Application.Features.Auth.Commands.Login;
using SylviaNG.Prescription.Application.Features.Auth.Models;

namespace SylviaNG.Prescription.Tests.Validators;

public class LoginValidatorTests
{
    private readonly LoginValidator _validator = new();

    [Fact]
    public void Validate_WithValidRequest_ShouldHaveNoErrors()
    {
        // Arrange
        var command = new LoginCommand(new LoginRequest { Username = "doctor.dev", Password = "DevPassword123!" });

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithEmptyUsername_ShouldHaveError()
    {
        // Arrange
        var command = new LoginCommand(new LoginRequest { Username = "", Password = "DevPassword123!" });

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "Request.Username");
    }

    [Fact]
    public void Validate_WithEmptyPassword_ShouldHaveError()
    {
        // Arrange
        var command = new LoginCommand(new LoginRequest { Username = "doctor.dev", Password = "" });

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "Request.Password");
    }
}
