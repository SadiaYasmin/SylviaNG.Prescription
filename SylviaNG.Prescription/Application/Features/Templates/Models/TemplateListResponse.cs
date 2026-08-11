namespace SylviaNG.Prescription.Application.Features.Templates.Models
{
    /// <summary>Flat, unpaginated list — templates are few (US-050).</summary>
    public class TemplateListResponse
    {
        public List<TemplateSummaryResponse> Templates { get; set; } = new();
    }
}
