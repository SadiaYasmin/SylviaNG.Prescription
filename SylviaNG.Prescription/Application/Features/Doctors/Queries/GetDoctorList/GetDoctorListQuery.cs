using MediatR;
using SylviaNG.Prescription.Application.Features.Doctors.Models;

namespace SylviaNG.Prescription.Application.Features.Doctors.Queries.GetDoctorList
{
    public class GetDoctorListQuery : IRequest<DoctorListResponse>
    {
        public DoctorListRequest Request { get; set; }

        public GetDoctorListQuery(DoctorListRequest request)
        {
            Request = request;
        }
    }
}
