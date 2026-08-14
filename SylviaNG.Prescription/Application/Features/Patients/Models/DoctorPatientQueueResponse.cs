namespace SylviaNG.Prescription.Application.Features.Patients.Models
{
    public class DoctorPatientQueueResponse
    {
        public List<DoctorPatientQueueItemResponse> Patients { get; set; } = new();
    }
}
