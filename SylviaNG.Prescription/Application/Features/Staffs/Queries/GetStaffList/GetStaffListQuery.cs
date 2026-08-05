using MediatR;
using SylviaNG.Prescription.Application.Features.Staffs.Models;

namespace SylviaNG.Prescription.Application.Features.Staffs.Queries.GetStaffList
{
    public class GetStaffListQuery : IRequest<StaffListResponse>
    {
        public StaffListRequest Request { get; set; }

        public GetStaffListQuery(StaffListRequest request)
        {
            Request = request;
        }
    }
}
