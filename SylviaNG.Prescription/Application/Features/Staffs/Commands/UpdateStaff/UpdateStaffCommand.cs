using MediatR;
using SylviaNG.Prescription.Application.Features.Staffs.Models;

namespace SylviaNG.Prescription.Application.Features.Staffs.Commands.UpdateStaff
{
    public class UpdateStaffCommand : IRequest<StaffSummaryResponse>
    {
        public long StaffId { get; set; }
        public UpdateStaffRequest Request { get; set; }

        public UpdateStaffCommand(long staffId, UpdateStaffRequest request)
        {
            StaffId = staffId;
            Request = request;
        }
    }
}
