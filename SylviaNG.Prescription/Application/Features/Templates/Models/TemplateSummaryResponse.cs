using SylviaNG.Prescription.Domain.Enums;

namespace SylviaNG.Prescription.Application.Features.Templates.Models
{
    public class TemplateSummaryResponse
    {
        public long TemplateId { get; set; }
        public string Name { get; set; } = string.Empty;
        public TemplateTypeEnum Type { get; set; }
        public TemplateLanguageEnum Language { get; set; }
        public bool Enabled { get; set; }
        public bool IsSystemDefault { get; set; }
    }
}
