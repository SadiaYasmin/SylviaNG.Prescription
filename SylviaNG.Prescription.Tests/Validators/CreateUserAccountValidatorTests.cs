using FluentAssertions;
using SylviaNG.Prescription.Application.Features.Auth.Commands.CreateUserAccount;
using SylviaNG.Prescription.Application.Features.Auth.Models;
using SylviaNG.Prescription.Domain.Enums;

namespace SylviaNG.Prescription.Tests.Validators;

public class CreateUserAccountValidatorTests
{
    private readonly CreateUserAccountValidator _validator = new();

    [Fact]
    public void Validate_WithValidRequest_ShouldHaveNoErrors()
    {
        // Arrange
        var command = new CreateUserAccountCommand(new CreateUserAccountRequest
        {
            Username = "new.doctor",
            Email = "new.doctor@example.com",
            Role = UserRoleEnum.Doctor
        });

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithEmptyUsername_ShouldHaveError()
    {
        // Arrange
        var command = new CreateUserAccountCommand(new CreateUserAccountRequest
        {
            Username = "",
            Role = UserRoleEnum.Doctor
        });

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "Request.Username");
    }

    [Fact]
    public void Validate_WithInvalidEmail_ShouldHaveError()
    {
        // Arrange
        var command = new CreateUserAccountCommand(new CreateUserAccountRequest
        {
            Username = "new.doctor",
            Email = "not-an-email",
            Role = UserRoleEnum.Doctor
        });

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "Request.Email");
    }
}
