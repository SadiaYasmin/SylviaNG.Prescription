using MediatR;

namespace SylviaNG.Prescription.Application.Features.Departments.Commands.DeactivateDepartment
{
    public class DeactivateDepartmentCommand : IRequest<Unit>
    {
        public long DepartmentId { get; set; }
        public DeactivateDepartmentCommand(long departmentId) => DepartmentId = departmentId;
    }
}
