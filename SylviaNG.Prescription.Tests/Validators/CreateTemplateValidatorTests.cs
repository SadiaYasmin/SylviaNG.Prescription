using FluentAssertions;
using SylviaNG.Prescription.Application.Features.Templates.Commands.CreateTemplate;
using SylviaNG.Prescription.Application.Features.Templates.Models;
using SylviaNG.Prescription.Domain.Enums;

namespace SylviaNG.Prescription.Tests.Validators;

public class CreateTemplateValidatorTests
{
    private readonly CreateTemplateValidator _validator = new();

    private static CreateTemplateRequest ValidRequest() => new()
    {
        Name = "My Template",
        Type = TemplateTypeEnum.Classic,
        Language = TemplateLanguageEnum.En
    };

    [Fact]
    public void Validate_WithValidRequest_ShouldHaveNoErrors()
    {
        var command = new CreateTemplateCommand(ValidRequest());

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithEmptyName_ShouldHaveError()
    {
        var request = ValidRequest();
        request.Name = "";
        var command = new CreateTemplateCommand(request);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "Request.Name");
    }

    [Fact]
    public void Validate_WithNameTooLong_ShouldHaveError()
    {
        var request = ValidRequest();
        request.Name = new string('a', 101);
        var command = new CreateTemplateCommand(request);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "Request.Name");
    }

    [Fact]
    public void Validate_WithInvalidType_ShouldHaveError()
    {
        var request = ValidRequest();
        request.Type = (TemplateTypeEnum)999;
        var command = new CreateTemplateCommand(request);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "Request.Type");
    }

    [Fact]
    public void Validate_WithInvalidLanguage_ShouldHaveError()
    {
        var request = ValidRequest();
        request.Language = (TemplateLanguageEnum)999;
        var command = new CreateTemplateCommand(request);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "Request.Language");
    }
}
