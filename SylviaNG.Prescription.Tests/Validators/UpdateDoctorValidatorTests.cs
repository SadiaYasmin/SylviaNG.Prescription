using FluentAssertions;
using SylviaNG.Prescription.Application.Features.Doctors.Commands.UpdateDoctor;
using SylviaNG.Prescription.Application.Features.Doctors.Models;

namespace SylviaNG.Prescription.Tests.Validators;

public class UpdateDoctorValidatorTests
{
    private readonly UpdateDoctorValidator _validator = new();

    private static UpdateDoctorRequest ValidRequest() => new()
    {
        FullName = "Dr. Jane Doe",
        Phone = "01712345678",
        IsActive = true
    };

    [Fact]
    public void Validate_WithValidRequest_ShouldHaveNoErrors()
    {
        var command = new UpdateDoctorCommand(1, ValidRequest());

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithZeroDoctorId_ShouldHaveError()
    {
        var command = new UpdateDoctorCommand(0, ValidRequest());

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "DoctorId");
    }

    [Fact]
    public void Validate_WithInvalidPhone_ShouldHaveError()
    {
        var request = ValidRequest();
        request.Phone = "12345";
        var command = new UpdateDoctorCommand(1, request);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "Request.Phone");
    }
}
