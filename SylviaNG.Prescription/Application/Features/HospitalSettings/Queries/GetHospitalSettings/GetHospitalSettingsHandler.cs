using MediatR;
using Microsoft.EntityFrameworkCore;
using SylviaNG.Prescription.Application.Common.Exceptions;
using SylviaNG.Prescription.Application.Features.HospitalSettings.Models;
using SylviaNG.Prescription.Application.Interfaces.Repositories;
using SylviaNG.Prescription.Application.Mappings;

namespace SylviaNG.Prescription.Application.Features.HospitalSettings.Queries.GetHospitalSettings
{
    /// <summary>
    /// US-045: single-record semantics. Deliberately dumb — does not create-on-read; the one
    /// row this queries for is guaranteed to exist by <c>TemplateEngineSeeder</c> at startup,
    /// so a genuinely missing row here is a real error, not a case to paper over.
    /// </summary>
    public class GetHospitalSettingsHandler : IRequestHandler<GetHospitalSettingsQuery, HospitalSettingsResponse>
    {
        // Note: this handler's enclosing namespace (Application.Features.HospitalSettings.*)
        // shadows the bare "HospitalSettings" entity type name (Domain.Entities.HospitalSettings),
        // so the fetched entity below is always referenced via `var`, never spelled out literally.
        private readonly IHospitalSettingsRepository _hospitalSettingsRepository;

        public GetHospitalSettingsHandler(IHospitalSettingsRepository hospitalSettingsRepository)
        {
            _hospitalSettingsRepository = hospitalSettingsRepository;
        }

        public async Task<HospitalSettingsResponse> Handle(GetHospitalSettingsQuery query, CancellationToken cancellationToken)
        {
            var settings = await _hospitalSettingsRepository.Query()
                .FirstOrDefaultAsync(cancellationToken)
                ?? throw new NotFoundException("HospitalSettings not found. The startup seeder should have created it.");

            return settings.ToResponse();
        }
    }
}
