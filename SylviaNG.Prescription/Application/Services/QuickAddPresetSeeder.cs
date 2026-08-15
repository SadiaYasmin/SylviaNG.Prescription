using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SylviaNG.Prescription.Application.Interfaces.Repositories;
using SylviaNG.Prescription.Domain.Entities;
using SylviaNG.Prescription.Domain.Enums;
using SylviaNG.Prescription.SharedKernel.Generic;
using SylviaNG.Prescription.SharedKernel.Utils;

namespace SylviaNG.Prescription.Application.Services
{
    /// <summary>
    /// US-044: seeds a small starter Quick Add set for a doctor's given section, the first
    /// time that section's list is opened (called from GetQuickAddPresetsHandler, not at
    /// app startup — seeding is scoped per doctor+section and can't be predicted ahead of a
    /// doctor even existing). Idempotent per doctor+section via
    /// <see cref="DoctorQuickAddSeedState"/>: once a seed-state row exists, seeding never
    /// runs again for that doctor+section, regardless of the section's current row count —
    /// this is what keeps a doctor's deliberately-emptied list from being silently refilled.
    /// </summary>
    public class QuickAddPresetSeeder
    {
        private readonly IQuickAddPresetRepository _quickAddPresetRepository;
        private readonly IUnitOfWork _unitOfWork;

        public QuickAddPresetSeeder(IQuickAddPresetRepository quickAddPresetRepository, IUnitOfWork unitOfWork)
        {
            _quickAddPresetRepository = quickAddPresetRepository;
            _unitOfWork = unitOfWork;
        }

        private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        // Shares its literal starter phrases with AdviceFollowUpPhraseDictionary's static
        // entries for Advice/FollowUp, so the seed set and the auto-translate dictionary
        // don't drift as two independently-maintained lists.
        private static readonly Dictionary<QuickAddSectionTypeEnum, (string Label, object Payload)[]> Starters = new()
        {
            [QuickAddSectionTypeEnum.Medicine] =
            [
                ("Napa 500", new { medicine = "Napa", strength = "500mg", dosage = "1 tablet", frequency = "TDS", duration = "5 days", instructions = "After meal" }),
                ("Seclo 20", new { medicine = "Seclo", strength = "20mg", dosage = "1 capsule", frequency = "OD", duration = "7 days", instructions = "Before breakfast" }),
                ("Fexo 120", new { medicine = "Fexo", strength = "120mg", dosage = "1 tablet", frequency = "OD", duration = "5 days", instructions = "At night" }),
            ],
            // Shape matches PrescriptionContent's DiagnosisItem{Text, Icd10}, not a plain
            // string — unlike C/C, H/O, and Investigation, Diagnosis in a real prescription
            // is a structured list item, so its preset payload must match that shape.
            [QuickAddSectionTypeEnum.Diagnosis] =
            [
                ("Viral Fever", new { text = "Viral Fever", icd10 = (string?)null }),
                ("Acute Gastritis", new { text = "Acute Gastritis", icd10 = (string?)null }),
                ("Upper Respiratory Tract Infection", new { text = "Upper Respiratory Tract Infection", icd10 = (string?)"J06.9" }),
            ],
            [QuickAddSectionTypeEnum.Investigation] =
            [
                ("CBC", "CBC"),
                ("Blood Sugar (Random)", "Blood Sugar (Random)"),
                ("Chest X-Ray", "Chest X-Ray"),
            ],
            [QuickAddSectionTypeEnum.Advice] =
            [
                ("Plenty of fluids", new { en = "Drink plenty of fluids.", bn = "প্রচুর পরিমাণে তরল পান করুন।" }),
                ("Adequate rest", new { en = "Take adequate rest.", bn = "পর্যাপ্ত বিশ্রাম নিন।" }),
                ("Avoid oily food", new { en = "Avoid oily and spicy food.", bn = "তৈলাক্ত ও মশলাযুক্ত খাবার এড়িয়ে চলুন।" }),
            ],
            [QuickAddSectionTypeEnum.FollowUp] =
            [
                ("Review in 7 days", new { en = "Review after 7 days.", bn = "৭ দিন পর পুনরায় দেখাবেন।" }),
                ("Follow up if not improved", new { en = "Follow up if symptoms don't improve.", bn = "উপসর্গ না কমলে পুনরায় দেখাবেন।" }),
            ],
        };

        public async Task SeedIfNeededAsync(long doctorId, QuickAddSectionTypeEnum sectionType, CancellationToken cancellationToken)
        {
            var alreadySeeded = await _unitOfWork.Context.DoctorQuickAddSeedStates
                .AnyAsync(s => s.DoctorId == doctorId && s.SectionType == sectionType, cancellationToken);
            if (alreadySeeded) return;

            foreach (var (label, payload) in Starters[sectionType])
            {
                await _quickAddPresetRepository.AddAsync(new QuickAddPreset
                {
                    DoctorId = doctorId,
                    SectionType = sectionType,
                    Label = label,
                    PayloadJson = JsonSerializer.Serialize(payload, JsonOptions)
                });
            }

            await _unitOfWork.Context.DoctorQuickAddSeedStates.AddAsync(
                new DoctorQuickAddSeedState { DoctorId = doctorId, SectionType = sectionType, SeededAt = DateTimeUtility.NowUtc() },
                cancellationToken);

            await _unitOfWork.SaveChangesAsync();
        }
    }
}
