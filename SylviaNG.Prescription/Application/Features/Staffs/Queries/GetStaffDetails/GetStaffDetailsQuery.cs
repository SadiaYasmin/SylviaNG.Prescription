using MediatR;
using SylviaNG.Prescription.Application.Features.Staffs.Models;

namespace SylviaNG.Prescription.Application.Features.Staffs.Queries.GetStaffDetails
{
    public class GetStaffDetailsQuery : IRequest<StaffDetailsResponse>
    {
        public long StaffId { get; set; }

        public GetStaffDetailsQuery(long staffId)
        {
            StaffId = staffId;
        }
    }
}
