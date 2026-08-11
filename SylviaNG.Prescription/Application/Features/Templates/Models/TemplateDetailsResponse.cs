using SylviaNG.Prescription.Domain.Enums;

namespace SylviaNG.Prescription.Application.Features.Templates.Models
{
    public class TemplateDetailsResponse
    {
        public long TemplateId { get; set; }
        public string Name { get; set; } = string.Empty;
        public TemplateTypeEnum Type { get; set; }
        public TemplateLanguageEnum Language { get; set; }
        public bool Enabled { get; set; }
        public bool IsSystemDefault { get; set; }
        public TemplateConfig Config { get; set; } = new();
    }
}
