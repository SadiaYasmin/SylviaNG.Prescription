using MediatR;
using Microsoft.EntityFrameworkCore;
using SylviaNG.Prescription.Application.Common;
using SylviaNG.Prescription.Application.Features.QuickAdd.Models;
using SylviaNG.Prescription.Application.Interfaces.Repositories;
using SylviaNG.Prescription.Application.Mappings;
using SylviaNG.Prescription.Application.Services;

namespace SylviaNG.Prescription.Application.Features.QuickAdd.Queries.GetQuickAddPresets
{
    public class GetQuickAddPresetsHandler : IRequestHandler<GetQuickAddPresetsQuery, List<QuickAddPresetResponse>>
    {
        private readonly IUserRepository _userRepository;
        private readonly IStaffRepository _staffRepository;
        private readonly IDoctorRepository _doctorRepository;
        private readonly IQuickAddPresetRepository _quickAddPresetRepository;
        private readonly QuickAddPresetSeeder _seeder;

        public GetQuickAddPresetsHandler(
            IUserRepository userRepository,
            IStaffRepository staffRepository,
            IDoctorRepository doctorRepository,
            IQuickAddPresetRepository quickAddPresetRepository,
            QuickAddPresetSeeder seeder)
        {
            _userRepository = userRepository;
            _staffRepository = staffRepository;
            _doctorRepository = doctorRepository;
            _quickAddPresetRepository = quickAddPresetRepository;
            _seeder = seeder;
        }

        public async Task<List<QuickAddPresetResponse>> Handle(GetQuickAddPresetsQuery query, CancellationToken cancellationToken)
        {
            var caller = await CallerContextResolver.ResolveCallerAsync(
                query.KeycloakId, _userRepository, _staffRepository, _doctorRepository);
            var doctorId = caller.DoctorId!.Value;

            // US-044: self-triggering starter seed on first-ever open of this doctor's
            // section — no-ops after the first call (see QuickAddPresetSeeder).
            await _seeder.SeedIfNeededAsync(doctorId, query.SectionType, cancellationToken);

            var presets = await _quickAddPresetRepository.Query()
                .Where(p => p.DoctorId == doctorId && p.SectionType == query.SectionType)
                .OrderBy(p => p.Label)
                .ToListAsync(cancellationToken);

            return presets.Select(p => p.ToResponse()).ToList();
        }
    }
}
