namespace SylviaNG.Prescription.Application.Features.Templates.Models
{
    /// <summary>US-047-049: Name plus the full, client-editable TemplateConfig.</summary>
    public class UpdateTemplateRequest
    {
        public string Name { get; set; } = string.Empty;
        public TemplateConfig Config { get; set; } = new();
    }
}
