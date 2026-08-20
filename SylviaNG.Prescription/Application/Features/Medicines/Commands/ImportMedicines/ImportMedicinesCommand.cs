using MediatR;
using SylviaNG.Prescription.Application.Features.Medicines.Models;

namespace SylviaNG.Prescription.Application.Features.Medicines.Commands.ImportMedicines
{
    public class ImportMedicinesCommand : IRequest<MedicineImportResultResponse>
    {
        public Stream FileStream { get; set; }

        public ImportMedicinesCommand(Stream fileStream)
        {
            FileStream = fileStream;
        }
    }
}
