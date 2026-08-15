using MediatR;

namespace SylviaNG.Prescription.Application.Features.QuickAdd.Queries.GetAdviceFollowUpPhraseDictionary
{
    /// <summary>US-043: the auto-translate lookup for the Advice/Follow-Up preset form.</summary>
    public class GetAdviceFollowUpPhraseDictionaryQuery : IRequest<Dictionary<string, string>>
    {
        public string KeycloakId { get; set; }

        public GetAdviceFollowUpPhraseDictionaryQuery(string keycloakId)
        {
            KeycloakId = keycloakId;
        }
    }
}
