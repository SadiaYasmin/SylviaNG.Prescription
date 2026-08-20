using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SylviaNG.Prescription.Application.Features.Medicines.Models;
using SylviaNG.Prescription.Application.Interfaces.Repositories;
using SylviaNG.Prescription.Domain.Entities;
using SylviaNG.Prescription.SharedKernel.Generic;

namespace SylviaNG.Prescription.Application.Features.Medicines.Commands.ImportMedicines
{
    /// <summary>
    /// Idempotent CSV catalog importer (medicine-feature-brief.md §5). Upserts on
    /// BrandName+Strength+Manufacturer so re-running the same file updates rather than
    /// duplicates. Header names are matched flexibly (case/space/punctuation-insensitive,
    /// with a few synonyms per column) so a MedEx-derived export doesn't have to be
    /// hand-renamed to match the entity's exact property names first.
    /// </summary>
    public class ImportMedicinesHandler : IRequestHandler<ImportMedicinesCommand, MedicineImportResultResponse>
    {
        private const int BatchSize = 1000;

        private static readonly Dictionary<string, string[]> ColumnAliases = new()
        {
            ["BrandName"] = new[] { "brandname", "brand", "name", "drugname", "productname" },
            ["GenericName"] = new[] { "genericname", "generic" },
            ["Strength"] = new[] { "strength", "dose", "dosage" },
            ["DosageForm"] = new[] { "dosageform", "form", "dosagetype" },
            ["Route"] = new[] { "route" },
            ["Manufacturer"] = new[] { "manufacturer", "company", "manufacturername" },
            ["Category"] = new[] { "category", "drugclass", "class" },
            ["UnitPrice"] = new[] { "unitprice", "price", "mrp" },
            ["DgdaRegistered"] = new[] { "dgdaregistered", "dgda", "registered" },
        };

        private readonly IMedicineRepository _medicineRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ImportMedicinesHandler> _logger;

        public ImportMedicinesHandler(IMedicineRepository medicineRepository, IUnitOfWork unitOfWork, ILogger<ImportMedicinesHandler> logger)
        {
            _medicineRepository = medicineRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<MedicineImportResultResponse> Handle(ImportMedicinesCommand command, CancellationToken cancellationToken)
        {
            var result = new MedicineImportResultResponse();

            using var reader = new StreamReader(command.FileStream);
            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HeaderValidated = null,
                MissingFieldFound = null,
            });

            if (!await csv.ReadAsync())
            {
                result.Errors.Add("The file is empty.");
                return result;
            }
            csv.ReadHeader();
            if (csv.HeaderRecord == null)
            {
                result.Errors.Add("Could not read a header row from the file.");
                return result;
            }

            var columnMap = MapColumns(csv.HeaderRecord);
            if (!columnMap.ContainsKey("BrandName"))
            {
                result.Errors.Add("Could not find a Brand Name column (expected one of: BrandName, Brand, Name, Drug Name, Product Name).");
                return result;
            }

            var existingByKey = (await _medicineRepository.Query().ToListAsync(cancellationToken))
                .ToDictionary(m => NaturalKey(m.BrandName, m.Strength, m.Manufacturer), m => m);

            var toAdd = new List<Medicine>();
            var rowNumber = 1; // header is row 1
            var unflushedCount = 0;

            while (await csv.ReadAsync())
            {
                rowNumber++;
                result.RowsRead++;

                var brandName = Field(csv, columnMap, "BrandName")?.Trim();
                if (string.IsNullOrWhiteSpace(brandName))
                {
                    result.Skipped++;
                    result.Errors.Add($"Row {rowNumber}: skipped, missing Brand Name.");
                    continue;
                }

                var genericName = NullIfEmpty(Field(csv, columnMap, "GenericName"));
                var strength = NullIfEmpty(Field(csv, columnMap, "Strength"));
                var dosageForm = NullIfEmpty(Field(csv, columnMap, "DosageForm"));
                var route = NullIfEmpty(Field(csv, columnMap, "Route"));
                var manufacturer = NullIfEmpty(Field(csv, columnMap, "Manufacturer"));
                var category = NullIfEmpty(Field(csv, columnMap, "Category"));

                decimal? unitPrice = null;
                var unitPriceRaw = Field(csv, columnMap, "UnitPrice")?.Trim();
                if (!string.IsNullOrEmpty(unitPriceRaw) && !decimal.TryParse(unitPriceRaw, NumberStyles.Number, CultureInfo.InvariantCulture, out var parsedPrice))
                {
                    result.Errors.Add($"Row {rowNumber}: unrecognized Unit Price \"{unitPriceRaw}\" — left blank.");
                }
                else if (!string.IsNullOrEmpty(unitPriceRaw))
                {
                    unitPrice = decimal.Parse(unitPriceRaw, NumberStyles.Number, CultureInfo.InvariantCulture);
                }

                var dgdaRegistered = ParseBool(Field(csv, columnMap, "DgdaRegistered"));

                var key = NaturalKey(brandName, strength, manufacturer);
                if (existingByKey.TryGetValue(key, out var existing))
                {
                    existing.GenericName = genericName;
                    existing.Strength = strength;
                    existing.DosageForm = dosageForm;
                    existing.Route = route;
                    existing.Manufacturer = manufacturer;
                    existing.Category = category;
                    existing.UnitPrice = unitPrice;
                    existing.DgdaRegistered = dgdaRegistered;

                    if (existing.MedicineId != 0)
                    {
                        // A genuinely pre-existing (already persisted) row.
                        _medicineRepository.Update(existing);
                        result.Updated++;
                    }
                    // else: a later duplicate row in this same file, colliding with an
                    // entity that's still sitting unsaved in `toAdd` — just overwrite its
                    // fields in place (already counted as Inserted) rather than calling
                    // Update() on an entity EF has never tracked/saved yet, which throws.
                }
                else
                {
                    var medicine = new Medicine
                    {
                        BrandName = brandName,
                        GenericName = genericName,
                        Strength = strength,
                        DosageForm = dosageForm,
                        Route = route,
                        Manufacturer = manufacturer,
                        Category = category,
                        UnitPrice = unitPrice,
                        DgdaRegistered = dgdaRegistered,
                        Active = true,
                    };
                    existingByKey[key] = medicine; // a later duplicate row in this same file updates it in place, not a second insert
                    toAdd.Add(medicine);
                    result.Inserted++;
                }

                if (++unflushedCount >= BatchSize)
                {
                    if (toAdd.Count > 0)
                    {
                        await _medicineRepository.AddRangeAsync(toAdd);
                        toAdd.Clear();
                    }
                    await _unitOfWork.SaveChangesAsync();
                    unflushedCount = 0;
                }
            }

            if (toAdd.Count > 0)
            {
                await _medicineRepository.AddRangeAsync(toAdd);
            }
            if (unflushedCount > 0 || toAdd.Count > 0)
            {
                await _unitOfWork.SaveChangesAsync();
            }

            _logger.LogInformation(
                "Medicine CSV import: {Read} read, {Inserted} inserted, {Updated} updated, {Skipped} skipped.",
                result.RowsRead, result.Inserted, result.Updated, result.Skipped);

            return result;
        }

        private static Dictionary<string, int> MapColumns(string[] headerRecord)
        {
            var map = new Dictionary<string, int>();
            for (var i = 0; i < headerRecord.Length; i++)
            {
                var normalized = NormalizeHeader(headerRecord[i]);
                foreach (var (canonical, aliases) in ColumnAliases)
                {
                    if (map.ContainsKey(canonical)) continue;
                    if (aliases.Contains(normalized)) map[canonical] = i;
                }
            }
            return map;
        }

        private static string NormalizeHeader(string header) =>
            new string(header.Where(char.IsLetterOrDigit).ToArray()).ToLowerInvariant();

        private static string? Field(CsvReader csv, Dictionary<string, int> columnMap, string canonical) =>
            columnMap.TryGetValue(canonical, out var index) ? csv.GetField(index) : null;

        private static string? NullIfEmpty(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

        private static bool ParseBool(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return false;
            var v = value.Trim().ToLowerInvariant();
            return v is "1" or "true" or "yes" or "y";
        }

        private static string NaturalKey(string? brandName, string? strength, string? manufacturer) =>
            string.Join("|", Normalize(brandName), Normalize(strength), Normalize(manufacturer));

        private static string Normalize(string? value) => (value ?? string.Empty).Trim().ToLowerInvariant();
    }
}
