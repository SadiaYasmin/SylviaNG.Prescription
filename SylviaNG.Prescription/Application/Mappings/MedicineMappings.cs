using SylviaNG.Prescription.Application.Features.Medicines.Models;
using SylviaNG.Prescription.Domain.Entities;

namespace SylviaNG.Prescription.Application.Mappings
{
    public static class MedicineMappings
    {
        public static MedicineSummaryResponse ToSummaryResponse(this Medicine medicine) => new()
        {
            MedicineId = medicine.MedicineId,
            BrandName = medicine.BrandName,
            GenericName = medicine.GenericName,
            Strength = medicine.Strength,
            DosageForm = medicine.DosageForm,
            Category = medicine.Category
        };

        public static MedicineCatalogResponse ToCatalogResponse(this Medicine medicine, int totalPrescribed) => new()
        {
            MedicineId = medicine.MedicineId,
            BrandName = medicine.BrandName,
            GenericName = medicine.GenericName,
            Strength = medicine.Strength,
            Manufacturer = medicine.Manufacturer,
            DosageForm = medicine.DosageForm,
            Category = medicine.Category,
            Active = medicine.Active,
            TotalPrescribed = totalPrescribed
        };
    }
}
