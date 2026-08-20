using Microsoft.Extensions.Logging;
using SylviaNG.Prescription.Application.Interfaces.Repositories;
using SylviaNG.Prescription.Domain.Entities;
using SylviaNG.Prescription.SharedKernel.Generic;

namespace SylviaNG.Prescription.Application.Services
{
    /// <summary>
    /// Seeds a small starter Medicine catalog (Epic F stub) on startup, mirroring
    /// <see cref="TemplateEngineSeeder"/>'s idempotent shape (skip if any row already
    /// exists) so the Prescription authoring medicine autocomplete (US-022) is usable
    /// out of the box without Epic F's admin CRUD existing yet.
    /// </summary>
    public class MedicineCatalogSeeder
    {
        private readonly IMedicineRepository _medicineRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<MedicineCatalogSeeder> _logger;

        public MedicineCatalogSeeder(
            IMedicineRepository medicineRepository,
            IUnitOfWork unitOfWork,
            ILogger<MedicineCatalogSeeder> logger)
        {
            _medicineRepository = medicineRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        private static readonly (string Brand, string Generic, string Strength, string Form, string Category)[] Starters =
        {
            ("Napa", "Paracetamol", "500mg", "Tablet", "Analgesic"),
            ("Napa Extra", "Paracetamol + Caffeine", "500mg+65mg", "Tablet", "Analgesic"),
            ("Fexo", "Fexofenadine", "120mg", "Tablet", "Antihistamine"),
            ("Seclo", "Omeprazole", "20mg", "Capsule", "Antacid"),
            ("Losectil", "Omeprazole", "20mg", "Capsule", "Antacid"),
            ("Amodis", "Metronidazole", "400mg", "Tablet", "Antibiotic"),
            ("Ace", "Paracetamol", "500mg", "Tablet", "Analgesic"),
            ("Monas", "Montelukast", "10mg", "Tablet", "Anti-asthmatic"),
            ("Ceevit", "Vitamin C", "250mg", "Tablet", "Vitamin"),
            ("Antac", "Ranitidine", "150mg", "Tablet", "Antacid"),
            ("Flagyl", "Metronidazole", "400mg", "Tablet", "Antibiotic"),
            ("Azithro", "Azithromycin", "500mg", "Tablet", "Antibiotic"),
            ("Napa Syrup", "Paracetamol", "120mg/5ml", "Syrup", "Analgesic"),
            ("Sergel", "Esomeprazole", "20mg", "Capsule", "Antacid"),
            ("Rivotril", "Clonazepam", "0.5mg", "Tablet", "Anxiolytic"),
            ("Histacin", "Chlorpheniramine", "4mg", "Tablet", "Antihistamine"),
            ("Ecosprin", "Aspirin", "75mg", "Tablet", "Antiplatelet"),
            ("Insulin Mixtard", "Human Insulin", "100IU/ml", "Injection", "Antidiabetic"),
            ("Metfor", "Metformin", "500mg", "Tablet", "Antidiabetic"),
            ("Amdocal", "Amlodipine", "5mg", "Tablet", "Antihypertensive"),
        };

        public async Task SeedAsync()
        {
            if ((await _medicineRepository.GetAllAsync()).Any())
                return;

            foreach (var (brand, generic, strength, form, category) in Starters)
            {
                await _medicineRepository.AddAsync(new Medicine
                {
                    BrandName = brand,
                    GenericName = generic,
                    Strength = strength,
                    DosageForm = form,
                    Category = category,
                    // Real, currently-marketed Bangladesh drugs — registered so the default
                    // DGDA-only search filter finds them without an admin having to flip it.
                    DgdaRegistered = true,
                    Active = true
                });
            }

            await _unitOfWork.SaveChangesAsync();
            _logger.LogInformation("Seeded {Count} starter medicine catalog entries.", Starters.Length);
        }
    }
}
