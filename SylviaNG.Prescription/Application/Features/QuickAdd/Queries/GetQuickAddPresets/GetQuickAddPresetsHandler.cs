using MediatR;
using Microsoft.EntityFrameworkCore;
using SylviaNG.Prescription.Application.Common;
using SylviaNG.Prescription.Application.Features.QuickAdd.Models;
using SylviaNG.Prescription.Application.Interfaces.Repositories;
using SylviaNG.Prescription.Application.Mappings;

namespace SylviaNG.Prescription.Application.Features.QuickAdd.Queries.GetQuickAddPresets
{
    public class GetQuickAddPresetsHandler : IRequestHandler<GetQuickAddPresetsQuery, List<QuickAddPresetResponse>>
    {
        private readonly IUserRepository _userRepository;
        private readonly IStaffRepository _staffRepository;
        private readonly IDoctorRepository _doctorRepository;
        private readonly IQuickAddPresetRepository _quickAddPresetRepository;

        public GetQuickAddPresetsHandler(
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

        public async Task<List<QuickAddPresetResponse>> Handle(GetQuickAddPresetsQuery query, CancellationToken cancellationToken)
        {
            var caller = await CallerContextResolver.ResolveCallerAsync(
                query.KeycloakId, _userRepository, _staffRepository, _doctorRepository);
            var doctorId = caller.DoctorId!.Value;

            var presets = await _quickAddPresetRepository.Query()
                .Where(p => p.DoctorId == doctorId && p.SectionType == query.SectionType)
                .OrderBy(p => p.Label)
                .ToListAsync(cancellationToken);

            return presets.Select(p => p.ToResponse()).ToList();
        }
    }
}
