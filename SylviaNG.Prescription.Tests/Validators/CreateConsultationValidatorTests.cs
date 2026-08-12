using FluentAssertions;
using SylviaNG.Prescription.Application.Features.Consultations.Commands.CreateConsultation;
using SylviaNG.Prescription.Application.Features.Consultations.Models;
using SylviaNG.Prescription.SharedKernel.Utils;

namespace SylviaNG.Prescription.Tests.Validators;

public class CreateConsultationValidatorTests
{
    private readonly CreateConsultationValidator _validator = new();

    private static CreateConsultationRequest ValidRequest() => new()
    {
        PatientId = 1,
        DoctorId = 10
    };

    [Fact]
    public void Validate_WithValidRequest_ShouldHaveNoErrors()
    {
        var command = new CreateConsultationCommand("kc-1", ValidRequest());

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_WithNonPositivePatientId_ShouldHaveError(long patientId)
    {
        var request = ValidRequest();
        request.PatientId = patientId;
        var command = new CreateConsultationCommand("kc-1", request);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "Request.PatientId");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_WithNonPositiveDoctorId_ShouldHaveError(long doctorId)
    {
        var request = ValidRequest();
        request.DoctorId = doctorId;
        var command = new CreateConsultationCommand("kc-1", request);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "Request.DoctorId");
    }

    [Fact]
    public void Validate_WithNullVisitDate_ShouldHaveNoErrors()
    {
        var request = ValidRequest();
        request.VisitDate = null;
        var command = new CreateConsultationCommand("kc-1", request);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithTodayVisitDate_ShouldHaveNoErrors()
    {
        var request = ValidRequest();
        request.VisitDate = DateTimeUtility.TodayLocal();
        var command = new CreateConsultationCommand("kc-1", request);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithPastVisitDate_ShouldHaveError()
    {
        var request = ValidRequest();
        request.VisitDate = DateTimeUtility.TodayLocal().AddDays(-1);
        var command = new CreateConsultationCommand("kc-1", request);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "Request.VisitDate");
    }

    [Fact]
    public void Validate_WithFutureVisitDate_ShouldHaveError()
    {
        var request = ValidRequest();
        request.VisitDate = DateTimeUtility.TodayLocal().AddDays(1);
        var command = new CreateConsultationCommand("kc-1", request);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "Request.VisitDate");
    }
}
