using MediatR;
using SylviaNG.Prescription.Application.Features.Doctors.Models;

namespace SylviaNG.Prescription.Application.Features.Doctors.Queries.GetDoctorDetails
{
    public class GetDoctorDetailsQuery : IRequest<DoctorDetailsResponse>
    {
        public long DoctorId { get; set; }

        public GetDoctorDetailsQuery(long doctorId)
        {
            DoctorId = doctorId;
        }
    }
}
