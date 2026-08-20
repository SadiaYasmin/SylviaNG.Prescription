namespace SylviaNG.Prescription.Application.Features.Medicines.Models
{
    /// <summary>Summary log for a CSV catalog import — see <c>ImportMedicinesHandler</c> for the upsert rule.</summary>
    public class MedicineImportResultResponse
    {
        public int RowsRead { get; set; }
        public int Inserted { get; set; }
        public int Updated { get; set; }
        public int Skipped { get; set; }
        public List<string> Errors { get; set; } = new();
    }
}
