using System.Text.Json;
using SylviaNG.Prescription.Application.Features.Templates.Models;
using SylviaNG.Prescription.Domain.Entities;

namespace SylviaNG.Prescription.Application.Mappings
{
    public static class TemplateMappings
    {
        private static readonly JsonSerializerOptions SerializerOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        /// <summary>Deserializes the entity's ConfigJson column into a TemplateConfig.</summary>
        public static TemplateConfig GetConfig(this PrescriptionTemplate template)
        {
            if (string.IsNullOrWhiteSpace(template.ConfigJson))
                return new TemplateConfig();

            return JsonSerializer.Deserialize<TemplateConfig>(template.ConfigJson, SerializerOptions)
                ?? new TemplateConfig();
        }

        /// <summary>Serializes a TemplateConfig into the entity's ConfigJson column.</summary>
        public static void SetConfig(this PrescriptionTemplate template, TemplateConfig config)
        {
            template.ConfigJson = JsonSerializer.Serialize(config, SerializerOptions);
        }

        public static TemplateSummaryResponse ToSummaryResponse(this PrescriptionTemplate template)
        {
            return new TemplateSummaryResponse
            {
                TemplateId = template.TemplateId,
                Name = template.Name,
                Type = template.Type,
                Language = template.Language,
                Enabled = template.Enabled,
                IsSystemDefault = template.IsSystemDefault
            };
        }

        public static TemplateDetailsResponse ToDetailsResponse(this PrescriptionTemplate template)
        {
            return new TemplateDetailsResponse
            {
                TemplateId = template.TemplateId,
                Name = template.Name,
                Type = template.Type,
                Language = template.Language,
                Enabled = template.Enabled,
                IsSystemDefault = template.IsSystemDefault,
                Config = template.GetConfig()
            };
        }
    }
}
