using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SylviaNG.Prescription.Application.Common;
using SylviaNG.Prescription.Application.Common.StaticData;
using SylviaNG.Prescription.Application.Interfaces.Repositories;
using SylviaNG.Prescription.Domain.Enums;

namespace SylviaNG.Prescription.Application.Features.QuickAdd.Queries.GetAdviceFollowUpPhraseDictionary
{
    public class GetAdviceFollowUpPhraseDictionaryHandler : IRequestHandler<GetAdviceFollowUpPhraseDictionaryQuery, Dictionary<string, string>>
    {
        private readonly IUserRepository _userRepository;
        private readonly IStaffRepository _staffRepository;
        private readonly IDoctorRepository _doctorRepository;
        private readonly IQuickAddPresetRepository _quickAddPresetRepository;

        public GetAdviceFollowUpPhraseDictionaryHandler(
            IUserRepository userRepository,
            IStaffRepository staffRepository,
            IDoctorRepository doctorRepository,
            IQuickAddPresetRepository quickAddPresetRepository)
        {
            _userRepository = userRepository;
            _staffRepository = staffRepository;
            _doctorRepository = doctorRepository;
            _quickAddPresetRepository = quickAddPresetRepository;
        }

        private class BilingualPayload
        {
            public string? En { get; set; }
            public string? Bn { get; set; }
        }

        public async Task<Dictionary<string, string>> Handle(GetAdviceFollowUpPhraseDictionaryQuery query, CancellationToken cancellationToken)
        {
            var caller = await CallerContextResolver.ResolveCallerAsync(
                query.KeycloakId, _userRepository, _staffRepository, _doctorRepository);
            var doctorId = caller.DoctorId!.Value;

            // Start from the standard dictionary, then let the doctor's own previously-saved
            // pairs override any entry with the same English key (US-043: "the standard
            // preset dictionary plus the doctor's own previously-saved English->Bangla pairs").
            var dictionary = new Dictionary<string, string>(AdviceFollowUpPhraseDictionary.Entries);

            var ownPresets = await _quickAddPresetRepository.Query()
                .Where(p => p.DoctorId == doctorId && (p.SectionType == QuickAddSectionTypeEnum.Advice || p.SectionType == QuickAddSectionTypeEnum.FollowUp))
                .ToListAsync(cancellationToken);

            foreach (var preset in ownPresets)
            {
                BilingualPayload? payload;
                try
                {
                    payload = JsonSerializer.Deserialize<BilingualPayload>(preset.PayloadJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }
                catch (JsonException)
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(payload?.En) || string.IsNullOrWhiteSpace(payload.Bn))
                    continue;

                dictionary[payload.En.Trim().ToLowerInvariant()] = payload.Bn;
            }

            return dictionary;
        }
    }
}
