using SylviaNG.Prescription.Domain.Enums;

namespace SylviaNG.Prescription.Application.Features.Templates.Models
{
    /// <summary>
    /// US-046: the client only chooses Name/Type/Language on create — the server builds
    /// ConfigJson from <c>TemplateDefaults</c>. Config is only client-editable via Update.
    /// </summary>
    public class CreateTemplateRequest
    {
        public string Name { get; set; } = string.Empty;
        public TemplateTypeEnum Type { get; set; }
        public TemplateLanguageEnum Language { get; set; }
    }
}
