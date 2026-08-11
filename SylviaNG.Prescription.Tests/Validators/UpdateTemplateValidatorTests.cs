using FluentAssertions;
using SylviaNG.Prescription.Application.Features.Templates.Commands.UpdateTemplate;
using SylviaNG.Prescription.Application.Features.Templates.Models;

namespace SylviaNG.Prescription.Tests.Validators;

public class UpdateTemplateValidatorTests
{
    private readonly UpdateTemplateValidator _validator = new();

    private static UpdateTemplateRequest ValidRequest() => new()
    {
        Name = "My Template",
        Config = new TemplateConfig
        {
            Header = new HeaderConfig { Height = 100, BgColor = "#0F766E" },
            Footer = new FooterConfig { Height = 60, BgColor = "#F0FDFA" },
            Style = new StyleConfig { FontSize = 14, SectionSpacing = 10, BorderRadius = 4, AccentColor = "#0F766E" }
        }
    };

    [Fact]
    public void Validate_WithValidRequest_ShouldHaveNoErrors()
    {
        var command = new UpdateTemplateCommand(1, ValidRequest());

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithEmptyName_ShouldHaveError()
    {
        var request = ValidRequest();
        request.Name = "";
        var command = new UpdateTemplateCommand(1, request);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Request.Name");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(29)]
    [InlineData(301)]
    public void Validate_WithOutOfRangeHeaderHeight_ShouldHaveError(int height)
    {
        var request = ValidRequest();
        request.Config.Header.Height = height;
        var command = new UpdateTemplateCommand(1, request);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Request.Config.Header.Height");
    }

    [Theory]
    [InlineData(7)]
    [InlineData(33)]
    public void Validate_WithOutOfRangeFontSize_ShouldHaveError(int fontSize)
    {
        var request = ValidRequest();
        request.Config.Style.FontSize = fontSize;
        var command = new UpdateTemplateCommand(1, request);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Request.Config.Style.FontSize");
    }

    [Fact]
    public void Validate_WithNegativeSectionSpacing_ShouldHaveError()
    {
        var request = ValidRequest();
        request.Config.Style.SectionSpacing = -1;
        var command = new UpdateTemplateCommand(1, request);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Request.Config.Style.SectionSpacing");
    }

    [Theory]
    [InlineData("teal")]
    [InlineData("#GGGGGG")]
    [InlineData("#12345")]
    public void Validate_WithInvalidHexColor_ShouldHaveError(string color)
    {
        var request = ValidRequest();
        request.Config.Style.AccentColor = color;
        var command = new UpdateTemplateCommand(1, request);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Request.Config.Style.AccentColor");
    }

    [Fact]
    public void Validate_WithNullColors_ShouldHaveNoErrors()
    {
        // Government-type templates leave colors null (US-047) — null must not be flagged
        // by the hex-color regex rule.
        var request = ValidRequest();
        request.Config.Header.BgColor = null;
        request.Config.Footer.BgColor = null;
        request.Config.Style.AccentColor = null;
        var command = new UpdateTemplateCommand(1, request);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }
}
