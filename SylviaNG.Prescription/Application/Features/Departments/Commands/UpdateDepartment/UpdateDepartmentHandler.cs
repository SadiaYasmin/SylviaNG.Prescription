using MediatR;
using SylviaNG.Prescription.Application.Interfaces.Services;

namespace SylviaNG.Prescription.Application.Features.Departments.Commands.UpdateDepartment
{
    public class UpdateDepartmentHandler : IRequestHandler<UpdateDepartmentCommand, Unit>
    {
        private readonly IDepartmentService _service;
        public UpdateDepartmentHandler(IDepartmentService service) => _service = service;

        public async Task<Unit> Handle(UpdateDepartmentCommand command, CancellationToken cancellationToken)
        {
            await _service.UpdateAsync(command.DepartmentId, command.Request);
            return Unit.Value;
        }
    }
}
