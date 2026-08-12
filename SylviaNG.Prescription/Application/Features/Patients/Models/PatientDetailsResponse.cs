namespace SylviaNG.Prescription.Application.Features.Patients.Models
{
    public class PatientDetailsResponse
    {
        public PatientSummaryResponse Profile { get; set; } = new();
    }
}
