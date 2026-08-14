using MediatR;
using SylviaNG.Prescription.Application.Features.Patients.Models;

namespace SylviaNG.Prescription.Application.Features.Patients.Queries.GetDoctorPatientQueue
{
    public class GetDoctorPatientQueueQuery : IRequest<DoctorPatientQueueResponse>
    {
        public string KeycloakId { get; set; }
        public DoctorPatientQueueRequest Request { get; set; }

        public GetDoctorPatientQueueQuery(string keycloakId, DoctorPatientQueueRequest request)
        {
            KeycloakId = keycloakId;
            Request = request;
        }
    }
}
