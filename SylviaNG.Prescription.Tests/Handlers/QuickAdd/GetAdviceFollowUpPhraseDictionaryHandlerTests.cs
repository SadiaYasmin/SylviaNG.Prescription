using FluentAssertions;
using MockQueryable;
using Moq;
using SylviaNG.Prescription.Application.Features.QuickAdd.Queries.GetAdviceFollowUpPhraseDictionary;
using SylviaNG.Prescription.Application.Interfaces.Repositories;
using SylviaNG.Prescription.Domain.Entities;
using SylviaNG.Prescription.Domain.Enums;

namespace SylviaNG.Prescription.Tests.Handlers.QuickAdd;

public class GetAdviceFollowUpPhraseDictionaryHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IStaffRepository> _staffRepositoryMock = new();
    private readonly Mock<IDoctorRepository> _doctorRepositoryMock = new();
    private readonly Mock<IQuickAddPresetRepository> _quickAddPresetRepositoryMock = new();
    private readonly GetAdviceFollowUpPhraseDictionaryHandler _handler;

    public GetAdviceFollowUpPhraseDictionaryHandlerTests()
    {
        _handler = new GetAdviceFollowUpPhraseDictionaryHandler(
            _userRepositoryMock.Object, _staffRepositoryMock.Object, _doctorRepositoryMock.Object, _quickAddPresetRepositoryMock.Object);

        _userRepositoryMock.Setup(r => r.GetByKeycloakIdAsync("kc-doc")).ReturnsAsync(
            new User { UserId = 5, KeycloakId = "kc-doc", Role = UserRoleEnum.Doctor, IsActive = true, Username = "doc" });
        _doctorRepositoryMock.Setup(r => r.GetByUserIdAsync(5)).ReturnsAsync(new Doctor { DoctorId = 10, UserId = 5, FullName = "Dr. Doc" });
    }

    private void SetUpPresets(params QuickAddPreset[] presets)
    {
        _quickAddPresetRepositoryMock.Setup(r => r.Query(It.IsAny<bool>())).Returns(presets.BuildMock());
    }

    [Fact]
    public async Task Handle_WithNoOwnPresets_ShouldReturnTheStaticDictionary()
    {
        SetUpPresets();

        var result = await _handler.Handle(new GetAdviceFollowUpPhraseDictionaryQuery("kc-doc"), default);

        result.Should().ContainKey("drink plenty of fluids.");
    }

    [Fact]
    public async Task Handle_WithOwnAdvicePreset_ShouldOverrideTheStaticEntry()
    {
        SetUpPresets(new QuickAddPreset
        {
            DoctorId = 10,
            SectionType = QuickAddSectionTypeEnum.Advice,
            Label = "Custom",
            PayloadJson = "{\"en\":\"Drink plenty of fluids.\",\"bn\":\"custom bangla\"}"
        });

        var result = await _handler.Handle(new GetAdviceFollowUpPhraseDictionaryQuery("kc-doc"), default);

        result["drink plenty of fluids."].Should().Be("custom bangla");
    }

    [Fact]
    public async Task Handle_WithOwnNovelPreset_ShouldAddANewEntry()
    {
        SetUpPresets(new QuickAddPreset
        {
            DoctorId = 10,
            SectionType = QuickAddSectionTypeEnum.FollowUp,
            Label = "Custom",
            PayloadJson = "{\"en\":\"See me next month.\",\"bn\":\"পরের মাসে দেখা করুন।\"}"
        });

        var result = await _handler.Handle(new GetAdviceFollowUpPhraseDictionaryQuery("kc-doc"), default);

        result["see me next month."].Should().Be("পরের মাসে দেখা করুন।");
    }

    [Fact]
    public async Task Handle_WithMalformedPayload_ShouldSkipItWithoutThrowing()
    {
        SetUpPresets(new QuickAddPreset { DoctorId = 10, SectionType = QuickAddSectionTypeEnum.Advice, Label = "Broken", PayloadJson = "not json" });

        var act = () => _handler.Handle(new GetAdviceFollowUpPhraseDictionaryQuery("kc-doc"), default);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Handle_ShouldIgnorePresetsFromOtherSectionsAndOtherDoctors()
    {
        SetUpPresets(
            new QuickAddPreset { DoctorId = 999, SectionType = QuickAddSectionTypeEnum.Advice, Label = "X", PayloadJson = "{\"en\":\"Other doctor's phrase.\",\"bn\":\"x\"}" },
            new QuickAddPreset { DoctorId = 10, SectionType = QuickAddSectionTypeEnum.Medicine, Label = "Napa", PayloadJson = "{\"medicine\":\"Napa\"}" });

        var result = await _handler.Handle(new GetAdviceFollowUpPhraseDictionaryQuery("kc-doc"), default);

        result.Should().NotContainKey("other doctor's phrase.");
    }
}
