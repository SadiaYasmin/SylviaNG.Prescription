using MediatR;
using SylviaNG.Prescription.Application.Interfaces.Services;

namespace SylviaNG.Prescription.Application.Features.Departments.Commands.CreateDepartment
{
    public class CreateDepartmentHandler : IRequestHandler<CreateDepartmentCommand, long>
    {
        private readonly IDepartmentService _service;
        public CreateDepartmentHandler(IDepartmentService service) => _service = service;

        public async Task<long> Handle(CreateDepartmentCommand command, CancellationToken cancellationToken)
            => await _service.CreateAsync(command.Request);
    }
}
