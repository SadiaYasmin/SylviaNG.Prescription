using MediatR;
using SylviaNG.Prescription.Application.Features.Departments.Models;

namespace SylviaNG.Prescription.Application.Features.Departments.Commands.UpdateDepartment
{
    public class UpdateDepartmentCommand : IRequest<Unit>
    {
        public long DepartmentId { get; set; }
        public DepartmentUpdateRequest Request { get; set; }
        public UpdateDepartmentCommand(long departmentId, DepartmentUpdateRequest request)
        {
            DepartmentId = departmentId;
            Request = request;
        }
    }
}
