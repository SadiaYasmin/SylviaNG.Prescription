using MediatR;
using SylviaNG.Prescription.Application.Features.Prescriptions.Models;

namespace SylviaNG.Prescription.Application.Features.Prescriptions.Queries.VerifyPrescription
{
    /// <summary>US-035: public, no auth. Looked up by DisplayCode (the id encoded in the QR/link), never a raw PK.</summary>
    public class VerifyPrescriptionQuery : IRequest<PrescriptionDocumentResponse>
    {
        public string DisplayCode { get; set; }

        public VerifyPrescriptionQuery(string displayCode)
        {
            DisplayCode = displayCode;
        }
    }
}
