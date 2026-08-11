using Microsoft.Extensions.Logging;
using SylviaNG.Prescription.Application.Features.Templates;
using SylviaNG.Prescription.Application.Interfaces.Repositories;
using SylviaNG.Prescription.Application.Mappings;
using SylviaNG.Prescription.Domain.Entities;
using SylviaNG.Prescription.Domain.Enums;
using SylviaNG.Prescription.SharedKernel.Generic;

namespace SylviaNG.Prescription.Application.Services
{
    /// <summary>
    /// Seeds the Epic H template engine baseline on startup, mirroring <see cref="AdminAccountSeeder"/>'s
    /// shape/DI lifetime. Idempotent: skips whichever half already has data, so it's safe to run on
    /// every startup.
    /// - Templates: seeds the three baseline templates (Classic system-default, Corporate,
    ///   Government) if none exist, so a fresh install ships with a ready-to-use template set.
    /// - HospitalSettings: seeds one blank row if none exists (its existence is what
    ///   GetHospitalSettingsHandler relies on instead of create-on-read).
    /// </summary>
    public class TemplateEngineSeeder
    {
        private readonly ITemplateRepository _templateRepository;
        private readonly IHospitalSettingsRepository _hospitalSettingsRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<TemplateEngineSeeder> _logger;

        public TemplateEngineSeeder(
            ITemplateRepository templateRepository,
            IHospitalSettingsRepository hospitalSettingsRepository,
            IUnitOfWork unitOfWork,
            ILogger<TemplateEngineSeeder> logger)
        {
            _templateRepository = templateRepository;
            _hospitalSettingsRepository = hospitalSettingsRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task SeedAsync()
        {
            var seededSomething = false;

            if (!(await _templateRepository.GetAllAsync()).Any())
            {
                foreach (var (name, type, isSystemDefault) in new[]
                {
                    ("Classic Bangladesh Chamber", TemplateTypeEnum.Classic, true),
                    ("Modern Corporate Hospital", TemplateTypeEnum.Corporate, false),
                    ("Government Hospital", TemplateTypeEnum.Government, false)
                })
                {
                    var template = new PrescriptionTemplate
                    {
                        Name = name,
                        Type = type,
                        Language = TemplateLanguageEnum.En,
                        Enabled = true,
                        IsSystemDefault = isSystemDefault
                    };
                    template.SetConfig(TemplateDefaults.BuildDefaultConfig(type, TemplateLanguageEnum.En));

                    await _templateRepository.AddAsync(template);
                }

                _logger.LogInformation("Seeded 3 baseline prescription templates (Classic system default, Corporate, Government).");
                seededSomething = true;
            }

            if (!(await _hospitalSettingsRepository.GetAllAsync()).Any())
            {
                var settings = new HospitalSettings
                {
                    Name = string.Empty,
                    Address = string.Empty,
                    Phone = string.Empty
                };

                await _hospitalSettingsRepository.AddAsync(settings);
                _logger.LogInformation("Seeded blank HospitalSettings row.");
                seededSomething = true;
            }

            if (seededSomething)
            {
                await _unitOfWork.SaveChangesAsync();
            }
        }
    }
}
