using MediatR;
using SylviaNG.Prescription.Application.Features.Departments.Models;
using SylviaNG.Prescription.Application.Interfaces.Services;

namespace SylviaNG.Prescription.Application.Features.Departments.Queries.GetDepartmentList
{
    public class GetDepartmentListHandler : IRequestHandler<GetDepartmentListQuery, List<DepartmentResponse>>
    {
        private readonly IDepartmentService _service;
        public GetDepartmentListHandler(IDepartmentService service) => _service = service;

        public async Task<List<DepartmentResponse>> Handle(GetDepartmentListQuery query, CancellationToken cancellationToken)
            => await _service.GetAllAsync();
    }
}
