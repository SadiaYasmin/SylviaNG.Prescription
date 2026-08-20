using MediatR;
using SylviaNG.Prescription.Application.Features.Departments.Models;

namespace SylviaNG.Prescription.Application.Features.Departments.Queries.GetDepartmentList
{
    public class GetDepartmentListQuery : IRequest<List<DepartmentResponse>>
    {
    }
}
