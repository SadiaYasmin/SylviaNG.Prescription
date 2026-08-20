using MediatR;
using SylviaNG.Prescription.Application.Features.Departments.Models;

namespace SylviaNG.Prescription.Application.Features.Departments.Commands.CreateDepartment
{
    public class CreateDepartmentCommand : IRequest<long>
    {
        public DepartmentCreateRequest Request { get; set; }
        public CreateDepartmentCommand(DepartmentCreateRequest request) => Request = request;
    }
}
