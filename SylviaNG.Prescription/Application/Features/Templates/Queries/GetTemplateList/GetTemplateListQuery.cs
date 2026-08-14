using MediatR;
using SylviaNG.Prescription.Application.Features.Templates.Models;

namespace SylviaNG.Prescription.Application.Features.Templates.Queries.GetTemplateList
{
    public class GetTemplateListQuery : IRequest<TemplateListResponse>
    {
        /// <summary>
        /// Default false preserves Admin's existing "see everything, including disabled"
        /// management view. A Doctor picking a preferred template (Epic D/K stub) passes
        /// true so disabled templates never show up as choosable.
        /// </summary>
        public bool EnabledOnly { get; set; }

        public GetTemplateListQuery() { }

        public GetTemplateListQuery(bool enabledOnly)
        {
            EnabledOnly = enabledOnly;
        }
    }
}
